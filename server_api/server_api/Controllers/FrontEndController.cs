using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http;


namespace server_api.Controllers
{
    public class FrontEndController : ApiController
    {

        // ~~~~~ GET ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        /// <summary>
        ///   This is a testing method. 
        ///   
        ///   This method returns all registered users' email addresses.
        /// </summary>
        /// <returns></returns>
        [Route("api/frontend/registeredUsers")]
        [HttpGet]
        public IEnumerable<string> GetAllRegisteredUsers()
        {
            var db = new AirUDatabaseCOE();

            List<User> allUsers = db.Users.Select(x => x).ToList<User>();
            List<string> allUsersString = new List<string>();
 
            foreach(var item in allUsers)
            {
                allUsersString.Add(item.Email);
            }

            return allUsersString;
        }

        [Route("api/frontend/AMSDataPointsPoints")]
        [HttpPost]
        public HttpResponseMessage GetAllDataPointsForAMS([FromBody]string deviceID)
        {
            // Does not actually pull in the deviceID
            var db = new AirUDatabaseCOE();
            List<Pollutant> pollutants = db.Pollutants.Select(x => x).ToList<Pollutant>();
            string pName = pollutants[0].PollutantName;

            AllAMSDataPoints dataPoints = new AllAMSDataPoints();

            StringBuilder msg = new StringBuilder();
            msg.Append("[");

            foreach (Pollutant p in pollutants)
            {
                
                var amsDataForPollutant = from a in db.Devices_States_and_Datapoints
                                          where a.DeviceID == deviceID
                                          && a.PollutantName == p.PollutantName
                                          orderby a.MeasurementTime
                                          select a;

                /* MOVE ALTITUDE TO STATE */
                if (amsDataForPollutant.Count() != 0 || p.PollutantName.Equals("Altitude"))
                {
                    msg.Append("{\"key\": \"");
                    msg.Append(p.PollutantName);
                    msg.Append("\", \"values\": [");

                    AMSPollutant curPollutantDPs = new AMSPollutant(p.PollutantName);



                    foreach (var item in amsDataForPollutant)
                    {
                        msg.Append("[");
                        msg.Append(ConvertDateTimeToMilliseconds(item.MeasurementTime));
                        msg.Append(", ");
                        msg.Append(item.Value);

                        curPollutantDPs.AddValue(item.MeasurementTime, item.Value);

                        msg.Append("], ");
                    }

                    msg.Remove(msg.Length - 2, 2);

                    msg.Append("]}, ");

                    //msg.ToString().Substring(0, msg.Length - 2);
                    dataPoints.pollutantList.Add(curPollutantDPs);
                }
                
            }

            msg.Remove(msg.Length - 2, 2);

            msg.Append("]");


            var message = Request.CreateResponse(HttpStatusCode.OK);

            //string json = JsonConvert.SerializeObject(dataPoints);

            message.Content = new StringContent(msg.ToString());


            return message;
            //return allDataPointsString;
        }

        /// <summary>
        ///   This is a testing method.
        ///   
        ///   Returns user with the specific email address if it exists. Else, returns 'not found'.
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        [Route("api/frontend")]
        [HttpGet]
        public HttpResponseMessage GetRegisteredUser([FromBody]string email)
        {
            //var db = new AirU_Database_Entity();
            var db = new AirUDatabaseCOE();

            User registeredUser = db.Users.SingleOrDefault(x => x.Email == email);

            if (registeredUser != null)
            {
                // User with email address: <email> does exsit.
                return Request.CreateResponse<string>(HttpStatusCode.OK, "User with email address: = " + registeredUser.Email + " does exist.");
            }
            else
            {
                // User with email address: <email> does not exist.
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Item not found.");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Route("api/ams/AMSData/latest")]
        [HttpGet]
        public HttpResponseMessage GetLatestMapValues()
        {
            LinkedList<Devices_States_and_Datapoints> results = new LinkedList<Devices_States_and_Datapoints>();

            // DEFAULT VALUES
            DateTime measurementTimeMax = DateTime.Now;
            int inOrOut = 0; // 0 = Outside, 1 = Inside
            int statePrivacy = 0; // 0 = Not Private, 1 = Private
            

            // SHOULD BE VARIABLE
            int latMin = -180;
            int latMax = 180;
            int longMin = -180;
            int longMax = 180;
            String pollutantName = "Temperature";

            // CURRENTLY NOT USED
            String deviceID = "12-34-56-78-9A-BC";
            DateTime measurementTimeMin = measurementTimeMax.AddHours(-12);


            //SqlConnection conn = new SqlConnection(@"Data Source=(LocalDB)\v11.0;AttachDbFilename=C:\Users\Zach\Documents\AirU.mdf;Integrated Security=True;Connect Timeout=30");
            SqlConnection conn = new SqlConnection(@"Data Source=mssql.eng.utah.edu;Initial Catalog=lobato;Persist Security Info=True;User ID=lobato;Password=eVHDpynh;MultipleActiveResultSets=True;Application Name=EntityFramework");
            //SqlConnection conn = new SqlConnection(@"Data Source=mssql.eng.utah.edu;Initial Catalog=lobato;Persist Security Info=True;User ID=lobato;PASSWORD=eVHDpynh;MultipleActiveResultSets=True;Application Name=EntityFramework");

            using (SqlConnection myConnection = conn)
            {
                string oString = @"select Devices_States_and_Datapoints.*
                                    from
                                    Devices_States_and_Datapoints
                                    right join
                                    (
	                                    select DeviceID, max(MeasurementTime) as MeasurementTime
	                                    from Devices_States_and_Datapoints
	                                    where MeasurementTime < @measurementTimeMax
	                                    and Lat >= @latMin
	                                    and Lat <= @latMax
	                                    and Long >= @longMin
	                                    and Long < @longMax
	                                    and InOrOut = @inOrOut
	                                    and StatePrivacy = @statePrivacy
	                                    and PollutantName = @pollutantName
	                                    group by DeviceID
                                    ) as LatestValues
                                    on Devices_States_and_Datapoints.MeasurementTime=LatestValues.MeasurementTime
                                    and Devices_States_and_Datapoints.DeviceID = LatestValues.DeviceID;";
                SqlCommand oCmd = new SqlCommand(oString, myConnection);


                String time = DateTime.Now.ToString("G");
                oCmd.Parameters.AddWithValue("@deviceID", deviceID);
                oCmd.Parameters.AddWithValue("@measurementTimeMax", measurementTimeMax);
                oCmd.Parameters.AddWithValue("@measurementTimeMin", measurementTimeMin);
                oCmd.Parameters.AddWithValue("@latMin", latMin);
                oCmd.Parameters.AddWithValue("@latMax", latMax);
                oCmd.Parameters.AddWithValue("@longMin", longMin);
                oCmd.Parameters.AddWithValue("@longMax", longMax);
                oCmd.Parameters.AddWithValue("@inOrOut", inOrOut);
                oCmd.Parameters.AddWithValue("@statePrivacy", statePrivacy);
                oCmd.Parameters.AddWithValue("@pollutantName", pollutantName);
                //oCmd.Parameters.AddWithValue("@deviceID", "12-34-56-78-9A-BC");

                myConnection.Open();
                using (SqlDataReader oReader = oCmd.ExecuteReader())
                {
                    while (oReader.Read())
                    {
                        Devices_States_and_Datapoints result = new Devices_States_and_Datapoints();
                        result.DeviceID = oReader["DeviceID"].ToString();
                        result.StateTime = (DateTime)oReader["StateTime"];
                        result.MeasurementTime = (DateTime)oReader["MeasurementTime"];
                        result.Lat = (decimal)oReader["Lat"];
                        result.Long = (decimal)oReader["Long"];
                        result.InOrOut = (bool)oReader["InOrOut"];
                        result.StatePrivacy = (bool)oReader["StatePrivacy"];
                        result.Value = (double)oReader["Value"];
                        result.PollutantName = oReader["PollutantName"].ToString();
                        results.AddLast(result);
                    }

                    myConnection.Close();
                }
            }


            Console.WriteLine("Hello");

            var message = Request.CreateResponse(HttpStatusCode.OK);
            return message;

        }



        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Route("api/ams/AMSData")]
        [HttpGet]
        public HttpResponseMessage GetAMSData()
        {
            var db = new AirUDatabaseCOE();

            // TODO: Add API database calls to retrive data and return to frontend.

            var message = Request.CreateResponse(HttpStatusCode.OK);

            return message;

        }

        // ~~~~~ POST ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        /// <summary>
        /// 
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [Route("api/frontend/register")]
        [HttpPost]
        public HttpResponseMessage UserRegistration([FromBody]User user)
        {
            var db = new AirUDatabaseCOE();

            User existingUser = db.Users.SingleOrDefault(x => x.Email == user.Email);

            var message = Request.CreateResponse(HttpStatusCode.OK);

            if (existingUser == null)
            {
                // Perform queries to insert new user into database.
                User newUser = new User();
                newUser.Email = user.Email;
                newUser.Pass = user.Pass;

                db.Users.Add(newUser);
                db.SaveChanges();

                // Account register success.
                message.Content = new StringContent("Account registration successful! Welcome, " + user.Email);
            }
            else
            {
                // Account register failed. Account with email address: '<user.Email>' already exists. Please try a different email address.
                message.Content = new StringContent("Account registration failed! Account with email address: " + user.Email + " already exists. Please try a different email address.");
            }

            return message;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [Route("api/frontend/login")]
        [HttpPost]
        public HttpResponseMessage UserLogin([FromBody]User user)
        {
            var db = new AirUDatabaseCOE();

            User validUserAndPass = db.Users.SingleOrDefault(x => x.Email == user.Email && x.Pass == user.Pass);

            var message = Request.CreateResponse(HttpStatusCode.OK);

            if (validUserAndPass != null)
            {
                // Login success.
                message.Content = new StringContent("Login Successful! Welcome, " + user.Email);
            }
            else
            {
                // Login fail.
                message.Content = new StringContent("Login failed! Please check email and password.");
            }

            return message;
        }

        /// <summary>
        ///   Registers an AMS device.
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        [Route("api/ams/add")]
        [HttpPost]
        public HttpResponseMessage AddAMSDevice([FromBody]DeviceAndState newDeviceAndState)
        {
            var db = new AirUDatabaseCOE();

            Device existingDevice = db.Devices.SingleOrDefault(x => x.DeviceID == newDeviceAndState.device.DeviceID);

            var message = Request.CreateResponse(HttpStatusCode.OK);

            if (existingDevice != null)
            {
                // Add device success.
                db.Devices.Add(newDeviceAndState.device);
                newDeviceAndState.state.StateTime = new DateTime(1900, 1, 1);
                newDeviceAndState.state.Long = 360.0m;
                newDeviceAndState.state.Lat = 360.0m;
                db.DeviceStates.Add(newDeviceAndState.state);
                message.Content = new StringContent("Successfully added device: \n\tDeviceID = " + newDeviceAndState.device.DeviceID +
                                                                               "\n\tDevicePrivacy = " + newDeviceAndState.device.DevicePrivacy +
                                                                               "\n\tEmail = " + newDeviceAndState.device.Email);
            }
            else
            {
                // Add device fail.
                message.Content = new StringContent("Adding device with DeviceID = " + newDeviceAndState.device.DeviceID + " was unsuccessful!");
            }

            db.SaveChanges();

            return message;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="value"></param>
        //[Route("api/ams")]
        //[HttpPut]
        //public void Put(int id, [FromBody]string value)
        //{
       
        //}

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="id"></param>
        //[Route("api/ams")]
        //[HttpDelete]
        //public void Delete(int id)
        //{
          
        //}


        public long ConvertDateTimeToMilliseconds(DateTime date)
        {
            return (long)(date - new DateTime(1970, 1, 1)).TotalMilliseconds;
        }

        public class DeviceAndState
        {
            public Device device {get; set;}
            public DeviceState state {get; set;}
        }

        /// <summary>
        /// 
        /// </summary>
        public class AllAMSDataPoints
        {
            public List<AMSPollutant> pollutantList {get; set;}

            public AllAMSDataPoints()
            {
                pollutantList = new List<AMSPollutant>();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public class AMSPollutant
        {
            public string key {get; set;}
            public List<Tuple<long, double>> values;

            public AMSPollutant(string Key)
            {
                key = Key;
                values = new List<Tuple<long, double>>();
            }

            public void AddValue(DateTime date, double value){
                values.Add(new Tuple<long, double>((long)(date - new DateTime(1970, 1, 1)).TotalMilliseconds, value));
            }
        }
    }
}

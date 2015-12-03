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
using Swashbuckle.Swagger.Annotations;
using System.Web.Http.Description;
using server_api.Models;


namespace server_api.Controllers
{
    /// <summary>
    /// The API Controller that interacts with the Web App
    /// </summary>
    public class FrontEndController : ApiController
    {
        /*
         * + = Working as expected
         * - = Needs work
         * 
         * // Testing Methods
         * + ServerTest() - This method simply returns successful.
         * + ServerAndDatabaseTest() - This method performs a database query and returns result.
         * 
         * // Map View
         * + GetAllDataPointsForDevice() - Returns all datapoints for a Device given a DeviceID.
         * 
         * // Heat Map View
         * 
         * 
         * // Device Compare View
         * + GetAllDataPointsForDevice() - Returns all datapoints for a Device given a DeviceID.
         * 
         * // Device Registration View
         * + DeviceRegistration() - Registers an AMS device.
         * 
         * // Device Settings View
         * + GetUsersDeviceStates() - Returns the set of DeviceStates associated with the given user email.
         * 
         * // User Registration View
         * + UserRegistration() - Validates user is not already in database and if not, creates new User in database.
         * 
         * // User Login View
         * + UserLogin() - Validates user based on Email and Pass.
         * 
         * 
         */

        // ~~~~~ GET ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        /// <summary>
        ///   This is a testing method. 
        ///   
        ///   This method simply returns successful.
        /// </summary>
        /// <returns></returns>
        [Route("frontend/servertest")]
        [HttpGet]
        public HttpResponseMessage ServerTest()
        {
            var message = Request.CreateResponse(HttpStatusCode.OK);
            message.Content = new StringContent("Success");
            return message;
        }

        /// <summary>
        ///   This is a testing method. 
        ///   
        ///   This method returns all registered users' email addresses.
        /// </summary>
        /// <returns></returns>
        [ResponseType(typeof(IEnumerable<SwaggerUser>))]
        [Route("frontend/registeredUsers")]
        [HttpGet]
        public HttpResponseMessage ServerAndDatabaseTest()
        {
            var db = new AirUDatabaseCOE();

            List<User> allUsers = db.Users.Select(x => x).ToList<User>();

            if(allUsers != null)
            {

                List<SwaggerUser> swaggerUsers = new List<SwaggerUser>();

                foreach (var item in allUsers)
                {
                    swaggerUsers.Add(new SwaggerUser(item.Email));
                }

                string json = JsonConvert.SerializeObject(swaggerUsers);

                return Request.CreateResponse<string>(HttpStatusCode.OK, json);
            }
            else
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "No users found!");
            }
        }

        /// <summary>
        ///   Returns all datapoints for a Device given a DeviceID.
        /// 
        ///   Primary Use: Compare View and single AMS device Map View "data graph"
        /// </summary>
        /// <param name="deviceID"></param>
        /// <returns></returns>
        [ResponseType(typeof(IEnumerable<SwaggerPollutantList>))]
        [Route("frontend/deviceDatapoints")]
        [HttpPost]
        public HttpResponseMessage GetAllDataPointsForDevice([FromBody]string deviceID)
        {
            var db = new AirUDatabaseCOE();
            List<Pollutant> pollutants = db.Pollutants.Select(x => x).ToList<Pollutant>();

            List<SwaggerPollutantList> data = new List<SwaggerPollutantList>();
            
            StringBuilder msg = new StringBuilder();

            foreach (Pollutant p in pollutants)
            {
                var amsDataForPollutant = from a in db.Devices_States_and_Datapoints
                                          where a.DeviceID == deviceID
                                          && a.PollutantName == p.PollutantName
                                          orderby a.MeasurementTime
                                          select a;

                /* MOVE ALTITUDE TO STATE */
                if (amsDataForPollutant.Count() != 0 && !p.PollutantName.Equals("Altitude"))
                {
                    SwaggerPollutantList pl = new SwaggerPollutantList(p.PollutantName);
                    
                    foreach (var item in amsDataForPollutant)
                    {
                        pl.values.Add(new object[2]);
                        pl.values.Last()[0] = ConvertDateTimeToMilliseconds(item.MeasurementTime);
                        pl.values.Last()[1] = (decimal)item.Value;

                    }
                    data.Add(pl);
                }
            }

            string json = JsonConvert.SerializeObject(data);

            var message = Request.CreateResponse(HttpStatusCode.OK);

            message.Content = new StringContent(json);

            return message;
        }

        // ~~~~~ POST ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        /// <summary>
        ///   Validates user is not already in database and if not, creates new User in database.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [Route("frontend/registerUser")]
        [HttpPost]
        public HttpResponseMessage UserRegistration([FromBody]User user)
        {
            var db = new AirUDatabaseCOE();

            User existingUser = db.Users.SingleOrDefault(x => x.Email == user.Email);

            if (existingUser == null)
            {
                // Perform queries to insert new user into database.
                User newUser = new User();
                newUser.Email = user.Email;
                newUser.Pass = user.Pass;

                db.Users.Add(newUser);
                db.SaveChanges();

                // Account register success.
                return Request.CreateResponse<string>(HttpStatusCode.OK, "Account registration successful! Welcome, " + user.Email);
            }
            else
            {
                // Account register failed. Account with email address: '<user.Email>' already exists. Please try a different email address.
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Account registration failed! Account with email address: " + 
                                                                             user.Email + 
                                                                             " already exists. Please try a different email address.");
            }
        }

        /// <summary>
        ///   Validates user based on Email and Pass.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [Route("frontend/login")]
        [HttpPost]
        public HttpResponseMessage UserLogin([FromBody]User user)
        {
            var db = new AirUDatabaseCOE();

            User validUserAndPass = db.Users.SingleOrDefault(x => x.Email == user.Email && x.Pass == user.Pass);

            if (validUserAndPass != null)
            {
                // Login success.
                return Request.CreateResponse<string>(HttpStatusCode.OK, "Login Successful! Welcome, " + user.Email);
            }
            else
            {
                // Login fail.
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Login failed! Please check email and password.");
            }
        }

        /// <summary>
        /// Registers an AMS device:
        /// - Validates request
        /// - Updates Database to represent new association between existing user and 
        ///    new device.
        /// </summary>
        /// <param name="newDeviceAndState">The current Device and its DeviceState</param>
        /// <returns></returns>
        [Route("frontend/registerDevice")]
        [HttpPost]
        public HttpResponseMessage DeviceRegistration([FromBody]DeviceAndState newDeviceAndState)
        {
            var db = new AirUDatabaseCOE();

            Device existingDevice = db.Devices.SingleOrDefault(x => x.DeviceID == newDeviceAndState.device.DeviceID);

            if (existingDevice != null)
            {
                // Add device success.
                db.Devices.Add(newDeviceAndState.device);
                newDeviceAndState.state.StateTime = new DateTime(1900, 1, 1);
                newDeviceAndState.state.Long = 360.0m;
                newDeviceAndState.state.Lat = 360.0m;
                db.DeviceStates.Add(newDeviceAndState.state);
                db.SaveChanges();
                return Request.CreateResponse<string>(HttpStatusCode.OK, "Successfully added device: \n\tDeviceID = " + 
                                                                         newDeviceAndState.device.DeviceID +
                                                                         "\n\tDevicePrivacy = " + newDeviceAndState.device.DevicePrivacy +
                                                                         "\n\tEmail = " + newDeviceAndState.device.Email);
                
            }
            else
            {
                // Add device fail.
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Adding device with DeviceID = " + 
                                                                            newDeviceAndState.device.DeviceID + 
                                                                            " was unsuccessful!");
            }
        }

        /// <summary>
        ///   Returns the set of DeviceStates associated with the given user email.
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        [Route("frontend/getUsersDeviceStates")]
        [HttpPost]
        public HttpResponseMessage GetUsersDeviceStates([FromBody]string email)
        {
            var db = new AirUDatabaseCOE();

            // Validate given email has associated User.
            User registeredUser = db.Users.SingleOrDefault(x => x.Email == email);

            if (registeredUser != null)
            {
                // Perform database query to retrive the most recent AMS DeviceStates for each AMS owned by User.
                var deviceStates = from device in db.Devices
                                   where device.Email == email
                                   join state in db.DeviceStates
                                   on device.DeviceID equals state.DeviceID
                                   select state;

                // TODO - Send user list of AMS DeviceState. 
                // JsonConvert.SerializeObject(deviceStates) - does not work, circular reference

                return Request.CreateResponse<string>(HttpStatusCode.OK, "Item found!");
            }
            else
            {
                // User with email address: <email> does not exist.
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Item not found.");
            }
        }

        /// <summary>
        /// Updates a single AMS DeviceState from the "my devices" settings web page.
        /// </summary>
        /// <param name="state">The state of the device</param>
        /// <returns></returns>
        [Route("frontend/state")]
        [HttpPost]
        public HttpResponseMessage UpdateAMSDeviceState([FromBody]DeviceState state)
        {
            var db = new AirUDatabaseCOE();
            
            // Validate Device from given DeviceId exists.
            Device registeredDevice = db.Devices.SingleOrDefault(x => x.DeviceID == state.DeviceID);

            if (registeredDevice != null)
            {
                // Request previous state from database based on state.DeviceID
                DeviceState previousState = (
                                    from device in db.DeviceStates
                                    where device.DeviceID == state.DeviceID
                                    && device.StateTime <= state.StateTime // should be measurementtime
                                    group device by device.DeviceID into deviceIDGroup
                                    select new
                                    {
                                        DeviceID = deviceIDGroup.Key,
                                        MaxMeasurementTime = deviceIDGroup.Max(device => device.StateTime)
                                    } into MaxStates
                                    join coordinates in db.DeviceStates
                                                            on MaxStates.MaxMeasurementTime equals coordinates.StateTime into latestStateGroup
                                    select latestStateGroup.FirstOrDefault()).Single();

                // Inherit lat and long from previous state
                decimal prevLong= previousState.Long;
                decimal prevLat = previousState.Lat;
                
                // TODO
                // Save new state to database

                // Send user newly updated state back to user

                var message = Request.CreateResponse(HttpStatusCode.OK);
                return message;
            }
            else
            {
                // Device with DeviceID: <deviceID> does not exist.
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Item not found.");
            }
        }

        /// <summary>
        ///   Returns the latest AMS Device States and Datapoints based on a specific min lat and long, and max lat and long. 
        ///   
        ///   Primary Use: Heatmap.
        /// </summary>
        /// <returns></returns>
        [Route("ams/latest")]
        [HttpPost]
        public HttpResponseMessage GetLatestMapValues([FromBody]HeatMapParameters para)
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
        ///   Returns the AMS DeviceStates for all AMS devices within specified MapParameters.
        ///   
        ///   Primary Use: Populate the Map View with AMS device icons. 
        /// </summary>
        /// <param name="para">The NE and SW bounds of a map</param>
        /// <returns></returns>
        [ResponseType(typeof(IEnumerable<SwaggerAMSList>))]
        [Route("ams/map")]
        [HttpPost]
        public HttpResponseMessage GetAllAMSDevicesInMapRange([FromBody]MapParameters para)
        {
            // SHOULD BE VARIABLE
            decimal latMin = para.southWest.lat;
            decimal latMax = para.northEast.lat;
            decimal longMin = para.southWest.lng;
            decimal longMax = para.northEast.lng;

            var db = new AirUDatabaseCOE();

            var results = from state in db.DeviceStates
                          where
                          state.Lat > latMin
                          && state.Lat < latMax
                          && state.Long > longMin
                          && state.Long < longMax
                          && state.StatePrivacy == false // Can create add in Spring
                          && state.InOrOut == false // Can create add in Spring
                          group state by state.DeviceID into deviceIDGroup
                          select new
                          {
                              MaxStateTime = deviceIDGroup.Max(device => device.StateTime)
                          } into MaxStates
                          join coordinates in db.DeviceStates
                          on MaxStates.MaxStateTime equals coordinates.StateTime into latestStateGroup
                          select latestStateGroup.FirstOrDefault();

            SwaggerAMSList amses = new SwaggerAMSList();

            foreach (DeviceState d in results)
            {
                amses.AddSwaggerDevice(d.DeviceID, d.Lat, d.Long);
            }

            var message = Request.CreateResponse(HttpStatusCode.OK);
            string json = JsonConvert.SerializeObject(amses);
            message.Content = new StringContent(json);

            return message;
        }
        
        /// <summary>
        ///   Returns the AMS DeviceStates for all AMS devices within specified HeatMapParameters.
        ///   
        ///   Primary Use: Populate the HeatMap View with AMS device icons. 
        /// </summary>
        /// <param name="para">The NE and SW bounds of a map and name of requested Pollutant</param>
        /// <returns></returns>
        [Route("ams/heatmap")]
        [HttpPost]
        public HttpResponseMessage GetAllAMSDeviceStatesInMapRange([FromBody]HeatMapParameters para)
        {
            LinkedList<Devices_States_and_Datapoints> results = new LinkedList<Devices_States_and_Datapoints>();

            // DEFAULT VALUES
            DateTime measurementTimeMax = DateTime.Now;
            int inOrOut = 0; // 0 = Outside, 1 = Inside
            int statePrivacy = 0; // 0 = Not Private, 1 = Private

            // SHOULD BE VARIABLE
            decimal latMin = para.mapParameters.southWest.lat;
            decimal latMax = para.mapParameters.northEast.lat;
            decimal longMin = para.mapParameters.southWest.lng;
            decimal longMax = para.mapParameters.northEast.lng;

            // CURRENTLY NOT USED
            /*
            String deviceID = "12-34-56-78-9A-BC";
            DateTime measurementTimeMin = measurementTimeMax.AddHours(-12);
            */

            
            SqlConnection conn = new SqlConnection(@"Data Source=mssql.eng.utah.edu;Initial Catalog=lobato;Persist Security Info=True;User ID=lobato;Password=eVHDpynh;MultipleActiveResultSets=True;Application Name=EntityFramework");
            

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
	                                    group by DeviceID
                                    ) as LatestValues
                                    on Devices_States_and_Datapoints.MeasurementTime=LatestValues.MeasurementTime
                                    and Devices_States_and_Datapoints.DeviceID = LatestValues.DeviceID;";
                SqlCommand oCmd = new SqlCommand(oString, myConnection);

                String time = DateTime.Now.ToString("G");
                //oCmd.Parameters.AddWithValue("@deviceID", deviceID);
                oCmd.Parameters.AddWithValue("@measurementTimeMax", measurementTimeMax);
                //oCmd.Parameters.AddWithValue("@measurementTimeMin", measurementTimeMin);
                oCmd.Parameters.AddWithValue("@latMin", latMin);
                oCmd.Parameters.AddWithValue("@latMax", latMax);
                oCmd.Parameters.AddWithValue("@longMin", longMin);
                oCmd.Parameters.AddWithValue("@longMax", longMax);
                oCmd.Parameters.AddWithValue("@inOrOut", inOrOut);
                oCmd.Parameters.AddWithValue("@statePrivacy", statePrivacy);
                //oCmd.Parameters.AddWithValue("@pollutantName", pollutantName);
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
        ///   Returns the latest datapoints for a single AMS device based on specified DeviceID. 
        ///   
        ///   Primary Use: "details" panel on Map View after selecting AMS device on map. 
        /// </summary>
        /// <param name="DeviceID"></param>
        /// <returns></returns>
        [ResponseType(typeof(IEnumerable<SwaggerLatestPollutantsList>))]
        [Route("frontend/singleLatest")]
        [HttpPost]
        public HttpResponseMessage GetLatestDataFromSingleAMSDevice([FromBody]string deviceID)
        {
            var db = new AirUDatabaseCOE();

            // Validate DeviceID represents an actual AMS device.
            Device registeredDevice = db.Devices.SingleOrDefault(x => x.DeviceID == deviceID);
            if (registeredDevice != null)
            {
                // Performs database query to obtain the latest Datapoints for specific DeviceID.
                SqlConnection conn = new SqlConnection(@"Data Source=mssql.eng.utah.edu;Initial Catalog=lobato;Persist Security Info=True;User ID=lobato;Password=eVHDpynh;MultipleActiveResultSets=True;Application Name=EntityFramework");
                LinkedList<Devices_States_and_Datapoints> results = new LinkedList<Devices_States_and_Datapoints>();
                using (SqlConnection myConnection = conn)
                {
                    string oString =   @"select Devices_States_and_DataPoints.DeviceID,
		                                        Devices_States_and_DataPoints.StateTime,
		                                        Devices_States_and_DataPoints.MeasurementTime,
		                                        Devices_States_and_DataPoints.Lat,
		                                        Devices_States_and_DataPoints.Long,
		                                        Devices_States_and_DataPoints.InOrOut,
		                                        Devices_States_and_DataPoints.StatePrivacy,
		                                        Devices_States_and_DataPoints.Value,
		                                        Devices_States_and_DataPoints.PollutantName
                                        from(select DeviceID, Max(MeasurementTime) as MaxMeasurementTime, PollutantName
	                                        from (select MaxStates.DeviceID, MaxStates.MaxStateTime, MeasurementTime, PollutantName
			                                        from (select DeviceID, Max(StateTime) as MaxStateTime
					                                        from DeviceStates
					                                        where DeviceID=@deviceID
					                                        group by DeviceID) as MaxStates
			                                        left join Devices_States_and_DataPoints
			                                        on MaxStates.DeviceID = Devices_States_and_DataPoints.DeviceID
			                                        and MaxStates.MaxStateTime = Devices_States_and_DataPoints.StateTime) as MaxStatesAndMeasurementTime
	                                        group by DeviceID, PollutantName) as MaxMeasurementTimeForPollutants
                                        left join Devices_States_and_DataPoints
			                                        on MaxMeasurementTimeForPollutants.DeviceID = Devices_States_and_DataPoints.DeviceID
			                                        and MaxMeasurementTimeForPollutants.PollutantName = Devices_States_and_DataPoints.PollutantName
			                                        and MaxMeasurementTimeForPollutants.MaxMeasurementTime = Devices_States_and_DataPoints.MeasurementTime";
                    SqlCommand oCmd = new SqlCommand(oString, myConnection);
                    oCmd.Parameters.AddWithValue("@deviceID", deviceID);
                    
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

                SwaggerLatestPollutantsList latestPollutants = new SwaggerLatestPollutantsList();

                foreach (Devices_States_and_Datapoints result in results)
                {
                    latestPollutants.AddPollutantAndValue(result.PollutantName, result.Value);
                }

                string json = JsonConvert.SerializeObject(latestPollutants);
                var message = Request.CreateResponse(HttpStatusCode.OK);
                message.Content = new StringContent(json);
                return message;
            }
            else
            {
                // Device with DeviceID: <deviceID> does not exist.
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Item not found.");
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="value"></param>
        [Route("ams")]
        [HttpPut]
        public void Put(int id, [FromBody]string value)
        {

        }

        /*
        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="id"></param>
        [Route("ams")]
        [HttpDelete]
        public void Delete(int id)
        {

        }
        */

        /// <summary>
        /// Converts DateTime to compatible JS time in Milliseconds
        /// </summary>
        /// <param name="date">the date to be converted</param>
        /// <returns>date in milliseconds since January 1st, 1970</returns>
        public long ConvertDateTimeToMilliseconds(DateTime date)
        {
            return (long)(date - new DateTime(1970, 1, 1)).TotalMilliseconds;
        }

        /// <summary>
        /// This class represents both a Device and DeviceState object.
        /// </summary>
        public class DeviceAndState
        {
            /// <summary>
            /// The Device
            /// </summary>
            public Device device {get; set;}

            /// <summary>
            /// The DeviceState
            /// </summary>
            public DeviceState state {get; set;}
        }

        /// <summary>
        /// This class stores both the NE and SW bounds sent from a
        /// map view.
        /// </summary>
        public class MapParameters
        {
            /// <summary>
            /// NE coordinates
            /// </summary>
            public Coordinate northEast { get; set; }

            /// <summary>
            /// SW coordinates
            /// </summary>
            public Coordinate southWest { get; set; }
        }

        /// <summary>
        /// This class represents a coordinate, which contains both a
        /// latitude and longitude;
        /// </summary>
        public class Coordinate
        {
            /// <summary>
            /// Latitude
            /// </summary>
            public decimal lat { get; set; }

            /// <summary>
            /// Longitude
            /// </summary>
            public decimal lng { get; set; }
        }

        /// <summary>
        /// This stores both the NE and SW bounds sent from a 
        /// map view, and combines them with the name of a pollutant.
        /// </summary>
        public class HeatMapParameters
        {
            /// <summary>
            /// NE and SW bound
            /// </summary>
            public MapParameters mapParameters {get; set;}

            /// <summary>
            /// Pollutant Name
            /// </summary>
            public string pollutantName {get; set;}
        }

        /// <summary>
        /// This class contains time/value data for each of an AMSes pollutants
        /// </summary>
        public class AMSPollutantsData
        {
            /// <summary>
            /// 
            /// </summary>
            public string key {get; set;}

            /// <summary>
            /// List of PollutantDataSets for AMS
            /// </summary>
            public List<PollutantDataSet> values {get; set;}

            /// <summary>
            /// Constructor - Initializes the AMS PollutantsData
            /// </summary>
            public AMSPollutantsData(string pollutant)
            {
                key = pollutant;
                values = new List<PollutantDataSet>();
            }
        }

        /// <summary>
        /// This class represents a pollutant and a set of times and values
        /// that may be used to store the data of a pollutant over time.
        /// </summary>
        public class PollutantDataSet
        {
            /// <summary>
            /// Pollutant Name
            /// </summary>
            public string key {get; set;}

            /// <summary>
            /// List of a pair of DateTimes (long - represented in milliseconds
            /// since 1970) and values (doubles)
            /// </summary>
            public List<Tuple<long, double>> points;

            /// <summary>
            /// Constructor - Creates a new instance with the key (Pollutant Name)
            /// set to the argument provided.
            /// </summary>
            /// <param name="key">Pollutant Name</param>
            public PollutantDataSet(string key)
            {
                this.key = key;
                points = new List<Tuple<long, double>>();
            }

            /// <summary>
            /// Adds a pair, a date and a value, to the list of dates and values
            /// 
            /// </summary>
            /// <param name="date">The time the value was measured for the point</param>
            /// <param name="value">The measured value for the point</param>
            public void AddValue(DateTime date, double value){
                points.Add(new Tuple<long, double>((long)(date - new DateTime(1970, 1, 1)).TotalMilliseconds, value));
            }
        }
    }
}

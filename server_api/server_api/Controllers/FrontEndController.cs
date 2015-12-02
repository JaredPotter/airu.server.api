﻿using Newtonsoft.Json;
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

        // ~~~~~ GET ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        /// <summary>
        ///   This is a testing method. 
        ///   
        ///   This method simply returns successful.
        /// </summary>
        /// <returns></returns>
        [Route("frontend/servertest")]
        [HttpGet]
        public HttpResponseMessage ServerResponding()
        {
            var message = Request.CreateResponse(HttpStatusCode.OK);
            message.Content = new StringContent("Success");

            SwaggerAMSList test = new SwaggerAMSList();
            test.AddSwaggerDevice("device1", 1, 2);
            test.AddSwaggerDevice("device2", 3, 4);
            test.AddSwaggerDevice("device3", 5, 6);

            string json = JsonConvert.SerializeObject(test);
            message.Content = new StringContent(json);
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
        public HttpResponseMessage GetAllRegisteredUsers()
        {
            var db = new AirUDatabaseCOE();

            List<User> allUsers = db.Users.Select(x => x).ToList<User>();
            List<SwaggerUser> swaggerUsers = new List<SwaggerUser>();

            foreach (var item in allUsers)
            {
                swaggerUsers.Add(new SwaggerUser(item.Email));
            }

            var message = Request.CreateResponse(HttpStatusCode.OK);

            string json = JsonConvert.SerializeObject(swaggerUsers);

            message.Content = new StringContent(json);

            return message;
        }

        /// <summary>
        /// 
        /// 
        ///   Primary Use: Compare View and single AMS device Map View "data graph"
        /// </summary>
        /// <param name="deviceID"></param>
        /// <returns></returns>
        [ResponseType(typeof(IEnumerable<SwaggerPollutantList>))]
        [Route("frontend/AMSDataPoints")]
        [HttpPost]
        public HttpResponseMessage GetAllDataPointsForAMS([FromBody]string deviceID)
        {
            // Does not actually pull in the deviceID
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
                if (amsDataForPollutant.Count() != 0 || p.PollutantName.Equals("Altitude"))
                {
                    SwaggerPollutantList pl = new SwaggerPollutantList(p.PollutantName);
                    pl.values.Add(new object[2]);

                    foreach (var item in amsDataForPollutant)
                    {
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

        /// <summary>
        ///   This is a testing method.
        ///   
        ///   Returns user with the specific email address if it exists. Else, returns 'not found'.
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        [Route("frontend")]
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
        [Route("ams/AMSData")]
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
        [Route("frontend/register")]
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
        [Route("frontend/login")]
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
        /// Registers an AMS device:
        /// - Validates request
        /// - Updates Database to represent new association between existing user and 
        ///    new device.
        /// </summary>
        /// <param name="newDeviceAndState">The current Device and its DeviceState</param>
        /// <returns></returns>
        [Route("ams/add")]
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
        ///   Returns the set of DeviceStates associated with the given email.
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        [Route("frontend/state")]
        [HttpPost]
        public HttpResponseMessage GetAMSDeviceStates([FromBody]string email)
        {
            var db = new AirUDatabaseCOE();

            // Validate given email has associated User.
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


            // Perform database query to retrive the most recent AMS DeviceStates for each AMS owned by User.


            // Send user list of AMS DeviceState. 

            //var message = Request.CreateResponse(HttpStatusCode.OK);
            //return message;
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

            // Request previous state from database based on state.DeviceID.

            // Inherit lat and long from previous state.

            // Save new state to database. 

            // Send user newly updated state back to user.

            var message = Request.CreateResponse(HttpStatusCode.OK);
            return message;
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
        [Route("ams/map")]
        [HttpPost]
        public HttpResponseMessage GetAllAMSDevicesInMapRange([FromBody]MapParameters para)
        {
            LinkedList<Devices_States_and_Datapoints> results = new LinkedList<Devices_States_and_Datapoints>();

            // DEFAULT VALUES
            DateTime measurementTimeMax = DateTime.Now;
            int inOrOut = 0; // 0 = Outside, 1 = Inside
            int statePrivacy = 0; // 0 = Not Private, 1 = Private

            // SHOULD BE VARIABLE
            decimal latMin = para.southWest.lat;
            decimal latMax = para.northEast.lat;
            decimal longMin = para.southWest.lng;
            decimal longMax = para.northEast.lng;

            SqlConnection conn = new SqlConnection(@"Data Source=mssql.eng.utah.edu;Initial Catalog=lobato;Persist Security Info=True;User ID=lobato;Password=eVHDpynh;MultipleActiveResultSets=True;Application Name=EntityFramework");

            SwaggerAMSList amses = new SwaggerAMSList();

            using (SqlConnection myConnection = conn)
            {
                string oString = @"select MaxState.DeviceID, MaxState.StateTime, DeviceStates.Lat, DeviceStates.Long
                                    from 
                                    (
	                                    select DeviceID, max(StateTime) as StateTime
	                                    from Devices_States_and_Datapoints
	                                    where 
	                                    Lat >= @latMin
                                        and Lat < @latMax
                                        and Long >= @longMin
                                        and Long < @longMax
                                        and InOrOut = @inOrOut
                                        and StatePrivacy = @statePrivacy
	                                    group by DeviceID
                                    ) as MaxState
                                    left join DeviceStates
                                    on MaxState.DeviceID=DeviceStates.DeviceID
                                    and MaxState.StateTime=DeviceStates.StateTime;";
                SqlCommand oCmd = new SqlCommand(oString, myConnection);

                String time = DateTime.Now.ToString("G");
                oCmd.Parameters.AddWithValue("@latMin", latMin);
                oCmd.Parameters.AddWithValue("@latMax", latMax);
                oCmd.Parameters.AddWithValue("@longMin", longMin);
                oCmd.Parameters.AddWithValue("@longMax", longMax);
                oCmd.Parameters.AddWithValue("@inOrOut", inOrOut);
                oCmd.Parameters.AddWithValue("@statePrivacy", statePrivacy);

                myConnection.Open();
                using (SqlDataReader oReader = oCmd.ExecuteReader())
                {
                    while (oReader.Read())
                    {
                        amses.AddSwaggerDevice(
                            oReader["DeviceID"].ToString(), 
                            (decimal)oReader["Lat"],
                            (decimal)oReader["Long"]
                        );
                    }

                    myConnection.Close();
                }
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
        [Route("frontend/singleLatest")]
        [HttpPost]
        public HttpResponseMessage GetLatestDataFromSingleAMSDevice([FromBody]string DeviceID)
        {
            var db = new AirUDatabaseCOE();

            // Validate DeviceID represents an actual AMS device.

            // Performs database query to obtain the latest Datapoints for specific DeviceID.

            // Send user the latest datapoints. 

            var message = Request.CreateResponse(HttpStatusCode.OK);
            return message;
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

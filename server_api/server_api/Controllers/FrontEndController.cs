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
         * // Testing Methods (DONE)
         * + ServerTest() - This method simply returns successful.
         * + ServerAndDatabaseTest() - This method performs a database query and returns result.
         * 
         * // Map View (DONE)
         * + GetAllDataPointsForDevice() - Returns all datapoints for a Device given a DeviceID. (Line Chart)
         * + GetLatestDataFromSingleAMSDevice() - Returns the latest datapoints for a single AMS device based on specified DeviceID. (Details Panel)
         * + GetAllDevicesInMapRange() -  Returns the AMS DeviceStates for all AMS devices within specified MapParameters. (Map)
         * 
         * // Heat Map View (DONE)
         * + GetAllDevicesInMapRange() -  Returns the AMS DeviceStates for all AMS devices within specified MapParameters. (Map)
         * + GetLatestValuesForSpecifiedPollutantInMapRange - Returns the values of the pollutants within the specified map range. (HeatMap Layer)
         * 
         * // Device Compare View (DONE)
         * + GetAllDataPointsForDevice() - Returns all datapoints for a Device given a DeviceID. (Compare Chart)
         * 
         * // Device Registration View (DONE)
         * + DeviceRegistration() - Registers an AMS device.
         * 
         * // Device Settings View
         * + GetUsersDeviceStates() - Returns the set of DeviceStates associated with the given user email.
         * - UpdateDeviceState() - Updates a single AMS DeviceState from the "my devices" settings web page.
         *   - Need output format
         * 
         * // User Registration View (DONE)
         * + UserRegistration() - Validates user is not already in database and if not, creates new User in database.
         * 
         * // User Login View (DONE)
         * + UserLogin() - Validates user based on Email and Pass.
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
        public IHttpActionResult ServerTest()
        {
            return Ok("Success");
        }

        // ~~~~~ GET ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        /// <summary>
        ///   This is a testing method. 
        ///   
        ///   This method simply returns successful.
        /// </summary>
        /// <returns></returns>
        [Route("frontend/servertestpost")]
        [HttpPost]
        public HttpResponseMessage ServerTestPost([FromBody]string name)
        {
            var message = Request.CreateResponse(HttpStatusCode.OK);
            message.Content = new StringContent("Success: " + name);
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
        public IHttpActionResult ServerAndDatabaseTest()
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

                return Ok(swaggerUsers);
            }
            else
            {
                return NotFound();
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
        public IHttpActionResult GetAllDataPointsForDevice([FromBody]string deviceID)
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

            return Ok(data);
        }

        // ~~~~~ POST ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        /// <summary>
        ///   Validates user is not already in database and if not, creates new User in database.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [Route("frontend/registerUser")]
        [HttpPost]
        public IHttpActionResult UserRegistration([FromBody]User user)
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
                return Ok("Account registration successful! Welcome, " + user.Email);
            }
            else
            {
                // Account register failed. Account with email address: '<user.Email>' already exists. Please try a different email address.
                return BadRequest("Account registration failed! Account with email address: " + 
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
        public IHttpActionResult UserLogin([FromBody]User user)
        {
            var db = new AirUDatabaseCOE();

            User validUserAndPass = db.Users.SingleOrDefault(x => x.Email == user.Email && x.Pass == user.Pass);

            if (validUserAndPass != null)
            {
                // Login success.
                return Ok("Login Successful! Welcome, " + user.Email);
            }
            else
            {
                // Login fail.
                return BadRequest("Login failed! Please check email and password.");
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
        public IHttpActionResult DeviceRegistration([FromBody]DeviceAndState newDeviceAndState)
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
                return Ok(newDeviceAndState);
                
            }
            else
            {
                // Add device fail.
                return BadRequest("Existing Device");
            }
        }

        /// <summary>
        ///   Returns the set of DeviceStates associated with the given user email.
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        [Route("frontend/getUsersDeviceStates")]
        [HttpPost]
        public IHttpActionResult GetUsersDeviceStates([FromBody]string email)
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

                return Ok(deviceStates);
                //return Ok("Item Found!");
            }
            else
            {
                // User with email address: <email> does not exist.
                return NotFound();
            }
        }

        /// <summary>
        ///   Updates a single AMS DeviceState from the "my devices" settings web page.
        /// </summary>
        /// <param name="state">The state of the device</param>
        /// <returns></returns>
        [Route("frontend/state")]
        [HttpPost]
        public IHttpActionResult UpdateDeviceState([FromBody]DeviceState state)
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

                return Ok();
            }
            else
            {
                // Device with DeviceID: <deviceID> does not exist.
                return NotFound();
            }
        }

        /// <summary>
        ///   Returns the AMS DeviceStates for all AMS devices within specified MapParameters.
        ///   
        ///   Primary Use: Populate the Map View with AMS device icons. 
        /// </summary>
        /// <param name="para">The NE and SW bounds of a map</param>
        /// <returns></returns>
        [ResponseType(typeof(IEnumerable<SwaggerAMSList>))]
        [Route("frontend/map")]
        [HttpPost]
        public IHttpActionResult GetAllDevicesInMapRange([FromBody]MapParameters para)
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

            return Ok(amses);
        }
        
        /// <summary>
        ///   Returns the values of the pollutants within the specified map range.
        ///   
        ///   Primary Use: Populate the HeatMap View with values for a specified pollutant.
        /// </summary>
        /// <param name="para">The NE and SW bounds of a map and name of requested Pollutant</param>
        /// <returns></returns>
        [ResponseType(typeof(IEnumerable<SwaggerHeatMapValueList>))]
        [Route("frontend/heatmap")]
        [HttpPost]
        public IHttpActionResult GetLatestValuesForSpecifiedPollutantInMapRange([FromBody]HeatMapParameters para)
        {
            // DEFAULT VALUES
            DateTime measurementTimeMax = DateTime.Now;
            int inOrOut = 0; // 0 = Outside, 1 = Inside
            int statePrivacy = 0; // 0 = Not Private, 1 = Private

            // SHOULD BE VARIABLE
            decimal latMin = para.mapParameters.southWest.lat;
            decimal latMax = para.mapParameters.northEast.lat;
            decimal longMin = para.mapParameters.southWest.lng;
            decimal longMax = para.mapParameters.northEast.lng;
            string pollutantName = para.pollutantName;

            // CURRENTLY NOT USED
            /*
            String deviceID = "12-34-56-78-9A-BC";
            DateTime measurementTimeMin = measurementTimeMax.AddHours(-12);
            */
            
            SqlConnection conn = new SqlConnection(@"Data Source=mssql.eng.utah.edu;Initial Catalog=lobato;Persist Security Info=True;User ID=lobato;Password=eVHDpynh;MultipleActiveResultSets=True;Application Name=EntityFramework");
            SwaggerHeatMapValueList pollutantCoordinatesAndValues = new SwaggerHeatMapValueList(pollutantName);

            using (SqlConnection myConnection = conn)
            {
                string oString = @"select Devices_States_and_DataPoints.DeviceID,
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
												where Lat > @latMin
												and Lat < @latMax
												and Long > @longMin
												and Long < @longMax
												and StatePrivacy=@statePrivacy
												and InOrOut=@inOrOut
					                            group by DeviceID) as MaxStates
			                            left join Devices_States_and_DataPoints
			                            on MaxStates.DeviceID = Devices_States_and_DataPoints.DeviceID
			                            and MaxStates.MaxStateTime = Devices_States_and_DataPoints.StateTime) as MaxStatesAndMeasurementTime
										where PollutantName=@pollutantName
	                            group by DeviceID, PollutantName) as MaxMeasurementTimeForPollutants
                            left join Devices_States_and_DataPoints
			                            on MaxMeasurementTimeForPollutants.DeviceID = Devices_States_and_DataPoints.DeviceID
			                            and MaxMeasurementTimeForPollutants.PollutantName = Devices_States_and_DataPoints.PollutantName
			                            and MaxMeasurementTimeForPollutants.MaxMeasurementTime = Devices_States_and_DataPoints.MeasurementTime
										order by DeviceID;";
                SqlCommand oCmd = new SqlCommand(oString, myConnection);

                oCmd.Parameters.AddWithValue("@latMin", latMin);
                oCmd.Parameters.AddWithValue("@latMax", latMax);
                oCmd.Parameters.AddWithValue("@longMin", longMin);
                oCmd.Parameters.AddWithValue("@longMax", longMax);
                oCmd.Parameters.AddWithValue("@inOrOut", inOrOut);
                oCmd.Parameters.AddWithValue("@statePrivacy", statePrivacy);
                oCmd.Parameters.AddWithValue("@pollutantName", pollutantName);

                myConnection.Open();
                using (SqlDataReader oReader = oCmd.ExecuteReader())
                {
                    while (oReader.Read())
                    {
                        pollutantCoordinatesAndValues.AddSwaggerCoordinateAndValue((decimal)oReader["Lat"], (decimal)oReader["Long"], (double)oReader["Value"]);
                    }

                    myConnection.Close();
                }
            }

            return Ok(pollutantCoordinatesAndValues);
        }

        /// <summary>
        ///   Returns the latest datapoints for a single AMS device based on specified DeviceID. 
        ///   
        ///   Primary Use: "details" panel on Map View after selecting AMS device on map. 
        /// </summary>
        /// <param name="deviceID"></param>
        /// <returns></returns>
        [ResponseType(typeof(IEnumerable<SwaggerLatestPollutantsList>))]
        [Route("frontend/singleLatest")]
        [HttpPost]
        public IHttpActionResult GetLatestDataFromSingleAMSDevice([FromBody]string deviceID)
        {
            var db = new AirUDatabaseCOE();

            // Validate DeviceID represents an actual AMS device.
            Device registeredDevice = db.Devices.SingleOrDefault(x => x.DeviceID == deviceID);
            if (registeredDevice != null)
            {
                // Performs database query to obtain the latest Datapoints for specific DeviceID.
                SqlConnection conn = new SqlConnection(@"Data Source=mssql.eng.utah.edu;Initial Catalog=lobato;Persist Security Info=True;User ID=lobato;Password=eVHDpynh;MultipleActiveResultSets=True;Application Name=EntityFramework");
                SwaggerLatestPollutantsList latestPollutants = new SwaggerLatestPollutantsList();
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
                            latestPollutants.AddPollutantAndValue(oReader["PollutantName"].ToString(), (double)oReader["Value"]);
                        }
                        myConnection.Close();
                    }
                }

                return Ok(latestPollutants);
            }
            else
            {
                // Device with DeviceID: <deviceID> does not exist.
                return NotFound();
            }

        }

        /*
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
        */
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

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
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;


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

        /// <summary>
        /// Response Example:
        /// {
        ///    "DateObserved": "2015-12-06 ",
        ///    "HourObserved": 17,
        ///    "LocalTimeZone": "MST",
        ///    "ReportingArea": "Salt Lake City",
        ///    "StateCode": "UT",
        ///    "Latitude": 40.777,
        ///    "Longitude": -111.93,
        ///    "ParameterName": "O3",
        ///    "AQI": 17,
        ///    "Category": {
        ///        "Number": 1,
        ///        "Name": "Good"
        /// }
        //}
        /// </summary>
        /// <returns></returns>
        [ResponseType(typeof(SwaggerAQIData))]
        [Route("frontend/aqi")]
        [HttpGet]
        public IHttpActionResult GetAQI()
        {
            HttpWebRequest request = WebRequest.Create("http://www.airnowapi.org/aq/observation/zipCode/current/?format=application/json&zipCode=84102&distance=25&API_KEY=1CD19983-D26A-46F2-8022-6A6E16A991F7") as HttpWebRequest;
            HttpWebResponse response = request.GetResponse() as HttpWebResponse;
            Stream stream = response.GetResponseStream();
            StreamReader reader = new StreamReader(stream);
            string jsonString = reader.ReadToEnd();

            var json = JsonConvert.DeserializeObject<SwaggerAQIData[]>(jsonString);

            return Ok(json[0]);
        }

        [Route("frontend/daq")]
        [HttpGet]
        public IHttpActionResult GetDAQStationData()
        {
            HttpWebRequest request = WebRequest.Create("http://air.utah.gov/xmlFeed.php?id=slc") as HttpWebRequest;
            HttpWebResponse response = request.GetResponse() as HttpWebResponse;
            Stream stream = response.GetResponseStream();
            //StreamReader reader = new StreamReader(stream);
            //string jsonString = reader.ReadToEnd();

            XmlSerializer serializer = new XmlSerializer(typeof(DAQData));

            StreamReader reader = new StreamReader(stream);
            DAQData data = (DAQData)serializer.Deserialize(reader);
            reader.Close();

            data.site.latitude = 40.734280;
            data.site.longitude = -111.871593;

            return Ok(data);
        }

        [System.Xml.Serialization.XmlRoot("air_quality_data")]
        public class DAQData
        {
            public string state { get; set; }
            public site site { get; set; }
        }

        [System.Xml.Serialization.XmlRoot("air_quality_data")]
        public class site
        {
            public string name { get; set; }
            public data data { get; set; }
            public double latitude { get; set; }
            public double longitude { get; set; }
        }

        public class data
        {
            public string date { get; set; }
            public string ozone { get; set; }
            public string ozone_8hr_avg { get; set; }
            public string pm25 { get; set; }
            public string pm25_24hr_avg { get; set; }
            public string nox { get; set; }
            public string no2 { get; set; }
            public string temperature { get; set; }
            public string relative_humidity { get; set; }
            public string wind_speed { get; set; }
            public string wind_direction { get; set; }
            public string co { get; set; }
            public string solar_radiation { get; set; }
            public string so2 { get; set; }
            public string noy { get; set; }
            public string bp { get; set; }
            public string pm10 { get; set; }
        }

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
        [ResponseType(typeof(IEnumerable<SwaggerUsers>))]
        [Route("frontend/registeredUsers")]
        [HttpGet]
        public IHttpActionResult ServerAndDatabaseTest()
        {
            var db = new AirUDatabaseCOE();

            List<User> allUsers = db.Users.Select(x => x).ToList<User>();

            if(allUsers != null)
            {
                List<SwaggerUsers> swaggerUsers = new List<SwaggerUsers>();

                foreach (var item in allUsers)
                {
                    swaggerUsers.Add(new SwaggerUsers(item.Email));
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


            Device existingDevice = db.Devices.SingleOrDefault(x => x.DeviceID == deviceID);

            if (existingDevice != null)
            {

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
            else
            {
                // Account register failed. Account with email address: '<user.Email>' already exists. Please try a different email address.
                return BadRequest("Device with ID: " + deviceID + " does not exist. Please try a different Device ID.");
            }
        }

        /// <summary>
        ///   Validates user is not already in database and if not, creates new User in database.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [Route("frontend/registerUser")]
        [HttpPost]
        public IHttpActionResult UserRegistration([FromBody]SwaggerUser user)
        {
            var db = new AirUDatabaseCOE();

            User existingUser = db.Users.SingleOrDefault(x => x.Email == user.email);

            if (existingUser == null)
            {
                // Perform queries to insert new user into database.
                User newUser = new User();
                newUser.Email = user.email;
                newUser.Pass = user.pass;

                db.Users.Add(newUser);
                db.SaveChanges();

                // Account register success.
                return Ok("Account registration successful! Welcome, " + user.email);
            }
            else
            {
                // Account register failed. Account with email address: '<user.Email>' already exists. Please try a different email address.
                return BadRequest("Account registration failed! Account with email address: " + 
                                                                             user.email + 
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
        public IHttpActionResult UserLogin([FromBody]SwaggerUser user)
        {
            var db = new AirUDatabaseCOE();

            User validUserAndPass = db.Users.SingleOrDefault(x => x.Email == user.email && x.Pass == user.pass);

            if (validUserAndPass != null)
            {
                // Login success.
                return Ok("Login Successful! Welcome, " + user.email);
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
        [ResponseType(typeof(SwaggerDeviceAndState))]
        [Route("frontend/registerDevice")]
        [HttpPost]
        public IHttpActionResult DeviceRegistration([FromBody]SwaggerDeviceAndState newDeviceAndState)
        {
            var db = new AirUDatabaseCOE();


            Device existingDevice = db.Devices.SingleOrDefault(x => x.DeviceID == newDeviceAndState.Id);
            if (existingDevice == null)
            {
                // Add device success.
                Device device = new Device();
                device.DeviceID = newDeviceAndState.Id;
                device.Email = "jaredpotter1@gmail.com"; // newDeviceAndState.Email;
                device.DevicePrivacy = newDeviceAndState.Private;
                db.Devices.Add(device);
                db.SaveChanges();

                DeviceState state = new DeviceState();
                state.Device = device;
                state.DeviceID = newDeviceAndState.Id;
                state.InOrOut = newDeviceAndState.Indoor;
                state.StatePrivacy = newDeviceAndState.Private;
                state.StateTime = new DateTime(1900, 1, 1);
                state.Long = 360.0m;
                state.Lat = 360.0m;
                db.DeviceStates.Add(state);
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
        /// ***METHOD NOT PART OF PROTOTYPE*** ignore till spring.
        /// 
        ///   Returns the set of DeviceStates associated with the given user email.
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        //[ResponseType(typeof())]
        [Route("frontend/getUsersDeviceStates")]
        [HttpGet]
        public IHttpActionResult GetUsersDeviceStates()
        {
            var db = new AirUDatabaseCOE();

            string email = "jaredpotter1@gmail.com";

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
            }
            else
            {
                // User with email address: <email> does not exist.
                return NotFound();
            }
        }

        /// <summary>
        /// ***METHOD NOT PART OF PROTOTYPE*** ignore till spring.
        /// 
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
        public IHttpActionResult GetAllDevicesInMapRange([FromBody]SwaggerMapParameters para)
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
        public IHttpActionResult GetLatestValuesForSpecifiedPollutantInMapRange([FromBody]SwaggerHeatMapParameters para)
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
        [ResponseType(typeof(SwaggerLatestDataPoints))]
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
                SwaggerLatestDataPoints latest = new SwaggerLatestDataPoints();
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

                        foreach (var item in latestPollutants.latest)
                        {
                            switch (item.pollutantName)
                            {
                                case "Altitude":
                                    latest.altitude = item.value.ToString();
                                    break;

                                case "CO":
                                    latest.co = item.value.ToString();
                                    break;

                                case "CO2":
                                    latest.co2 = item.value.ToString();
                                    break;

                                case "Humidity":
                                    latest.humidity = item.value.ToString();
                                    break;

                                case "NO2":
                                    latest.no2 = item.value.ToString();
                                    break;

                                case "PM":
                                    latest.pm = item.value.ToString();
                                    break;

                                case "Pressure":
                                    latest.pressure = item.value.ToString();
                                    break;

                                case "Temperature":
                                    latest.temp = item.value.ToString();
                                    break;
                            }
                        }
                        myConnection.Close();
                    }
                }
                return Ok(latest);
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
        public static long ConvertDateTimeToMilliseconds(DateTime date)
        {
            return (long)(date - new DateTime(1970, 1, 1)).TotalMilliseconds;
        }
    }
}

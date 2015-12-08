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
using System.Globalization;


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

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [ResponseType(typeof(IEnumerable<SwaggerDAQData>))]
        [Route("frontend/daq")]
        [HttpGet]
        public IHttpActionResult GetSingleDAQStationData()
        {
            string[] apiUrls = new string[] 
                { "http://air.utah.gov/xmlFeed.php?id=boxelder",   // "Box Elder County"
                  "http://air.utah.gov/xmlFeed.php?id=cache",      // "Cache County"
                  "http://air.utah.gov/xmlFeed.php?id=p2",         // Carbon/"Price"
                  "http://air.utah.gov/xmlFeed.php?id=bv",         // "Davis County"
                  "http://air.utah.gov/xmlFeed.php?id=rs",         // "Duchesne County"
                  "http://air.utah.gov/xmlFeed.php?id=slc",        // "Salt Lake County"
                  "http://air.utah.gov/xmlFeed.php?id=tooele",     // "Tooele County"
                  "http://air.utah.gov/xmlFeed.php?id=v4",         // "Uintah County"
                  "http://air.utah.gov/xmlFeed.php?id=utah",       // "Utah County"
                  "http://air.utah.gov/xmlFeed.php?id=washington", // "Washington County"
                  "http://air.utah.gov/xmlFeed.php?id=weber"       // "Weber County"
                };

            Tuple<double, double>[] gpsLocations = new Tuple<double, double>[] 
            { new Tuple<double, double>(41.510544, -112.014640), 
              new Tuple<double, double>(41.737159, -111.836706),
              new Tuple<double, double>(39.598401, -110.811250),
              new Tuple<double, double>(40.979952, -111.887608),
              new Tuple<double, double>(40.163389, -110.402936),
              new Tuple<double, double>(40.734280, -111.871593), 
              new Tuple<double, double>(40.530786, -112.298464),
              new Tuple<double, double>(40.455679, -109.528717),
              new Tuple<double, double>(40.296847, -111.695003),
              new Tuple<double, double>(37.096288, -113.568486),
              new Tuple<double, double>(41.222803, -111.973789) };

            SwaggerDAQData[] dataArray = new SwaggerDAQData[11];

            for (int i = 0; i < apiUrls.Length; i++)
            {
                HttpWebRequest request = WebRequest.Create(apiUrls[i]) as HttpWebRequest;
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                Stream stream = response.GetResponseStream();
                XmlSerializer serializer = new XmlSerializer(typeof(SwaggerDAQData));
                StreamReader reader = new StreamReader(stream);
                SwaggerDAQData data = (SwaggerDAQData)serializer.Deserialize(reader);

                data.site.latitude = gpsLocations[i].Item1;
                data.site.longitude = gpsLocations[i].Item2;

                dataArray[i] = data;
            }

            return Ok(dataArray);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        [Route("frontend/daqChart")]
        [HttpPost]
        public IHttpActionResult GetDAQChartData([FromBody]string name)
        {
            Dictionary<string, string> apiUrlDict = new Dictionary<string, string>();
            apiUrlDict.Add("Box Elder County", "http://air.utah.gov/xmlFeed.php?id=boxelder");
            apiUrlDict.Add("Cache County", "http://air.utah.gov/xmlFeed.php?id=cache");
            apiUrlDict.Add("Price", "http://air.utah.gov/xmlFeed.php?id=p2");
            apiUrlDict.Add("Davis County", "http://air.utah.gov/xmlFeed.php?id=bv");
            apiUrlDict.Add("Duchesne County", "http://air.utah.gov/xmlFeed.php?id=rs");
            apiUrlDict.Add("Salt Lake County", "http://air.utah.gov/xmlFeed.php?id=slc");
            apiUrlDict.Add("Tooele County", "http://air.utah.gov/xmlFeed.php?id=tooele");
            apiUrlDict.Add("Uintah County", "http://air.utah.gov/xmlFeed.php?id=v4");
            apiUrlDict.Add("Utah County", "http://air.utah.gov/xmlFeed.php?id=utah");
            apiUrlDict.Add("Washington County", "http://air.utah.gov/xmlFeed.php?id=washington");
            apiUrlDict.Add("Weber County", "http://air.utah.gov/xmlFeed.php?id=weber");

            string url = apiUrlDict[name];

            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            HttpWebResponse response = request.GetResponse() as HttpWebResponse;
            Stream stream = response.GetResponseStream();
            XmlSerializer serializer = new XmlSerializer(typeof(SwaggerDAQData));
            StreamReader reader = new StreamReader(stream);
            SwaggerDAQData data = (SwaggerDAQData)serializer.Deserialize(reader);

            List<SwaggerPollutantList> pollutantDataList = new List<SwaggerPollutantList>();

            List<string> dates = new List<string>();
            SwaggerPollutantList ozone = new SwaggerPollutantList("Ozone ppm");
            SwaggerPollutantList pm25 = new SwaggerPollutantList("PM 2.5 ug/m^3");
            SwaggerPollutantList no2 = new SwaggerPollutantList("NO2 ppm");
            SwaggerPollutantList temperature = new SwaggerPollutantList("Temperature F");
            SwaggerPollutantList co = new SwaggerPollutantList("CO ppm");

            foreach(var dataSet in data.site.data)
            {
                DateTime date = DateTime.ParseExact(dataSet.date, "MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                long dateMilliseconds = ConvertDateTimeToMilliseconds(date);

                if(dataSet.ozone != "")
                {
                    ozone.values.Add(new object[2]);
                    ozone.values.Last()[0] = dateMilliseconds;
                    ozone.values.Last()[1] = Decimal.Parse(dataSet.ozone);
                }

                if (dataSet.pm25 != "")
                {
                    pm25.values.Add(new object[2]);
                    pm25.values.Last()[0] = dateMilliseconds;
                    pm25.values.Last()[1] = Decimal.Parse(dataSet.pm25);
                }

                if (dataSet.no2 != "")
                {
                    no2.values.Add(new object[2]);
                    no2.values.Last()[0] = dateMilliseconds;
                    no2.values.Last()[1] = Decimal.Parse(dataSet.no2);
                }

                if (dataSet.temperature != "")
                {
                    temperature.values.Add(new object[2]);
                    temperature.values.Last()[0] = dateMilliseconds;
                    temperature.values.Last()[1] = Decimal.Parse(dataSet.temperature);
                }

                if (dataSet.co != "")
                {
                    co.values.Add(new object[2]);
                    co.values.Last()[0] = dateMilliseconds;
                    co.values.Last()[1] = Decimal.Parse(dataSet.co);
                }
            }

            if (ozone.values.Count != 0)
            {
                pollutantDataList.Add(ozone);
            }

            if (pm25.values.Count != 0)
            {
                pollutantDataList.Add(pm25);
            }

            if (no2.values.Count != 0)
            {
                pollutantDataList.Add(no2);
            }

            if (temperature.values.Count != 0)
            {
                pollutantDataList.Add(temperature);
            }

            if (co.values.Count != 0)
            {
                pollutantDataList.Add(co);
            }


            return Ok(pollutantDataList);
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
            var db = new AirUDBCOE();

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
            var db = new AirUDBCOE();


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
            var db = new AirUDBCOE();

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
            var db = new AirUDBCOE();

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
        /// <param name="newDeviceState">The current Device and its DeviceState</param>
        /// <returns></returns>
        [ResponseType(typeof(SwaggerDeviceState))]
        [Route("frontend/registerUserDevice")]
        [HttpPost]
        public IHttpActionResult RegisterUserDevice([FromBody]SwaggerDeviceState newDeviceState)
        {
            var db = new AirUDBCOE();

            Device existingDevice = db.Devices.SingleOrDefault(x => x.DeviceID == newDeviceState.id);
            if (existingDevice == null)
            {
                // Add device success.
                Device device = new Device();
                device.Name = newDeviceState.name;
                device.DeviceID = newDeviceState.id;
                device.Email = "jaredpotter1@gmail.com"; // newDeviceAndState.Email;
                device.DevicePrivacy = newDeviceState.privacy;
                device.Purpose = newDeviceState.purpose;
                db.Devices.Add(device);
                db.SaveChanges();

                DeviceState state = new DeviceState();
                state.Device = device;
                state.DeviceID = newDeviceState.id;
                state.InOrOut = newDeviceState.indoor;
                state.StatePrivacy = newDeviceState.privacy;
                state.StateTime = new DateTime(1900, 1, 1);
                state.Long = 0.0m;
                state.Lat = 90.0m;
                db.DeviceStates.Add(state);
                db.SaveChanges();

                return Ok(newDeviceState);
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
        /// <returns></returns>
        [ResponseType(typeof(IEnumerable<SwaggerDeviceState>))]
        [Route("frontend/getUserDeviceStates")]
        [HttpGet]
        public IHttpActionResult GetUserDeviceStates()
        {
            var db = new AirUDBCOE();

            string email = "jaredpotter1@gmail.com";

            // Validate given email has associated User.
            User registeredUser = db.Users.SingleOrDefault(x => x.Email == email);

            if (registeredUser != null)
            {
                SqlConnection conn = new SqlConnection(@"Data Source=mssql.eng.utah.edu;Initial Catalog=lobato;Persist Security Info=True;User ID=lobato;Password=eVHDpynh;MultipleActiveResultSets=True;Application Name=EntityFramework");
                List<SwaggerDeviceState> swaggerDeviceStates = new List<SwaggerDeviceState>();
                using (SqlConnection myConnection = conn)
                {
                    string oString = @"select MaxCompleteStates.DeviceID, Devices.Name, Devices.Purpose, MaxCompleteStates.StateTime, MaxCompleteStates.Lat, MaxCompleteStates.Long, MaxCompleteStates.InOrOut, MaxCompleteStates.StatePrivacy from
                                        (select MaxStates.DeviceID, MaxStates.StateTime, DeviceStates.Lat, DeviceStates.Long, DeviceStates.InOrOut, DeviceStates.StatePrivacy from
	                                        (select DeviceID, Max(StateTime) as StateTime
				                                        from DeviceStates
				                                        group by DeviceID) as MaxStates
		                                        left join DeviceStates
		                                        on MaxStates.DeviceID=DeviceStates.DeviceID
		                                        and MaxStates.StateTime = DeviceStates.StateTime) as MaxCompleteStates
		                                        left join Devices
		                                        on Devices.DeviceID=MaxCompleteStates.DeviceID
		                                        where Devices.Email = @owner;";
                    SqlCommand oCmd = new SqlCommand(oString, myConnection);

                    oCmd.Parameters.AddWithValue("@owner", email);

                    myConnection.Open();
                    using (SqlDataReader oReader = oCmd.ExecuteReader())
                    {
                        while (oReader.Read())
                        {
                            swaggerDeviceStates.Add(new SwaggerDeviceState(
                                                                    (string)oReader["Name"],
                                                                    (string)oReader["DeviceID"],
                                                                    (bool)oReader["StatePrivacy"],
                                                                    (string)oReader["Purpose"],
                                                                    (bool) oReader["InOrOut"],
                                                                    (decimal)oReader["Lat"],
                                                                    (decimal)oReader["Long"],
                                                                    email));
                        }

                        myConnection.Close();
                    }
                }
                return Ok(swaggerDeviceStates);
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
        [ResponseType(typeof(IEnumerable<SwaggerDeviceState>))]
        [Route("frontend/updateUserDeviceState")]
        [HttpPut]
        public IHttpActionResult UpdateUserDeviceState([FromBody]SwaggerDeviceState state)
        {
            var db = new AirUDBCOE();
            
            // Validate Device from given DeviceId exists.
            Device registeredDevice = db.Devices.SingleOrDefault(x => x.DeviceID == state.id);

            if (registeredDevice != null)
            {
                // Request previous state from database based on state.DeviceID
                DeviceState previousState = (
                                    from device in db.DeviceStates
                                    where device.DeviceID == state.id
                                    && device.StateTime <= DateTime.Now // **May be a future source of contention - REVIEW**
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

                DeviceState newDeviceState = new DeviceState();
                newDeviceState.Device = previousState.Device;
                newDeviceState.DeviceID = state.id;
                newDeviceState.InOrOut = state.indoor;
                newDeviceState.StatePrivacy = state.privacy;
                newDeviceState.Lat = previousState.Lat;
                newDeviceState.Long = previousState.Long;
                newDeviceState.StateTime = DateTime.Now;
                db.DeviceStates.Add(newDeviceState);
                db.SaveChanges();

                registeredDevice.Name = state.name;
                registeredDevice.Purpose = state.purpose;

                //db.Devices.Add(registeredDevice);
                db.SaveChanges();

                // Send user newly updated state back to user
                return Ok(state);
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

            var db = new AirUDBCOE();

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
            var db = new AirUDBCOE();

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

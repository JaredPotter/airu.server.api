using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;


namespace server_api.Controllers
{
    public class FrontEndController : ApiController
    {

        [Route("api/frontend/registeredUsers")]
        [HttpGet]
        public IEnumerable<string> GetAllRegisteredUsers()
        {
            var db = new AirU_Database_Entity();

            List<User> allUsers = db.Users.Select(x => x).ToList<User>();
            List<string> allUsersString = new List<string>();
 
            foreach(var item in allUsers)
            {
                allUsersString.Add(item.Email);
            }

            return allUsersString;
        }

        [Route("api/frontend/{user_id}")]
        [HttpGet]
        public HttpResponseMessage GetRegisteredUser(int user_id)
        {
            var db = new AirU_Database_Entity();

            List<User> allUsers = db.Users.Select(x => x).ToList<User>();
            User user = null;

            if (allUsers.Count > user_id)
            {
                user = allUsers[user_id];
            }
            

            if(user != null)
            {
                // User with email address: <email> does exsit.
                return Request.CreateResponse<string>(HttpStatusCode.OK, "User id = [" + user_id + "] and username = " + user.Email + " does exist.");
            }
            else
            {
                // User with email address: <email> does not exist.
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Item not found.");
            }
        }

        [Route("api/frontend/{id}")]
        [HttpGet]
        public HttpResponseMessage GetAMSData()
        {
            var db = new AirU_Database_Entity();

            var message = Request.CreateResponse(HttpStatusCode.OK);

            string email = "zacharyisaiahlobato@gmail.com";
            string pass = "burritos";
            string badPass = "enchiladas";
            string deviceID = "12-34-56-78-9A-BC";
            string badDeviceID = "I-SHOULDNT-WORK";
            float dataPointValue = 52;
            string pollutantName = "Temperature";

            /* Register User */
            // --Validate user is/isnot registered
            User validUser = db.Users.SingleOrDefault(x => x.Email == email);

            // --Insert user data into DB

            /*
            User steve = new User();
            steve.Email = "steve@jobs.com";
            steve.Pass = "apple";

            db.Users.Add(steve);
            db.SaveChanges();
            */

            /* Log User In */
            // --Validate USER and PASS are correct in DB
            User validUserAndPass = db.Users.SingleOrDefault(x => x.Email == email && x.Pass == pass);
            User invalidUserAndPass = db.Users.SingleOrDefault(x => x.Email == email && x.Pass == badPass);

            /* Register Device */
            // -- Validate that the device is not registered
            Device validDevce = db.Devices.SingleOrDefault(x => x.DeviceID == deviceID);
            Device invalidDevce = db.Devices.SingleOrDefault(x => x.DeviceID == badDeviceID);
            // -- Insert device into DB
            /*
            Device newDevice = new Device();
            newDevice.DeviceID = "ZZ-ZZ-ZZ-JJ-JJ-JJ";
            newDevice.DevicePrivacy = false;
            newDevice.Email = "steve@jobs.com";
            db.Devices.Add(newDevice);
            
            db.SaveChanges(); 
            */
            // -- Insert device state into DB
            /*
            DeviceState newDeviceState = new DeviceState();
            newDeviceState.DeviceID = "ZZ-ZZ-ZZ-JJ-JJ-JJ";
            newDeviceState.InOrOut = false;
            newDeviceState.StatePrivacy = false;
            newDeviceState.StateTime = new DateTime(2015,11,25,13,16,1);
            newDeviceState.Long = 123.456789m;
            newDeviceState.Lat = 87.1224m;
            db.DeviceStates.Add(newDeviceState);

            db.SaveChanges();
            */

            /* Insert Datapoint into DB */
            /*
            DataPoint newDataPoint = new DataPoint();
            newDataPoint.DeviceID = "ZZ-ZZ-ZZ-JJ-JJ-JJ";
            newDataPoint.MeasurementTime = DateTime.Now;
            newDataPoint.Value = dataPointValue;
            newDataPoint.PollutantName = pollutantName;

            db.DataPoints.Add(newDataPoint);
            db.SaveChanges();
            */

            return message;
        }

        [Route("api/frontend/login")]
        [HttpPost]
        public HttpResponseMessage UserLogin([FromBody]User user)
        {
            var db = new AirU_Database_Entity();

            User validUserAndPass = db.Users.SingleOrDefault(x => x.Email == user.Email && x.Pass == user.Pass);

            var message = Request.CreateResponse(HttpStatusCode.OK);

            if(validUserAndPass != null)
            {
                // Login success.
                message.Content = new StringContent("Login Successful! Welcome, " + user.Email);
            }
            else
            {
                // Login fail.
                message.Content = new StringContent("Login failed! Please check email and password");
            }

            return message;
            //string email = "zacharyisaiahlobato@gmail.com";
            //string pass = "burritos";
            //string badPass = "enchiladas";
            //string deviceID = "12-34-56-78-9A-BC";
            //string badDeviceID = "I-SHOULDNT-WORK";
            //float dataPointValue = 52;
            //string pollutantName = "Temperature";

            /* Register User */
            // --Validate user is/isnot registered
            //User validUser = db.Users.SingleOrDefault(x => x.Email == email);

            // --Insert user data into DB

            /*
            User steve = new User();
            steve.Email = "steve@jobs.com";
            steve.Pass = "apple";

            db.Users.Add(steve);
            db.SaveChanges();
            */

            /* Log User In */
            // --Validate USER and PASS are correct in DB
            
           // User invalidUserAndPass = db.Users.SingleOrDefault(x => x.Email == email && x.Pass == badPass);

            /* Register Device */
            // -- Validate that the device is not registered
            //Device validDevce = db.Devices.SingleOrDefault(x => x.DeviceID == deviceID);
            //Device invalidDevce = db.Devices.SingleOrDefault(x => x.DeviceID == badDeviceID);
            // -- Insert device into DB
            /*
            Device newDevice = new Device();
            newDevice.DeviceID = "ZZ-ZZ-ZZ-JJ-JJ-JJ";
            newDevice.DevicePrivacy = false;
            newDevice.Email = "steve@jobs.com";
            db.Devices.Add(newDevice);
            
            db.SaveChanges(); 
            */
            // -- Insert device state into DB
            /*
            DeviceState newDeviceState = new DeviceState();
            newDeviceState.DeviceID = "ZZ-ZZ-ZZ-JJ-JJ-JJ";
            newDeviceState.InOrOut = false;
            newDeviceState.StatePrivacy = false;
            newDeviceState.StateTime = new DateTime(2015,11,25,13,16,1);
            newDeviceState.Long = 123.456789m;
            newDeviceState.Lat = 87.1224m;
            db.DeviceStates.Add(newDeviceState);

            db.SaveChanges();
            */

            /* Insert Datapoint into DB */
            /*
            DataPoint newDataPoint = new DataPoint();
            newDataPoint.DeviceID = "ZZ-ZZ-ZZ-JJ-JJ-JJ";
            newDataPoint.MeasurementTime = DateTime.Now;
            newDataPoint.Value = dataPointValue;
            newDataPoint.PollutantName = pollutantName;

            db.DataPoints.Add(newDataPoint);
            db.SaveChanges();
            */

            //data.Add(dataSet.firstName + dataSet.lastName);

            //var message = Request.CreateResponse(HttpStatusCode.OK);
           // message.Headers.Location = new Uri(Request.RequestUri + data.Count.ToString());
            
        }

        [Route("api/frontend/register")]
        [HttpPost]
        public HttpResponseMessage UserRegistration([FromBody]User user)
        {
            var db = new AirU_Database_Entity();

            User existingUser = db.Users.SingleOrDefault(x => x.Email == user.Email);

            var message = Request.CreateResponse(HttpStatusCode.OK);

            if(existingUser == null)
            {
                // TODO: perform queries to insert new user into database.
                User newUser = new User();
                newUser.Email = user.Email;
                newUser.Pass  = user.Pass;

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

        // PUT api/values/5
        public void Put(int id, [FromBody]string value)
        {
       
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
          
        }
    }
}

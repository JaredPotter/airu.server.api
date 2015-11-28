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
            var db = new AirU_Database_Entity();

            List<User> allUsers = db.Users.Select(x => x).ToList<User>();
            List<string> allUsersString = new List<string>();
 
            foreach(var item in allUsers)
            {
                allUsersString.Add(item.Email);
            }

            return allUsersString;
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
            var db = new AirU_Database_Entity();

            User registeredUser = db.Users.SingleOrDefault(x => x.Email == email);

            if (registeredUser != null)
            {
                // User with email address: <email> does exsit.
                return Request.CreateResponse<string>(HttpStatusCode.OK, "User with email addres: = " + registeredUser.Email + " does exist.");
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
        [Route("api/ams/AMSData")]
        [HttpGet]
        public HttpResponseMessage GetAMSData()
        {
            var db = new AirU_Database_Entity();

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
            var db = new AirU_Database_Entity();

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
            var db = new AirU_Database_Entity();

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
        [Route("api/ams")]
        [HttpPost]
        public HttpResponseMessage AddAMSDevice([FromBody] Device device)
        {
            var db = new AirU_Database_Entity();

            Device newDevice = new Device();
            newDevice.DeviceID = device.DeviceID;           // Ex. "ZZ-ZZ-ZZ-JJ-JJ-JJ".
            newDevice.DevicePrivacy = device.DevicePrivacy; // Ex. false.
            newDevice.Email = device.Email;                 // Ex. "steve@jobs.com";
            db.Devices.Add(newDevice);

            db.SaveChanges();

            var message = Request.CreateResponse(HttpStatusCode.OK, "Successfully added device: \n\tDeviceID = "      + newDevice.DeviceID +
                                                                                               "\n\tDevicePrivacy = " + newDevice.DevicePrivacy +
                                                                                               "\n\tEmail = "         + newDevice.Email);
            return message;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="value"></param>
        [Route("api/ams")]
        [HttpPut]
        public void Put(int id, [FromBody]string value)
        {
       
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        [Route("api/ams")]
        [HttpDelete]
        public void Delete(int id)
        {
          
        }
    }
}

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace server_api.Controllers
{
    public class AMSController : ApiController
    {

        [Route("api/ams")]
        [HttpGet]
        public IEnumerable<string> AMSGet()
        {
            return null;
        }

        [Route("api/ams/{id}")]
        [HttpGet]
        public HttpResponseMessage Get(int id)
        {
            var db = new AirU_Database_Entity();

    
            //return Request.CreateResponse<string>(HttpStatusCode.OK);

            return null;
            //return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Item not found");
           
        }

        // POST api/values
        //public HttpResponseMessage Post([FromBody]AMSDataSet dataSet)
        //{


        //    string json = JsonConvert.SerializeObject(dataSet);
        //    Console.WriteLine(json);
        //    var message = Request.CreateResponse(HttpStatusCode.OK);
        //    message.Content = new StringContent(json);
        //    return message;
        //}

        // POST api/values
        public HttpResponseMessage Post([FromBody]AMSState state)
        {
            string json = JsonConvert.SerializeObject(state);
            Console.WriteLine(json);
            var message = Request.CreateResponse(HttpStatusCode.OK);
            message.Content = new StringContent(json);
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

    public class AMSDataSet
    {
        
        public Pressure[] Pressure { get; set; }
        public string MAC { get; set; }
        public Temperature[] Temperature { get; set; }
        public Altitude[] Altitude { get; set; }
        public Humidity[] Humidity { get; set; }
    }

    public class AMSState
    {
        public string MAC { get; set; }
        public State[] State { get; set; }
    }

    public class State
    {
        public string latitude { get; set; }
        public string date { get; set; }
        public string longitude { get; set; }
    }

    public class Temperature
    {
        public string date { get; set; }
        public string unit { get; set; }
        public string value { get; set; }
    }

    public class Pressure
    {
        public string date { get; set; }
        public string unit { get; set; }
        public string value { get; set; }
    }

    public class Altitude
    {
        public string date { get; set; }
        public string unit { get; set; }
        public string value { get; set; }
    }

    public class Humidity
    {
        public string date { get; set; }
        public string unit { get; set; }
        public string value { get; set; }
    }
}

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;

namespace server_api.Controllers
{
    public class AMSController : ApiController
    {
        static List<string> data = initList();

        private static List<string> initList()
        {
            var ret = new List<string>();
            ret.Add("Taylor Wilson");
            ret.Add("Zach Lobato");
            ret.Add("Jared Moore");
            ret.Add("Jared Potter");
            return ret;
        }

        // GET api/values
        public IEnumerable<string> Get()
        {
            return data;
        }

        // GET api/values/5
        public HttpResponseMessage Get(int id)
        {
            var db = new AirU_Database_Entity();

            if (data.Count > id)
            {
                return Request.CreateResponse<string>(HttpStatusCode.OK, data[id]);
            }
            else
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Item not found");
            }
        }

        // POST api/values
        //public HttpResponseMessage Post([FromBody]AMSDataSet dataSet)
        //{

        //    data.Add(dataSet.MAC);

        //    string json = JsonConvert.SerializeObject(dataSet);
        //    Console.WriteLine(json);
        //    var message = Request.CreateResponse(HttpStatusCode.OK);
        //    message.Content = new StringContent(json);
        //    return message;
        //}

        // POST api/values
        public HttpResponseMessage Post([FromBody]AMSState state)
        {

            data.Add(state.MAC);

            string json = JsonConvert.SerializeObject(state);
            Console.WriteLine(json);
            var message = Request.CreateResponse(HttpStatusCode.OK);
            message.Content = new StringContent(json);
            return message;
        }

        // PUT api/values/5
        public void Put(int id, [FromBody]string value)
        {
            data[id] = value;
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
            data.RemoveAt(id);
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

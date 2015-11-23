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
    public class FrontEndController : ApiController
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
            if(data.Count > id)
            {
                return Request.CreateResponse<string>(HttpStatusCode.OK, data[id]);
            }
            else
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Item not found");
            }
        }

        // POST api/values
        public HttpResponseMessage Post([FromBody]FrontEndDataSet dataSet)
        {

            data.Add(dataSet.firstName + dataSet.lastName);

            var message = Request.CreateResponse(HttpStatusCode.OK);
            message.Headers.Location = new Uri(Request.RequestUri + data.Count.ToString());
            return message;
        }

        //public HttpResponseMessage Post([FromBody]FrontEndDataSet[] dataSet)
        //{

        //    foreach (var item in dataSet)
        //    {
        //        data.Add(item.firstName + item.lastName);
        //    }

        //    var message = Request.CreateResponse(HttpStatusCode.OK);
        //    message.Headers.Location = new Uri(Request.RequestUri + data.Count.ToString());
        //    return message;
        //}

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

    public class FrontEndDataSet
    {
        public string firstName { get; set; }
        public string lastName { get; set; }
    }
}

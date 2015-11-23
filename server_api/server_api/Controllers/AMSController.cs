using System;
using System.Collections.Generic;
using System.Linq;
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
        public string Get(int id)
        {
            return data[id];
        }

        // POST api/values
        public void Post([FromBody]AMSDataSet[] dataSet)
        {
            foreach(var item in dataSet)
            {
                data.Add(item.firstName + item.lastName);
            }
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
        public string firstName {get; set;}
        public string lastName {get; set;}
    }
}

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace server_api.Models
{
    public class SwaggerResponseModels
    {
    }

    public class SwaggerUser
    {
        public string email {get; set;}
        public SwaggerUser(string Email)
        {
            email = Email;
        }
    }

    public class SwaggerPollutantList
    {
        public string key {get; set;}

        public List<object[]> values { get; set; }

        public SwaggerPollutantList(string pollutantName)
        {
            key = pollutantName;
            values = new List<object[]>();
            
        }
    }
}
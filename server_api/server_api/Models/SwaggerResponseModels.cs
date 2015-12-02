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

    public class SwaggerAMSList
    {
        public List<SwaggerDevice> ams;

        public SwaggerAMSList()
        {
            this.ams = new List<SwaggerDevice>();
        }

        public void AddSwaggerDevice(String deviceID, decimal lat, decimal lng)
        {
            ams.Add(new SwaggerDevice(deviceID, lat, lng));
        }


        public class SwaggerDevice
        {
            public String deviceID { get; set; }
            public decimal lat { get; set; }
            public decimal lng { get; set; }

            public SwaggerDevice(String deviceID, decimal lat, decimal lng)
            {
                this.deviceID = deviceID;
                this.lat = lat;
                this.lng = lng;
            }

        }

        
    }


}
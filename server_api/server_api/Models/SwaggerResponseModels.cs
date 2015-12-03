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

    /// <summary>
    ///   Used to return Device locations on the map view.
    /// 
    ///   {
    ///   "ams": [{
    ///               "deviceID": "mac_addr",
    ///               "lat": 40,
    ///               "lng": -111
    ///            }, {
    ///               "deviceID": "mac_addr",
    ///               "lat": 40,
    ///               "lng": -111
    ///           }]
    ///    }
    /// </summary>
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

    public class SwaggerHeatMapValueList
    {
        public string pollutant { get; set; }
        public List<SwaggerCoordinateAndValue> values;

        public SwaggerHeatMapValueList(string pollutant)
        {
            this.pollutant = pollutant;
            values = new List<SwaggerCoordinateAndValue>();
        }

        public void AddSwaggerCoordinateAndValue(decimal lat, decimal lng, double value)
        {
            values.Add(new SwaggerCoordinateAndValue(lat, lng, value));
        }


        public class SwaggerCoordinateAndValue
        {
            public decimal lat { get; set; }
            public decimal lng { get; set; }
            public double value { get; set; }

            public SwaggerCoordinateAndValue(decimal lat, decimal lng, double value)
            {
                this.lat = lat;
                this.lng = lng;
                this.value = value;
            }
        }
    }

    /// <summary>
    ///  Used to return values to the details pane.
    /// 
    ///  {
    ///  "latest": [{
    ///               "pollutantName": "pName",
    ///               "value": 40
    ///              }, {
    ///               "pollutantNAme": "pName",
    ///               "value": 40,
    ///             }]
    ///   }
    /// </summary>
    public class SwaggerLatestPollutantsList
    {
        public List<PollutantAndValue> latest;

        public SwaggerLatestPollutantsList()
        {
            this.latest = new List<PollutantAndValue>();
        }

        public void AddPollutantAndValue(String pollutantName, double value)
        {
            latest.Add(new PollutantAndValue(pollutantName, value));
        }


        public class PollutantAndValue
        {
            public String pollutantName { get; set; }
            public double value { get; set; }

            public PollutantAndValue(String pollutantName, double value)
            {
                this.pollutantName = pollutantName;
                this.value = value;
            }

        }
    }

}
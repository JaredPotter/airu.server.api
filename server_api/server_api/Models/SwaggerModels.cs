using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace server_api.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class SwaggerModels
    {
    }

    /// <summary>
    /// 
    /// </summary>
    public class SwaggerUsers
    {
        /// <summary>
        /// 
        /// </summary>
        public string email {get; set;}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Email"></param>
        public SwaggerUsers(string Email)
        {
            email = Email;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class SwaggerUser
    {
        /// <summary>
        /// 
        /// </summary>
        public string email {get; set;}

        /// <summary>
        /// 
        /// </summary>
        public string pass { get; set; }
    }

    /// <summary>
    /// {
	/// "Name": "string",
	/// "Id": "string",
	/// "private": false,
	/// "Purpose": "string",
	/// "Outdoor": true
    /// }
    /// </summary>
    public class SwaggerDeviceAndState
    {
        /// <summary>
        /// 
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [StringLength(17)]
        public string Id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool Private { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Purpose { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool Indoor { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class SwaggerPollutantList
    {
        /// <summary>
        /// 
        /// </summary>
        public string key {get; set;}

        /// <summary>
        /// 
        /// </summary>
        public List<object[]> values { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pollutantName"></param>
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
        /// <summary>
        /// 
        /// </summary>
        public List<SwaggerDevice> ams;

        /// <summary>
        /// 
        /// </summary>
        public SwaggerAMSList()
        {
            this.ams = new List<SwaggerDevice>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="deviceID"></param>
        /// <param name="lat"></param>
        /// <param name="lng"></param>
        public void AddSwaggerDevice(String deviceID, decimal lat, decimal lng)
        {
            ams.Add(new SwaggerDevice(deviceID, lat, lng));
        }

        /// <summary>
        /// 
        /// </summary>
        public class SwaggerDevice
        {
            /// <summary>
            /// 
            /// </summary>
            public String deviceID { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public decimal lat { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public decimal lng { get; set; }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="deviceID"></param>
            /// <param name="lat"></param>
            /// <param name="lng"></param>
            public SwaggerDevice(String deviceID, decimal lat, decimal lng)
            {
                this.deviceID = deviceID;
                this.lat = lat;
                this.lng = lng;
            }

        }
    }
    /// <summary>
    /// This class stores both the NE and SW bounds sent from a
    /// map view.
    /// </summary>
    public class SwaggerMapParameters
    {
        /// <summary>
        /// NE coordinates
        /// </summary>
        public SwaggerCoordinate northEast { get; set; }

        /// <summary>
        /// SW coordinates
        /// </summary>
        public SwaggerCoordinate southWest { get; set; }
    }

    /// <summary>
    /// This class represents a coordinate, which contains both a
    /// latitude and longitude;
    /// </summary>
    public class SwaggerCoordinate
    {
        /// <summary>
        /// Latitude
        /// </summary>
        public decimal lat { get; set; }

        /// <summary>
        /// Longitude
        /// </summary>
        public decimal lng { get; set; }
    }

    /// <summary>
    /// This stores both the NE and SW bounds sent from a 
    /// map view, and combines them with the name of a pollutant.
    /// </summary>
    public class SwaggerHeatMapParameters
    {
        /// <summary>
        /// NE and SW bound
        /// </summary>
        public SwaggerMapParameters mapParameters { get; set; }

        /// <summary>
        /// Pollutant Name
        /// </summary>
        public string pollutantName { get; set; }
    }

    public class SwaggerHeatMapValueList
    {
        /// <summary>
        /// 
        /// </summary>
        public string pollutant { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public List<SwaggerCoordinateAndValue> values;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pollutant"></param>
        public SwaggerHeatMapValueList(string pollutant)
        {
            this.pollutant = pollutant;
            values = new List<SwaggerCoordinateAndValue>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lat"></param>
        /// <param name="lng"></param>
        /// <param name="value"></param>
        public void AddSwaggerCoordinateAndValue(decimal lat, decimal lng, double value)
        {
            values.Add(new SwaggerCoordinateAndValue(lat, lng, value));
        }

        /// <summary>
        /// 
        /// </summary>
        public class SwaggerCoordinateAndValue
        {
            /// <summary>
            /// 
            /// </summary>
            public decimal lat { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public decimal lng { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public double value { get; set; }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="lat"></param>
            /// <param name="lng"></param>
            /// <param name="value"></param>
            public SwaggerCoordinateAndValue(decimal lat, decimal lng, double value)
            {
                this.lat = lat;
                this.lng = lng;
                this.value = value;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class SwaggerLatestDataPoints
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string co { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string pm { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string co2 { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string no2 { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string o3 { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string temp { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string humidity { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string pressure { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string altitude { get; set; }
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
        /// <summary>
        /// 
        /// </summary>
        public List<PollutantAndValue> latest;

        /// <summary>
        /// 
        /// </summary>
        public SwaggerLatestPollutantsList()
        {
            this.latest = new List<PollutantAndValue>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pollutantName"></param>
        /// <param name="value"></param>
        public void AddPollutantAndValue(String pollutantName, double value)
        {
            latest.Add(new PollutantAndValue(pollutantName, value));
        }

        /// <summary>
        /// 
        /// </summary>
        public class PollutantAndValue
        {
            /// <summary>
            /// 
            /// </summary>
            public String pollutantName { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public double value { get; set; }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="pollutantName"></param>
            /// <param name="value"></param>
            public PollutantAndValue(String pollutantName, double value)
            {
                this.pollutantName = pollutantName;
                this.value = value;
            }
        }
    }
}
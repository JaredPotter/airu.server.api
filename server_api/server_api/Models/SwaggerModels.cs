using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;

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
    [System.Xml.Serialization.XmlRoot("air_quality_data")]
    public class SwaggerDAQData
    {
        public string state { get; set; }
        public Site site { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlRoot("site")]
        public class Site
        {
            public string name { get; set; }

            [XmlElement("data")]
            public Data[] data { get; set; }

            public double latitude { get; set; }
            public double longitude { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlRoot("data")]
        public class Data
        {
            public string date { get; set; }
            public string ozone { get; set; }
            public string ozone_8hr_avg { get; set; }
            public string pm25 { get; set; }
            public string pm25_24hr_avg { get; set; }
            public string nox { get; set; }
            public string no2 { get; set; }
            public string temperature { get; set; }
            public string relative_humidity { get; set; }
            public string wind_speed { get; set; }
            public string wind_direction { get; set; }
            public string co { get; set; }
            public string solar_radiation { get; set; }
            public string so2 { get; set; }
            public string noy { get; set; }
            public string bp { get; set; }
            public string pm10 { get; set; }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class SwaggerAQIData
    {
        public string DateObserved { get; set; }
        public int HourObserved { get; set; }
        public string LocalTimeZone { get; set; }
        public string ReportingArea { get; set; }
        public string StateCode { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public string ParameterName { get; set; }
        public int AQI { get; set; }
        public Category Category;
    }

    /// <summary>
    /// 
    /// </summary>
    public class Category
    {
        public int Number { get; set; }
        public string Name { get; set; }
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
    public class SwaggerDeviceState
    {
        /// <summary>
        /// 
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [StringLength(17)]
        public string id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool privacy { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string purpose { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool indoor { get; set; }

        public Location location;

        public string owner { get; set; }

        public SwaggerDeviceState(string name, string id, bool privacy, string purpose, bool indoor, decimal lat, decimal lng, string owner)
        {
            this.name = name;
            this.id = id;
            this.privacy = privacy;
            this.purpose = purpose;
            this.indoor = indoor;
            this.owner = owner;
            this.location = new Location(lat, lng);
        }

        public class Location
        {
            public decimal lat { get; set; }
            public decimal lng { get; set; }

            public Location(decimal lat, decimal lng)
            {
                this.lat = lat;
                this.lng = lng;
            }
        }
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
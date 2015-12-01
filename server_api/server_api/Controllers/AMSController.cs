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
    /// <summary>
    /// 
    /// </summary>
    public class AMSController : ApiController
    {

        /// <summary>
        ///  Possibly change to Put ("Update") vs Post ("Create/Add"). 
        /// </summary>
        /// <param name="deviceState"></param>
        /// <returns></returns>
        //[Route("api/ams")]
        //[HttpPost]
        //public HttpResponseMessage UpdateAMSDeviceState([FromBody]DeviceState deviceState)
        //{
        //    var db = new AirU_Database_Entity();

        //    Device device = db.Devices.SingleOrDefault(x => x.DeviceID == deviceState.DeviceID);

        //    if(device == null)
        //    {
        //        return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Failed to add device state with Device with ID = " + deviceState.DeviceID + " not found.");
        //    }

        //    DeviceState newDeviceState = new DeviceState();
        //    newDeviceState.DeviceID = deviceState.DeviceID;         // Ex. "ZZ-ZZ-ZZ-JJ-JJ-JJ";
        //    newDeviceState.InOrOut = deviceState.InOrOut;           // Ex. false;
        //    newDeviceState.StatePrivacy = deviceState.StatePrivacy; // Ex.  false;
        //    newDeviceState.StateTime = deviceState.StateTime;       // Ex. new DateTime(2015, 11, 25, 13, 16, 1);
        //    newDeviceState.Long = deviceState.Long;                 // Ex. 123.456789m;
        //    newDeviceState.Lat = deviceState.Lat;                   // Ex. 87.1224m;
        //    db.DeviceStates.Add(newDeviceState);

        //    db.SaveChanges();

        //    var message = Request.CreateResponse(HttpStatusCode.OK, "Successfully added device state: \n\tDevice ID = "    + newDeviceState.DeviceID  + 
        //                                                                                             "\n\t1nOr0ut = "      + newDeviceState.InOrOut +
        //                                                                                             "\n\tStatePrivacy = " + newDeviceState.StatePrivacy + 
        //                                                                                             "\n\tStartTieme = "   + newDeviceState.StateTime + 
        //                                                                                             "\n\tLong = "         + newDeviceState.Long +
        //                                                                                             "\n\tLat = "          + newDeviceState.Lat);
        //    return message;
        //}

        /// <summary>
        /// </summary>
        /// <param name="dataSet">AMSDataSet Model</param>
        /// <returns></returns>
        [Route("api/ams/data")]
        [HttpPost]
        public HttpResponseMessage AddAMSDataSet([FromBody]DataPoint[] dataSet)
        {
            var db = new AirU_Database_Entity();

            db.DataPoints.AddRange(dataSet);

            db.SaveChanges();

            //string json = JsonConvert.SerializeObject(dataSet);
           // Console.WriteLine(json);
            var message = Request.CreateResponse(HttpStatusCode.OK);
            //message.Content = new StringContent(json);
            return message;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        [Route("api/ams/state")]
        [HttpPost]
        public HttpResponseMessage UpdateAMSDeviceState([FromBody]DeviceState[] states)
        {
            var db = new AirU_Database_Entity();

            Device device = db.Devices.SingleOrDefault(x => x.DeviceID == states[0].DeviceID);

            if (device == null)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Failed to add device state with Device with ID = " + states[0].DeviceID + " not found.");
            }

            db.DeviceStates.AddRange(states);

            db.SaveChanges();

            var message = Request.CreateResponse(HttpStatusCode.OK);

            return message;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="value"></param>
        [Route("api/ams")]
        [HttpPut]
        public void PutAMSData(int id, [FromBody]string value)
        {

        }

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="id"></param>
        [Route("api/ams")]
        [HttpDelete]
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
        public decimal latitude { get; set; }
        public DateTime date { get; set; }
        public decimal longitude { get; set; }
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

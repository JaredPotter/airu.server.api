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
        /// </summary>
        /// <param name="dataSet">AMSDataSet Model</param>
        /// <returns></returns>
        [Route("ams/data")]
        [HttpPost]
        public IHttpActionResult AddAMSDataSet([FromBody]DataPoint[] dataSet)
        {
            var db = new AirUDatabaseCOE();

            db.DataPoints.AddRange(dataSet);

            db.SaveChanges();

            return Ok(dataSet);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="state">*xml comment*</param>
        /// <returns></returns>
        [Route("ams/state")]
        [HttpPost]
        public IHttpActionResult UpdateAMSDeviceState([FromBody]DeviceState[] states)
        {
            var db = new AirUDatabaseCOE();
            string deviceID = states[0].DeviceID;
            Device device = db.Devices.SingleOrDefault(x => x.DeviceID == deviceID);

            if (device == null)
            {
                // Failed to add DeviceState.
                return Ok("Failed to add device state with Device with ID = " + states[0].DeviceID + " not found.");                
            }

            db.DeviceStates.AddRange(states);

            db.SaveChanges();

            // Success.
            return Ok(states);
        }

        /// <summary>
        /// *xml comment*
        /// </summary>
        /// <param name="id"></param>
        /// <param name="value"></param>
        [Route("ams")]
        [HttpPut]
        public void PutAMSData(int id, [FromBody]string value)
        {

        }

        /// <summary>
        /// *xml comment*
        /// </summary>
        /// <param name="id"></param>
        [Route("ams")]
        [HttpDelete]
        public void Delete(int id)
        {

        }
    }
}

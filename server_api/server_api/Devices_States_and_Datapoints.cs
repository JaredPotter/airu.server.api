namespace server_api
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Devices_States_and_Datapoints
    {
        [StringLength(17)]
        public string DeviceID { get; set; }

        public DateTime? StateTime { get; set; }

        [Key]
        [Column(Order = 0)]
        public DateTime MeasurementTime { get; set; }

        public decimal? Lat { get; set; }

        public decimal? Long { get; set; }

        public bool? InOrOut { get; set; }

        public bool? StatePrivacy { get; set; }

        [Key]
        [Column(Order = 1)]
        public double Value { get; set; }

        [Key]
        [Column(Order = 2)]
        [StringLength(30)]
        public string PollutantName { get; set; }
    }
}

namespace server_api
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class DataPoint
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(17)]
        public string DeviceID { get; set; }

        [Key]
        [Column(Order = 1)]
        public DateTime MeasurementTime { get; set; }

        public double Value { get; set; }

        [Key]
        [Column(Order = 2)]
        [StringLength(30)]
        public string PollutantName { get; set; }

        public virtual Device Device { get; set; }

        public virtual Pollutant Pollutant { get; set; }
    }
}

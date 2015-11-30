namespace server_api
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class DeviceState
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(17)]
        public string DeviceID { get; set; }

        [Key]
        [Column(Order = 1)]
        public DateTime StateTime { get; set; }

        public decimal Lat { get; set; }

        public decimal Long { get; set; }

        public bool InOrOut { get; set; }

        public bool StatePrivacy { get; set; }

        public virtual Device Device { get; set; }
    }
}

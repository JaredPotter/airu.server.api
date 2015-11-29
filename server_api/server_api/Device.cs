namespace server_api
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Device
    {
        public Device()
        {
            DataPoints = new HashSet<DataPoint>();
            DeviceStates = new HashSet<DeviceState>();
            DeviceGroups = new HashSet<DeviceGroup>();
        }

        [StringLength(17)]
        public string DeviceID { get; set; }

        [Required]
        [StringLength(320)]
        public string Email { get; set; }

        public bool DevicePrivacy { get; set; }

        public virtual ICollection<DataPoint> DataPoints { get; set; }

        public virtual User User { get; set; }

        public virtual ICollection<DeviceState> DeviceStates { get; set; }

        public virtual ICollection<DeviceGroup> DeviceGroups { get; set; }
    }
}

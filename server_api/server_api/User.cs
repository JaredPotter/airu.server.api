namespace server_api
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class User
    {
        public User()
        {
            DeviceGroups = new HashSet<DeviceGroup>();
            Devices = new HashSet<Device>();
        }

        [Key]
        [StringLength(320)]
        public string Email { get; set; }

        [Required]
        [StringLength(20)]
        public string Pass { get; set; }

        public virtual ICollection<DeviceGroup> DeviceGroups { get; set; }

        public virtual ICollection<Device> Devices { get; set; }
    }
}

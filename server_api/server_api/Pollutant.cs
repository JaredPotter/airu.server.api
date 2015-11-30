namespace server_api
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Pollutant
    {
        public Pollutant()
        {
            DataPoints = new HashSet<DataPoint>();
        }

        [Key]
        [StringLength(30)]
        public string PollutantName { get; set; }

        public virtual ICollection<DataPoint> DataPoints { get; set; }
    }
}

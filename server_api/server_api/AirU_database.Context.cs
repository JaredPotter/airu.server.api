﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace server_api
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class AirU_Database_Entity : DbContext
    {
        public AirU_Database_Entity()
            : base("name=AirU_Database_Entity")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<DataPoint> DataPoints { get; set; }
        public virtual DbSet<DeviceGroup> DeviceGroups { get; set; }
        public virtual DbSet<Device> Devices { get; set; }
        public virtual DbSet<DeviceState> DeviceStates { get; set; }
        public virtual DbSet<Pollutant> Pollutants { get; set; }
        public virtual DbSet<User> Users { get; set; }
    }
}

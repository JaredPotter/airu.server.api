namespace server_api
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class AirUDBCOE : DbContext
    {
        public AirUDBCOE()
            : base("name=AirUDBCOE")
        {
        }

        public virtual DbSet<DataPoint> DataPoints { get; set; }
        public virtual DbSet<DeviceGroup> DeviceGroups { get; set; }
        public virtual DbSet<Device> Devices { get; set; }
        public virtual DbSet<DeviceState> DeviceStates { get; set; }
        public virtual DbSet<Pollutant> Pollutants { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Devices_States_and_Datapoints> Devices_States_and_Datapoints { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DataPoint>()
                .Property(e => e.DeviceID)
                .IsUnicode(false);

            modelBuilder.Entity<DataPoint>()
                .Property(e => e.PollutantName)
                .IsUnicode(false);

            modelBuilder.Entity<DeviceGroup>()
                .Property(e => e.GroupName)
                .IsUnicode(false);

            modelBuilder.Entity<DeviceGroup>()
                .Property(e => e.Email)
                .IsUnicode(false);

            modelBuilder.Entity<Device>()
                .Property(e => e.DeviceID)
                .IsUnicode(false);

            modelBuilder.Entity<Device>()
                .Property(e => e.Email)
                .IsUnicode(false);

            modelBuilder.Entity<Device>()
                .HasMany(e => e.DataPoints)
                .WithRequired(e => e.Device)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Device>()
                .HasMany(e => e.DeviceStates)
                .WithRequired(e => e.Device)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Device>()
                .HasMany(e => e.DeviceGroups)
                .WithMany(e => e.Devices)
                .Map(m => m.ToTable("DevicesAndGroups").MapLeftKey("DeviceID").MapRightKey(new[] { "GroupName", "g_Email" }));

            modelBuilder.Entity<DeviceState>()
                .Property(e => e.DeviceID)
                .IsUnicode(false);

            modelBuilder.Entity<DeviceState>()
                .Property(e => e.Lat)
                .HasPrecision(9, 6);

            modelBuilder.Entity<DeviceState>()
                .Property(e => e.Long)
                .HasPrecision(9, 6);

            modelBuilder.Entity<Pollutant>()
                .Property(e => e.PollutantName)
                .IsUnicode(false);

            modelBuilder.Entity<Pollutant>()
                .HasMany(e => e.DataPoints)
                .WithRequired(e => e.Pollutant)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<User>()
                .Property(e => e.Email)
                .IsUnicode(false);

            modelBuilder.Entity<User>()
                .Property(e => e.Pass)
                .IsUnicode(false);

            modelBuilder.Entity<User>()
                .HasMany(e => e.DeviceGroups)
                .WithRequired(e => e.User)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<User>()
                .HasMany(e => e.Devices)
                .WithRequired(e => e.User)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Devices_States_and_Datapoints>()
                .Property(e => e.DeviceID)
                .IsUnicode(false);

            modelBuilder.Entity<Devices_States_and_Datapoints>()
                .Property(e => e.Lat)
                .HasPrecision(9, 6);

            modelBuilder.Entity<Devices_States_and_Datapoints>()
                .Property(e => e.Long)
                .HasPrecision(9, 6);

            modelBuilder.Entity<Devices_States_and_Datapoints>()
                .Property(e => e.PollutantName)
                .IsUnicode(false);
        }
    }
}

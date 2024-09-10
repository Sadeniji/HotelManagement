using HotelManagement.Data.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace HotelManagement.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
    {
        public DbSet<RoomType> RoomTypes { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Amenity> Amenities { get; set; }
        public DbSet<RoomTypeAmenity> RoomTypeAmenities { get; set; }
        public DbSet<Booking> Bookings { get; set; }

        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    base.OnConfiguring(optionsBuilder);
        //    optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        //}

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<RoomTypeAmenity>()
                .HasKey(ra => new { ra.RoomTypeId, ra.AmenityId });

            builder.Entity<RoomType>()
                .HasMany(rt => rt.Rooms)
                .WithOne(r => r.RoomType)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Room>()
                .HasIndex(r => r.RoomNumber)
                .IsUnique();
        }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder
                .Properties<Ulid>()
                .HaveConversion<UlidToStringConverter>()
                .HaveConversion<UlidToBytesConverter>();
        }
    }

    public class UlidToBytesConverter : ValueConverter<Ulid, byte[]>
    {
        private static readonly ConverterMappingHints defaultHints = new ConverterMappingHints(size: 16);

        public UlidToBytesConverter() : this(null)
        {
        }

        public UlidToBytesConverter(ConverterMappingHints mappingHints = null)
            : base(
                convertToProviderExpression: x => x.ToByteArray(),
                convertFromProviderExpression: x => new Ulid(x),
                mappingHints: defaultHints.With(mappingHints))
        {
        }
    }

    public class UlidToStringConverter : ValueConverter<Ulid, string>
    {
        private static readonly ConverterMappingHints defaultHints = new ConverterMappingHints(size: 26);

        public UlidToStringConverter() : this(null)
        {
        }

        public UlidToStringConverter(ConverterMappingHints mappingHints = null)
            : base(
                convertToProviderExpression: x => x.ToString(),
                convertFromProviderExpression: x => Ulid.Parse(x),
                mappingHints: defaultHints.With(mappingHints))
        {
        }
    }
}

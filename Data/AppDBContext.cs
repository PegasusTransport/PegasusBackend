using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PegasusBackend.Models;

namespace PegasusBackend.Data
{
    public class AppDBContext(DbContextOptions<AppDBContext> options) : IdentityDbContext<User>(options)
    {
        public DbSet<Drivers> Drivers { get; set; }
        public DbSet<Cars> Cars { get; set; }
        public DbSet<Bookings> Bookings { get; set; }
        public DbSet<TaxiSettings> TaxiSettings { get; set; }
        public DbSet<IdempotencyRecord> IdempotencyRecords { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var user = modelBuilder.Entity<User>();
            var driver = modelBuilder.Entity<Drivers>();
            var taxiSettings = modelBuilder.Entity<TaxiSettings>();
            var idempotencyRecord = modelBuilder.Entity<IdempotencyRecord>();

            user
                .HasOne(u => u.Driver)
                .WithOne(d => d.User)
                .HasForeignKey<Drivers>(d => d.UserId);

            driver
                .HasOne(d => d.Car)
                .WithOne(c => c.Driver)
                .HasForeignKey<Drivers>(d => d.CarId)
                .IsRequired(false);

            driver
                .HasMany(d => d.Bookings)
                .WithOne(b => b.Driver)
                .HasForeignKey(b => b.DriverIdFK);

            taxiSettings
                .Property(p => p.KmPrice)
                .HasColumnType("decimal(18,2)");

            taxiSettings
                .Property(p => p.MinutePrice)
                .HasColumnType("decimal(18,2)");

            taxiSettings
                .Property(p => p.StartPrice)
                .HasColumnType("decimal(18,2)");

            taxiSettings
                .Property(p => p.ZonePrice)
                .HasColumnType("decimal(18,2)");

            taxiSettings
                .Property(p => p.UpdatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            idempotencyRecord
                .HasIndex(i => i.IdempotencyKey)
                .IsUnique(); // ← Viktigt! En key kan bara finnas en gång

            idempotencyRecord
                .HasIndex(i => i.ExpiresAt); // ← För snabb cleanup av gamla records

            idempotencyRecord
                .HasOne(i => i.Booking)
                .WithMany()
                .HasForeignKey(i => i.BookingId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

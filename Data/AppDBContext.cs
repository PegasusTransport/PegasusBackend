using Microsoft.EntityFrameworkCore;
using PegasusBackend.Models;

namespace PegasusBackend.Data
{
    public class AppDBContext : DbContext
    {
        public AppDBContext(DbContextOptions<AppDBContext> options) : base(options)
        {
        }

        public DbSet<Users> Users { get; set; }
        public DbSet<Drivers> Drivers { get; set; }
        public DbSet<Customers> Customers { get; set; }
        public DbSet<Cars> Cars { get; set; }
        public DbSet<Bookings> Bookings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var user = modelBuilder.Entity<Users>();
            var driver = modelBuilder.Entity<Drivers>();
            var customer = modelBuilder.Entity<Customers>();

            user
                .HasOne(u => u.Driver)
                .WithOne(d => d.User)
                .HasForeignKey<Drivers>(d => d.UserId);

            user
                .HasOne(u => u.Customer)
                .WithOne(c => c.User)
                .HasForeignKey<Customers>(c => c.UserIdFK);

            driver
                .HasOne(d => d.Car)
                .WithOne(c => c.Driver)
                .HasForeignKey<Drivers>(d => d.CarId);

            customer
                .HasMany(c => c.Bookings)
                .WithOne(b => b.Customer)
                .HasForeignKey(b => b.CustomerIdFK);

            driver
                .HasMany(d => d.Bookings)
                .WithOne(b => b.Driver)
                .HasForeignKey(b => b.DriverIdFK);

        }
    }
}

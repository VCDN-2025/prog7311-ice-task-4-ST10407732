using Ice_task_4.Models;
using Microsoft.EntityFrameworkCore;

namespace Ice_task_4.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Room> Rooms { get; set; }
        public DbSet<Guest> Guests { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Seed some initial data
            modelBuilder.Entity<Room>().HasData(
                new Room { RoomId = 1, RoomNumber = "101", RoomType = "Single", PricePerNight = 100.00m, IsAvailable = true },
                new Room { RoomId = 2, RoomNumber = "102", RoomType = "Double", PricePerNight = 150.00m, IsAvailable = true },
                new Room { RoomId = 3, RoomNumber = "103", RoomType = "Suite", PricePerNight = 250.00m, IsAvailable = true }
            );

            // Seed admin user
            modelBuilder.Entity<User>().HasData(
                new User { UserId = 1, Username = "admin", Password = "admin123", Role = "Admin" }
            );
        }
    }
}
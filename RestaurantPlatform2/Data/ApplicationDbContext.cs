using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RestaurantPlatform.Models;
using RestaurantPlatform2.Models;

namespace RestaurantPlatform2.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Dish> Dishes { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Review> Reviews { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Установяване на навигационната връзка между Dish и Review
            modelBuilder.Entity<Dish>()
                  .HasMany(d => d.Reviews)
                  .WithOne(r => r.Dish)
                  .HasForeignKey(r => r.DishId)
                  .OnDelete(DeleteBehavior.Cascade);

        }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<OrderStatusLog> OrderStatusLogs { get; set; }
        public DbSet<ContactMessage> ContactMessages { get; set; }
        public DbSet<WorkingHours> WorkingHours { get; set; }
        public DbSet<FavoriteDish> FavoriteDishes { get; set; }
        public DbSet<SiteSetting> SiteSettings { get; set; }
    }
}

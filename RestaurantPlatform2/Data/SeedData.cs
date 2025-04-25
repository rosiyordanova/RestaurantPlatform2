using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RestaurantPlatform.Models;
using RestaurantPlatform2.Data;

namespace RestaurantPlatform.Data
{
    public static class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>());

            // Ако вече има ястия – не прави нищо
            if (context.Dishes.Any())
                return;

            // Категории
            var soups = new Category { Name = "Супи" };
            var salads = new Category { Name = "Салати" };
            var desserts = new Category { Name = "Десерти" };

            context.Categories.AddRange(soups, salads, desserts);

            // Ястия
            context.Dishes.AddRange(
                new Dish
                {
                    Name = "Пилешка супа",
                    Description = "Класическа пилешка супа с фиде",
                    Price = 4.50m,
                    ImageUrl = "/images/soup.jpg",
                    Category = soups
                },
                new Dish
                {
                    Name = "Шопска салата",
                    Description = "Домат, краставица, сирене",
                    Price = 5.00m,
                    ImageUrl = "/images/shopska.jpg",
                    Category = salads
                },
                new Dish
                {
                    Name = "Тирамису",
                    Description = "Италиански десерт с кафе и маскарпоне",
                    Price = 6.50m,
                    ImageUrl = "/images/tiramisu.jpg",
                    Category = desserts
                }
            );

            context.SaveChanges();
        }
        public static async Task InitializeRolesAndAdminAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

            // Създаване на роля Admin, ако не съществува
            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                await roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            // Проверка за съществуващ админ
            string adminEmail = "admin@restaurant.bg";
            string adminPassword = "Admin123!";

            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new IdentityUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
        }
    }
}

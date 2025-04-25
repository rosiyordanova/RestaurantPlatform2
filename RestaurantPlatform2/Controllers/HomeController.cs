using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantPlatform.Models;
using RestaurantPlatform2.Data; // �������� �� ApplicationDbContext
using RestaurantPlatform2.Models;
using RestaurantPlatform.Models.ViewModels;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace RestaurantPlatform2.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public HomeController(
            ILogger<HomeController> logger,
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            // �������� ������������� �� ���� � ���������
            var adminEmail = "admin@restaurant.com";
            var adminUser = await _userManager.FindByEmailAsync(adminEmail);

            // �������� ���� ���� "Admin" ����������
            var roleManager = HttpContext.RequestServices.GetService<RoleManager<IdentityRole>>();
            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                await roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            // �������� �� ����������� ��� ������
            if (adminUser != null && !await _userManager.IsInRoleAsync(adminUser, "Admin"))
            {
                await _userManager.AddToRoleAsync(adminUser, "Admin");
                TempData["Success"] = "������� �� ������� ���� �������������.";
            }

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpGet]
        public IActionResult Location()
        {
            return View(new ContactMessageViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Location(ContactMessageViewModel model)
        {
            if (ModelState.IsValid)
            {
                var message = new ContactMessage
                {
                    Name = model.Name,
                    Email = model.Email,
                    Message = model.Message
                };

                _context.ContactMessages.Add(message);
                await _context.SaveChangesAsync();

                TempData["Success"] = "����������� ���� ��������� �������!";
                return RedirectToAction("Location");
            }

            return View(model);
        }
        [Authorize]
        [Authorize]
        public async Task<IActionResult> Recommendations()
        {
            var user = await _userManager.GetUserAsync(User);

            // ��������� �� ������� �� �����������
            var topUserDishes = await _context.OrderItems
                .Where(oi => oi.Order.UserId == user.Id)
                .GroupBy(oi => oi.DishId)
                .Select(g => new
                {
                    DishId = g.Key,
                    Count = g.Sum(i => i.Quantity)
                })
                .OrderByDescending(g => g.Count)
                .Take(3)
                .Join(_context.Dishes,
                      g => g.DishId,
                      d => d.Id,
                      (g, d) => d)
                .ToListAsync();

            // ��� ���� ����� ������� ? ����� ��������� ����� ���� ������
            if (!topUserDishes.Any())
            {
                topUserDishes = await _context.OrderItems
                    .GroupBy(oi => oi.DishId)
                    .Select(g => new
                    {
                        DishId = g.Key,
                        Count = g.Sum(i => i.Quantity)
                    })
                    .OrderByDescending(g => g.Count)
                    .Take(3)
                    .Join(_context.Dishes,
                          g => g.DishId,
                          d => d.Id,
                          (g, d) => d)
                    .ToListAsync();

                ViewBag.IsGlobal = true; // ��������� ��������� � �������
            }

            return View(topUserDishes);
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantPlatform.Data;
using RestaurantPlatform2.Data;
using System.Globalization;

namespace RestaurantPlatform.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Dashboard()
        {
            ViewBag.DishCount = _context.Dishes.Count();
            ViewBag.OrderCount = _context.Orders.Count();
            ViewBag.ReservationCount = _context.Reservations.Count();
            ViewBag.UserCount = _context.Users.Count();

            return View();
        }

        // API за поръчки по дни
        [HttpGet]
        public async Task<IActionResult> GetOrdersPerDay()
        {
            var data = await _context.Orders
                .GroupBy(o => o.OrderDate.Date)
                .Select(g => new
                {
                    Date = g.Key.ToString("dd.MM"),
                    Count = g.Count()
                })
                .OrderBy(d => d.Date)
                .ToListAsync();

            return Json(data);
        }

        // API за топ ястия
        [HttpGet]
        public async Task<IActionResult> GetTopDishes()
        {
            var data = await _context.OrderItems
                .GroupBy(i => i.Dish.Name)
                .Select(g => new
                {
                    Dish = g.Key,
                    Quantity = g.Sum(x => x.Quantity)
                })
                .OrderByDescending(d => d.Quantity)
                .Take(5)
                .ToListAsync();

            return Json(data);
        }
        [HttpGet]
        public IActionResult OrdersPerDay()
        {
            var data = _context.Orders
                .GroupBy(o => o.OrderDate.Date)
                .Select(g => new
                {
                    Date = g.Key.ToString("dd.MM"),
                    Count = g.Count()
                })
                .OrderBy(x => x.Date)
                .ToList();

            return Json(data);
        }

        [HttpGet]
        public IActionResult TopDishes()
        {
            var data = _context.OrderItems
                .GroupBy(i => i.Dish.Name)
                .Select(g => new
                {
                    Dish = g.Key,
                    Quantity = g.Sum(i => i.Quantity)
                })
                .OrderByDescending(x => x.Quantity)
                .Take(5)
                .ToList();

            return Json(data);
        }

        [HttpGet]
        public IActionResult ReservationsByHour()
        {
            var data = _context.Reservations
                .GroupBy(r => r.Time.Hours)
                .Select(g => new
                {
                    Hour = g.Key,
                    Count = g.Count()
                })
                .OrderBy(x => x.Hour)
                .ToList();

            return Json(data);
        }

    }
}

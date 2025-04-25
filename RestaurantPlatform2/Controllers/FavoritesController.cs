using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantPlatform.Models;
using RestaurantPlatform2.Data;

namespace RestaurantPlatform.Controllers
{
    [Authorize]
    public class FavoritesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public FavoritesController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Favorites
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var favorites = await _context.FavoriteDishes
                .Include(f => f.Dish)
                .Where(f => f.UserId == user.Id)
                .ToListAsync();

            return View(favorites);
        }

        // POST: Favorites/Add/5
        [HttpPost]
        public async Task<IActionResult> Add(int dishId)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);

                bool exists = await _context.FavoriteDishes
                    .AnyAsync(f => f.UserId == user.Id && f.DishId == dishId);

                if (!exists)
                {
                    var favorite = new FavoriteDish
                    {
                        UserId = user.Id,
                        DishId = dishId
                    };

                    _context.FavoriteDishes.Add(favorite);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Ястието беше добавено в любими.";
                }
            }
            catch
            {
                TempData["Error"] = "Възникна грешка. Моля, опитайте отново.";
            }

            return RedirectToAction("Index", "Dishes");
        }

        // POST: Favorites/Remove/5
        [HttpPost]
        public async Task<IActionResult> Remove(int dishId)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);

                var favorite = await _context.FavoriteDishes
                    .FirstOrDefaultAsync(f => f.UserId == user.Id && f.DishId == dishId);

                if (favorite != null)
                {
                    _context.FavoriteDishes.Remove(favorite);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Ястието беше премахнато от любими.";
                }
            }
            catch
            {
                TempData["Error"] = "Възникна грешка. Моля, опитайте отново.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantPlatform.Models.ViewModels;
using RestaurantPlatform2.Data;

namespace RestaurantPlatform.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ProfileController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Profile/MyAccount
        public async Task<IActionResult> MyAccount()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var model = new ProfileViewModel
            {
                Email = user.Email,
                TotalOrders = await _context.Orders.CountAsync(o => o.UserId == user.Id),
                TotalReservations = await _context.Reservations.CountAsync(r => r.UserId == user.Id),
                TotalFavorites = await _context.FavoriteDishes.CountAsync(f => f.UserId == user.Id)
            };

            return View(model);
        }
    }
}

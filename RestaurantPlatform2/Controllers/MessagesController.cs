using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantPlatform2.Data;

namespace RestaurantPlatform2.Controllers
{
    [Authorize(Roles = "Admin")]
    public class MessagesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MessagesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var messages = await _context.ContactMessages
                .OrderByDescending(m => m.SentAt)
                .ToListAsync();

            return View(messages);
        }

        public async Task<IActionResult> Details(int id)
        {
            var message = await _context.ContactMessages.FindAsync(id);
            if (message == null)
                return NotFound();

            return View(message);
        }
    }
}

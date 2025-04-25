using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantPlatform.Models;
using RestaurantPlatform2.Data;

namespace RestaurantPlatform.Controllers
{
    [Authorize(Roles = "Admin")]
    public class SiteSettingsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SiteSettingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: SiteSettings/Edit
        public async Task<IActionResult> Edit()
        {
            var setting = await _context.SiteSettings.FirstOrDefaultAsync();
            if (setting == null)
            {
                setting = new SiteSetting();
                _context.SiteSettings.Add(setting);
                await _context.SaveChangesAsync();
            }
            return View(setting);
        }

        // POST: SiteSettings/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SiteSetting setting)
        {
            var existing = await _context.SiteSettings.FirstOrDefaultAsync();
            if (existing == null) return NotFound();

            existing.GlobalMessage = setting.GlobalMessage;
            existing.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Настройките бяха запазени.";
            return RedirectToAction(nameof(Edit));
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantPlatform.Models;
using RestaurantPlatform2.Data;

namespace RestaurantPlatform2.Controllers
{
    [Authorize(Roles = "Admin")]
    public class WorkingHoursController : Controller
    {
        private readonly ApplicationDbContext _context;

        public WorkingHoursController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Списък с всички работни дни и часове
        // Списък с всички работни дни и часове
        public async Task<IActionResult> Index()
        {
            var hours = await _context.WorkingHours
                .OrderBy(h => h.Day)
                .ToListAsync();

            if (hours.Count == 0)
            {
                // Ако няма добавени работни часове, може да изведеш съобщение
                TempData["NoWorkingHours"] = "Няма добавени работни часове.";
            }

            return View(hours);
        }


        // Форма за добавяне
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(WorkingHours model)
        {
            if (ModelState.IsValid)
            {
                _context.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // Редакция
        public async Task<IActionResult> Edit(int id)
        {
            var workingHour = await _context.WorkingHours.FindAsync(id);
            if (workingHour == null)
                return NotFound();

            return View(workingHour);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, WorkingHours model)
        {
            if (id != model.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // Потвърждение за изтриване
        public async Task<IActionResult> Delete(int id)
        {
            var workingHour = await _context.WorkingHours.FindAsync(id);
            if (workingHour == null)
                return NotFound();

            return View(workingHour);
        }

        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var workingHour = await _context.WorkingHours.FindAsync(id);
            if (workingHour != null)
            {
                _context.WorkingHours.Remove(workingHour);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}

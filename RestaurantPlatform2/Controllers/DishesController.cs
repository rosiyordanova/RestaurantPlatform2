using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RestaurantPlatform.Data;
using RestaurantPlatform.Models;
using RestaurantPlatform.Models.Enums;
using RestaurantPlatform2.Data;
using System.IO;
using System.Linq;

namespace RestaurantPlatform.Controllers
{
    public class DishesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public DishesController(ApplicationDbContext context, UserManager<IdentityUser> userManager, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index(string search, int? categoryId, decimal? maxPrice, string season)
        {
            var dishes = _context.Dishes
                .Include(d => d.Category)
                .Where(d => d.IsAvailable || User.IsInRole("Admin"))
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                dishes = dishes.Where(d => d.Name.Contains(search));

            if (categoryId.HasValue)
                dishes = dishes.Where(d => d.CategoryId == categoryId);

            if (maxPrice.HasValue)
                dishes = dishes.Where(d => d.Price <= maxPrice);

            if (!string.IsNullOrWhiteSpace(season) && Enum.TryParse(season, out Season parsedSeason))
                dishes = dishes.Where(d => d.Season == parsedSeason);

            ViewData["Categories"] = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name");
            return View(await dishes.ToListAsync());
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewData["Categories"] = _context.Categories.ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(Dish dish)
        {
            if (dish.ImageFile != null)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var extension = Path.GetExtension(dish.ImageFile.FileName).ToLower();

                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError("ImageFile", "Позволени формати: .jpg, .jpeg, .png, .gif");
                }

                if (dish.ImageFile.Length > 2 * 1024 * 1024)
                {
                    ModelState.AddModelError("ImageFile", "Максимален размер: 2MB");
                }
            }

            if (ModelState.IsValid)
            {
                dish.CreatedAt = DateTime.Now;

                if (dish.ImageFile != null)
                {
                    string fileName = Path.GetFileNameWithoutExtension(dish.ImageFile.FileName);
                    string extension = Path.GetExtension(dish.ImageFile.FileName);
                    fileName = $"{fileName}_{Guid.NewGuid()}{extension}";
                    string path = Path.Combine(_webHostEnvironment.WebRootPath, "images", fileName);

                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await dish.ImageFile.CopyToAsync(stream);
                    }

                    dish.ImageUrl = "/images/" + fileName;
                }

                _context.Add(dish);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["Categories"] = _context.Categories.ToList();
            return View(dish);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var dish = await _context.Dishes.FindAsync(id);
            if (dish == null) return NotFound();

            ViewData["Categories"] = _context.Categories.ToList();
            return View(dish);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, Dish dish)
        {
            if (id != dish.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                ViewData["Categories"] = _context.Categories.ToList();
                return View(dish);
            }

            try
            {
                var existingDish = await _context.Dishes.FindAsync(id);
                if (existingDish == null) return NotFound();

                existingDish.Name = dish.Name;
                existingDish.Description = dish.Description;
                existingDish.Price = dish.Price;
                existingDish.CategoryId = dish.CategoryId;
                existingDish.Season = dish.Season;
                existingDish.IsAvailable = dish.IsAvailable;
                existingDish.IsNew = dish.IsNew;

                if (dish.ImageFile != null)
                {
                    string fileName = Path.GetFileNameWithoutExtension(dish.ImageFile.FileName);
                    string extension = Path.GetExtension(dish.ImageFile.FileName);
                    fileName = $"{fileName}_{Guid.NewGuid()}{extension}";
                    string path = Path.Combine(_webHostEnvironment.WebRootPath, "images", fileName);

                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await dish.ImageFile.CopyToAsync(stream);
                    }

                    existingDish.ImageUrl = "/images/" + fileName;
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = "Редакцията е успешна.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Грешка при редактиране.");
            }

            ViewData["Categories"] = _context.Categories.ToList();
            return View(dish);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var dish = await _context.Dishes
                .Include(d => d.Category)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (dish == null) return NotFound();

            return View(dish);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var dish = await _context.Dishes.FindAsync(id);
            if (dish != null)
            {
                _context.Dishes.Remove(dish);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var dish = await _context.Dishes
                .Include(d => d.Category)
                .Include(d => d.Reviews)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (dish == null) return NotFound();

            var reviews = await _context.Reviews
                .Where(r => r.DishId == id && r.IsApproved)
                .Include(r => r.User)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            ViewData["ApprovedReviews"] = reviews;

            return View(dish);
        }
    }
}

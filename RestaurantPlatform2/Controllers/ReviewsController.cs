using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantPlatform.Data;
using RestaurantPlatform.Models;
using RestaurantPlatform2.Data;
using RestaurantPlatform.ViewModels;

namespace RestaurantPlatform.Controllers
{
    [Authorize]
    public class ReviewsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ReviewsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Reviews/Create?dishId=5
        public async Task<IActionResult> Create(int dishId)
        {
            var user = await _userManager.GetUserAsync(User);

            var existingReview = await _context.Reviews
                .FirstOrDefaultAsync(r => r.DishId == dishId && r.UserId == user.Id);

            if (existingReview != null)
            {
                return RedirectToAction("Edit", new { id = existingReview.Id });
            }

            var dish = await _context.Dishes.FindAsync(dishId);
            if (dish == null) return NotFound();

            var model = new ReviewViewModel
            {
                DishId = dishId,
                DishName = dish.Name
            };

            return View(model);
        }

        // POST: Reviews/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ReviewViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // По желание можеш да заредиш dishName пак при грешка
                var dish = await _context.Dishes.FindAsync(model.DishId);
                model.DishName = dish?.Name;
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);

            var review = new Review
            {
                DishId = model.DishId,
                UserId = user.Id,
                Rating = model.Rating,
                Comment = model.Comment,
                CreatedAt = DateTime.Now,
                IsApproved = false
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Вашата рецензия беше изпратена за одобрение.";
            return RedirectToAction("Details", "Dishes", new { id = model.DishId });
        }

        // GET: Reviews/Edit
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var review = await _context.Reviews
                .Include(r => r.Dish)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (review == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (review.UserId != user.Id && !User.IsInRole("Admin"))
                return Forbid();

            var model = new ReviewViewModel
            {
                DishId = review.DishId,
                Rating = review.Rating,
                Comment = review.Comment,
                DishName = review.Dish?.Name
            };

            ViewData["ReviewId"] = review.Id;
            return View(model);
        }

        // POST: Reviews/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ReviewViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var dish = await _context.Dishes.FindAsync(model.DishId);
                model.DishName = dish?.Name;
                ViewData["ReviewId"] = id;
                return View(model);
            }

            var review = await _context.Reviews.FindAsync(id);
            if (review == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (review.UserId != user.Id && !User.IsInRole("Admin"))
                return Forbid();

            review.Rating = model.Rating;
            review.Comment = model.Comment;
            review.CreatedAt = DateTime.Now;
            review.IsApproved = false;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Рецензията беше редактирана и изпратена за одобрение.";
            return RedirectToAction("Details", "Dishes", new { id = review.DishId }); // <- правилно!
        }


        // GET: Reviews/DishReviews/5
        [AllowAnonymous]
        public async Task<IActionResult> DishReviews(int dishId)
        {
            var reviews = await _context.Reviews
                .Include(r => r.User)
                .Where(r => r.DishId == dishId && r.IsApproved)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            ViewData["DishId"] = dishId;
            return View(reviews);
        }

        // GET: Reviews/Pending (Admin only)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Pending()
        {
            var reviews = await _context.Reviews
                .Include(r => r.Dish)
                .Include(r => r.User)
                .Where(r => !r.IsApproved)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return View(reviews);
        }

        // POST: Reviews/Approve
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null) return NotFound();

            review.IsApproved = true;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Рецензията беше одобрена.";
            return RedirectToAction(nameof(Pending));
        }

        // POST: Reviews/Delete
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null) return NotFound();

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Рецензията беше изтрита.";
            return RedirectToAction(nameof(Pending));
        }
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> All(string userEmail, string dishName, string status)
        {
            var reviews = _context.Reviews
                .Include(r => r.Dish)
                .Include(r => r.User)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(userEmail))
            {
                reviews = reviews.Where(r => r.User.Email.Contains(userEmail));
            }

            if (!string.IsNullOrWhiteSpace(dishName))
            {
                reviews = reviews.Where(r => r.Dish.Name.Contains(dishName));
            }

            if (status == "approved")
            {
                reviews = reviews.Where(r => r.IsApproved);
            }
            else if (status == "pending")
            {
                reviews = reviews.Where(r => !r.IsApproved);
            }

            var list = await reviews.OrderByDescending(r => r.CreatedAt).ToListAsync();
            return View("Pending", list);
        }

    }
}

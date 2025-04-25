using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantPlatform.Data;
using RestaurantPlatform.Models;
using RestaurantPlatform.ViewModels;
using RestaurantPlatform2.Data;

namespace RestaurantPlatform.Controllers
{
    [Authorize]
    public class ReservationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ReservationsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Reservations/Create
        public IActionResult Create()
        {
            return View(new ReservationViewModel());
        }

        // POST: Reservations/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ReservationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                ModelState.AddModelError("", "Потребителят не е намерен.");
                return View(model);
            }

            var workingHours = await _context.WorkingHours
                .FirstOrDefaultAsync(w => w.Day == model.Date.DayOfWeek);

            if (workingHours == null || model.Time < workingHours.OpenTime || model.Time > workingHours.CloseTime)
            {
                ModelState.AddModelError("", "Резервацията е извън работното време.");
                TempData["Error"] = "Резервацията е извън работното време.";
                return View(model);
            }

            var reservation = new Reservation
            {
                UserId = user.Id,
                Date = model.Date,
                Time = model.Time,
                NumberOfGuests = model.NumberOfGuests.Value, // safe, защото ModelState.IsValid == true
                Note = model.Note,
                CreatedAt = DateTime.Now,
                Status = ReservationStatus.Изчакваща,
                IsNew = true
            };

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Резервацията беше създадена успешно!";
            return RedirectToAction(nameof(MyReservations));
        }

        // GET: Reservations/Edit
        public async Task<IActionResult> Edit(int id)
        {
            var reservation = await _context.Reservations
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reservation == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (reservation.UserId != user.Id && !User.IsInRole("Admin"))
                return Forbid();

            var model = new ReservationViewModel
            {
                Date = reservation.Date,
                Time = reservation.Time,
                NumberOfGuests = reservation.NumberOfGuests,
                Note = reservation.Note
            };

            ViewData["ReservationId"] = reservation.Id;
            return View(model);
        }

        // POST: Reservations/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ReservationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewData["ReservationId"] = id;
                return View(model);
            }

            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (reservation.UserId != user.Id && !User.IsInRole("Admin"))
                return Forbid();

            var workingHours = await _context.WorkingHours
                .FirstOrDefaultAsync(w => w.Day == model.Date.DayOfWeek);

            if (workingHours == null || model.Time < workingHours.OpenTime || model.Time > workingHours.CloseTime)
            {
                ModelState.AddModelError("", "Резервацията е извън работното време.");
                ViewData["ReservationId"] = id;
                return View(model);
            }

            reservation.Date = model.Date;
            reservation.Time = model.Time;
            reservation.NumberOfGuests = model.NumberOfGuests.Value;
            reservation.Note = model.Note;
            reservation.IsNew = true;
            reservation.Status = ReservationStatus.Изчакваща;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Резервацията беше редактирана успешно!";
            return RedirectToAction(nameof(MyReservations));
        }

        // GET: Reservations/Delete
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var reservation = await _context.Reservations
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reservation == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (reservation.UserId != user.Id && !User.IsInRole("Admin"))
                return Forbid();

            return View(reservation);
        }

        // POST: Reservations/DeleteConfirmed
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (reservation.UserId != user.Id && !User.IsInRole("Admin"))
                return Forbid();

            _context.Reservations.Remove(reservation);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Резервацията беше изтрита успешно!";
            return RedirectToAction(nameof(MyReservations));
        }

        // GET: Reservations/MyReservations
        public async Task<IActionResult> MyReservations()
        {
            var user = await _userManager.GetUserAsync(User);

            var reservations = await _context.Reservations
                .Where(r => r.UserId == user.Id)
                .OrderByDescending(r => r.Date)
                .ToListAsync();

            return View(reservations);
        }

        // GET: Reservations/AllReservations (admin only)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AllReservations()
        {
            var reservations = await _context.Reservations
                .Include(r => r.User)
                .OrderByDescending(r => r.Date)
                .ToListAsync();

            return View(reservations);
        }

        // GET + POST: ChangeStatus
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ChangeStatus(int id)
        {
            var reservation = await _context.Reservations
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reservation == null)
                return NotFound();

            ViewData["Statuses"] = Enum.GetValues(typeof(ReservationStatus));
            return View(reservation);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeStatus(int id, ReservationStatus status)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null) return NotFound();

            reservation.Status = status;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Статусът на резервацията беше успешно променен!";
            return RedirectToAction(nameof(AllReservations));
        }
    }
}

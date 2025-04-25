using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RestaurantPlatform2.Data;
using RestaurantPlatform.Models;
using RestaurantPlatform.Pdf;
using QuestPDF.Fluent;

namespace RestaurantPlatform.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private const string SessionKey = "CartItems";

        public OrdersController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private List<CartItem> GetCart()
        {
            var sessionData = HttpContext.Session.GetString(SessionKey);
            return sessionData != null
                ? JsonConvert.DeserializeObject<List<CartItem>>(sessionData)
                : new List<CartItem>();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create()
        {
            var cart = GetCart();

            if (!cart.Any())
            {
                TempData["Error"] = "Кошницата е празна. Не може да се създаде поръчка.";
                return RedirectToAction("Index", "Cart");
            }

            var user = await _userManager.GetUserAsync(User);

            var order = new Order
            {
                UserId = user.Id,
                OrderDate = DateTime.Now,
                Items = cart.Select(item => new OrderItem
                {
                    DishId = item.DishId,
                    Quantity = item.Quantity
                }).ToList()
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            HttpContext.Session.Remove(SessionKey);
            TempData["Success"] = "Поръчката беше изпратена успешно!";
            return RedirectToAction(nameof(MyOrders));
        }

        public async Task<IActionResult> MyOrders()
        {
            var user = await _userManager.GetUserAsync(User);
            var orders = await _context.Orders
                .Include(o => o.Items)
                .ThenInclude(i => i.Dish)
                .Where(o => o.UserId == user.Id)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AllOrders(string userEmail, DateTime? startDate, DateTime? endDate, OrderStatus? status)
        {
            var orders = _context.Orders
                .Include(o => o.User)
                .Include(o => o.Items).ThenInclude(i => i.Dish)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(userEmail))
            {
                orders = orders.Where(o => o.User.Email.Contains(userEmail));
            }

            if (startDate.HasValue)
                orders = orders.Where(o => o.OrderDate >= startDate);

            if (endDate.HasValue)
                orders = orders.Where(o => o.OrderDate <= endDate);

            if (status.HasValue)
                orders = orders.Where(o => o.Status == status);

            var list = await orders.OrderByDescending(o => o.OrderDate).ToListAsync();
            return View(list);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ChangeStatus(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Items)
                .ThenInclude(i => i.Dish)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            ViewData["Statuses"] = new SelectList(Enum.GetValues(typeof(OrderStatus)));
            return View(order);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ChangeStatus(int id, OrderStatus status)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            order.Status = status;

            _context.OrderStatusLogs.Add(new OrderStatusLog
            {
                OrderId = order.Id,
                Status = status,
                ChangedAt = DateTime.Now,
                ChangedByUserId = user.Id
            });

            await _context.SaveChangesAsync();
            return RedirectToAction("AllOrders");
        }

        [Authorize]
        public async Task<IActionResult> StatusHistory(int orderId)
        {
            var user = await _userManager.GetUserAsync(User);
            var order = await _context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                return NotFound();

            if (order.UserId != user.Id && !User.IsInRole("Admin"))
                return Forbid();

            var history = await _context.OrderStatusLogs
                .Where(l => l.OrderId == orderId)
                .Include(l => l.ChangedByUser)
                .OrderByDescending(l => l.ChangedAt)
                .ToListAsync();

            ViewData["OrderId"] = orderId;
            return View(history);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult WeeklyReport() => GenerateReport(DateTime.Now.AddDays(-7), DateTime.Now);

        [Authorize(Roles = "Admin")]
        public IActionResult ExportReport(DateTime? startDate, DateTime? endDate) => GenerateReport(startDate, endDate);

        private IActionResult GenerateReport(DateTime? start, DateTime? end)
        {
            var orders = _context.Orders
                .Include(o => o.Items).ThenInclude(i => i.Dish)
                .AsQueryable();

            if (start.HasValue) orders = orders.Where(o => o.OrderDate >= start);
            if (end.HasValue) orders = orders.Where(o => o.OrderDate <= end);

            var list = orders.ToList();

            using var package = new OfficeOpenXml.ExcelPackage();
            var sheet = package.Workbook.Worksheets.Add("Отчет");
            sheet.Cells[1, 1].Value = "Номер";
            sheet.Cells[1, 2].Value = "Дата";
            sheet.Cells[1, 3].Value = "Потребител";
            sheet.Cells[1, 4].Value = "Ястия";
            sheet.Cells[1, 5].Value = "Сума";

            int row = 2;
            foreach (var order in list)
            {
                var items = string.Join(", ", order.Items.Select(i => $"{i.Dish.Name} x{i.Quantity}"));
                var total = order.Items.Sum(i => i.Dish.Price * i.Quantity);

                sheet.Cells[row, 1].Value = order.Id;
                sheet.Cells[row, 2].Value = order.OrderDate.ToString("dd.MM.yyyy");
                sheet.Cells[row, 3].Value = order.UserId;
                sheet.Cells[row, 4].Value = items;
                sheet.Cells[row, 5].Value = total;
                row++;
            }

            sheet.Cells[1, 1, row - 1, 5].AutoFitColumns();
            var bytes = package.GetAsByteArray();

            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "report.xlsx");
        }

        [Authorize]
        public async Task<IActionResult> Receipt(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var order = await _context.Orders
                .Include(o => o.Items).ThenInclude(i => i.Dish)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null || (order.UserId != user.Id && !User.IsInRole("Admin")))
                return Forbid();

            return View(order);
        }

        [Authorize]
        public async Task<IActionResult> GenerateInvoicePdf(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var order = await _context.Orders
                .Include(o => o.Items).ThenInclude(i => i.Dish)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null || (order.UserId != user.Id && !User.IsInRole("Admin")))
                return Forbid();

            var document = new InvoiceDocument(order);
            var pdfBytes = document.GeneratePdf();

            return File(pdfBytes, "application/pdf", $"invoice_order_{order.Id}.pdf");
        }

        [Authorize]
        public async Task<IActionResult> Reorder(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            var order = await _context.Orders
                .Include(o => o.Items).ThenInclude(i => i.Dish)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == user.Id);

            if (order == null)
            {
                TempData["Error"] = "Поръчката не е намерена или не е ваша.";
                return RedirectToAction(nameof(MyOrders));
            }

            var cartItems = order.Items.Select(i => new CartItem
            {
                DishId = i.DishId,
                DishName = i.Dish.Name,
                Quantity = i.Quantity,
                Price = i.Dish.Price
            }).ToList();

            HttpContext.Session.SetString("CartItems", JsonConvert.SerializeObject(cartItems));
            TempData["Success"] = "Поръчката беше заредена отново в кошницата.";
            return RedirectToAction("Index", "Cart");
        }
    }
}

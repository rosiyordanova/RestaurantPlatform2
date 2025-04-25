using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RestaurantPlatform.Data;
using RestaurantPlatform.Models;
using RestaurantPlatform2.Data;

namespace RestaurantPlatform.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const string SessionKey = "CartItems";

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        private List<CartItem> GetCart()
        {
            var sessionData = HttpContext.Session.GetString(SessionKey);
            return sessionData != null ? JsonConvert.DeserializeObject<List<CartItem>>(sessionData) : new List<CartItem>();
        }

        private void SaveCart(List<CartItem> cart)
        {
            HttpContext.Session.SetString(SessionKey, JsonConvert.SerializeObject(cart));
        }

        public IActionResult Index()
        {
            var cart = GetCart();
            return View(cart);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int id)
        {
            var dish = await _context.Dishes.FindAsync(id);
            if (dish == null) return NotFound();

            var cart = GetCart();
            var existingItem = cart.FirstOrDefault(c => c.DishId == id);

            if (existingItem != null)
                existingItem.Quantity++;
            else
                cart.Add(new CartItem { DishId = id, DishName = dish.Name, Price = dish.Price, Quantity = 1 });

            SaveCart(cart);
            TempData["Success"] = "Ястието е добавено в кошницата.";
            return RedirectToAction("Index", "Dishes");
        }

        public IActionResult Remove(int id)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(c => c.DishId == id);
            if (item != null) cart.Remove(item);
            SaveCart(cart);
            return RedirectToAction("Index");
        }

        public IActionResult Clear()
        {
            SaveCart(new List<CartItem>());
            return RedirectToAction("Index");
        }
    }
}

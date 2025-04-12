using Microsoft.AspNetCore.Mvc;
using masterPiece.Models;
using Newtonsoft.Json;

namespace masterPiece.Controllers
{
    public class CartController : Controller
    {
        
        private readonly MyDbContext _context;

        public CartController(MyDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var cart = GetCartFromSession();
            return View(cart);
        }

        public IActionResult AddToCart(int id)
        {
            var product = _context.Products.FirstOrDefault(p => p.Id == id);
            if (product == null) return NotFound();

            var cart = GetCartFromSession();

            var existingItem = cart.FirstOrDefault(i => Convert.ToInt32(i["ProductId"]) == id);

            if (existingItem != null)
            {
                existingItem["Quantity"] = Convert.ToInt32(existingItem["Quantity"]) + 1;
            }
            else
            {
                cart.Add(new Dictionary<string, object>
                {
                    ["ProductId"] = product.Id,
                    ["Name"] = product.Name,
                    ["Price"] = product.Price,
                    ["Quantity"] = 1,
                    ["ImagePath"] = product.ImagePath ?? "default.jpg"
                });
            }

            SaveCartToSession(cart);
            return RedirectToAction("Index");
        }

        public IActionResult Remove(int id)
        {
            var cart = GetCartFromSession();
            var item = cart.FirstOrDefault(i => Convert.ToInt32(i["ProductId"]) == id);
            if (item != null)
            {
                cart.Remove(item);
                SaveCartToSession(cart);
            }

            return RedirectToAction("Index");
        }

        private List<Dictionary<string, object>> GetCartFromSession()
        {
            var session = HttpContext.Session.GetString("Cart");
            return session != null
                ? JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(session)
                : new List<Dictionary<string, object>>();
        }

        private void SaveCartToSession(List<Dictionary<string, object>> cart)
        {
            HttpContext.Session.SetString("Cart", JsonConvert.SerializeObject(cart));
        }
        [HttpPost]
        public IActionResult UpdateQuantity(int productId, int quantity)
        {
            var cart = GetCartFromSession();

            var item = cart.FirstOrDefault(i => Convert.ToInt32(i["ProductId"]) == productId);
            if (item != null)
            {
                item["Quantity"] = quantity;
                SaveCartToSession(cart);
            }

            return RedirectToAction("Index");
        }

        public IActionResult CheckLoginAndRedirect()
        {
            int? userId = HttpContext.Session.GetInt32("userId");

            if (userId == null)
            {
                TempData["NotRegistered"] = "You are not registered. Please sign up first.";
                return RedirectToAction("Register", "Users"); // غير مسجل نهائيًا
            }

            // افترضنا إنو فيه جدول Users
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);

            if (user == null)
            {
                return RedirectToAction("Login", "Users"); // عنده حساب بس مش داخل
            }

            // إذا كل شيء تمام
            return RedirectToAction("Checkout", "Cart");
        }
        public IActionResult Checkout()
        {
            return View();
        }


    }
}

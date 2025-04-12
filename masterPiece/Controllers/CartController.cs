using Microsoft.AspNetCore.Mvc;
using masterPiece.Models;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;

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
            int? userId = HttpContext.Session.GetInt32("userId");

            if (userId != null)
            {
                var cart = _context.Carts
                    .Include(c => c.CartDetails)
                    .ThenInclude(cd => cd.Product)
                    .FirstOrDefault(c => c.UserId == userId);

                if (cart != null)
                {
                    var items = cart.CartDetails.Select(cd => new Dictionary<string, object>
                    {
                        ["ProductId"] = cd.ProductId,
                        ["Name"] = cd.Product?.Name ?? "",
                        ["Price"] = cd.Product?.Price ?? 0,
                        ["Quantity"] = cd.Quantity,
                        ["ImagePath"] = cd.Product?.ImagePath ?? "default.jpg"
                    }).ToList();

                    return View(items);
                }

                return View(new List<Dictionary<string, object>>());
            }

            var sessionCart = HttpContext.Session.GetString("Cart");
            var sessionItems = sessionCart != null
                ? JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(sessionCart)
                : new List<Dictionary<string, object>>();

            return View(sessionItems);
        }

        public IActionResult AddToCart(int id)
        {
            var product = _context.Products.FirstOrDefault(p => p.Id == id);
            if (product == null) return NotFound();

            int? userId = HttpContext.Session.GetInt32("userId");

            if (userId == null)
            {
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
            }
            else
            {
                var cart = _context.Carts.FirstOrDefault(c => c.UserId == userId);
                if (cart == null)
                {
                    cart = new Cart { UserId = userId.Value };
                    _context.Carts.Add(cart);
                    _context.SaveChanges();
                }

                var cartItem = _context.CartDetails.FirstOrDefault(cd => cd.CartId == cart.Id && cd.ProductId == id);

                if (cartItem != null)
                {
                    cartItem.Quantity += 1;
                }
                else
                {
                    _context.CartDetails.Add(new CartDetail
                    {
                        CartId = cart.Id,
                        ProductId = product.Id,
                        Quantity = 1
                    });
                }

                _context.SaveChanges();
            }

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
                return RedirectToAction("Register", "Users");
            }

            var user = _context.Users.FirstOrDefault(u => u.Id == userId);

            if (user == null)
            {
                return RedirectToAction("Login", "Users");
            }

            return RedirectToAction("Checkout", "Cart");
        }

        public IActionResult Checkout()
        {
            int? userId = HttpContext.Session.GetInt32("userId");

            if (userId != null)
            {
                var cart = _context.Carts.FirstOrDefault(c => c.UserId == userId);
                if (cart != null)
                {
                    var items = _context.CartDetails
                        .Where(cd => cd.CartId == cart.Id)
                        .Select(cd => new
                        {
                            cd.ProductId,
                            ProductName = cd.Product.Name,
                            Price = cd.Product.Price,
                            Quantity = cd.Quantity,
                            ImagePath = cd.Product.ImagePath ?? "default.jpg"
                        })
                        .ToList()
                        .Select(item => new Dictionary<string, object>
                        {
                            ["ProductId"] = item.ProductId,
                            ["Name"] = item.ProductName,
                            ["Price"] = item.Price,
                            ["Quantity"] = item.Quantity,
                            ["ImagePath"] = item.ImagePath
                        })
                        .ToList();

                    return View(items);
                }

                return View(new List<Dictionary<string, object>>());
            }
            else
            {
                var sessionCart = HttpContext.Session.GetString("Cart");
                var sessionItems = sessionCart != null
                    ? JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(sessionCart)
                    : new List<Dictionary<string, object>>();

                return View(sessionItems);
            }
        }

        public IActionResult OrderSuccess()
        {
            return View();
        }

        [HttpPost]
        public IActionResult PlaceOrder(string FullName, string Address, string PhoneNumber)
        {
            int? userId = HttpContext.Session.GetInt32("userId");

            if (userId == null)
            {
                return RedirectToAction("Login", "Users");
            }

            var cart = _context.Carts.FirstOrDefault(c => c.UserId == userId);
            if (cart == null)
            {
                TempData["Error"] = "Your cart is empty.";
                return RedirectToAction("Checkout");
            }

            var cartItems = _context.CartDetails
                .Where(cd => cd.CartId == cart.Id)
                .ToList();

            var order = new Order
            {
                UserId = userId.Value,
                OrderDate = DateTime.Now,
                TotalAmount = cartItems.Sum(i => i.Quantity * (i.Product?.Price ?? 0)),
                Status = "Pending",
                DeliveryAddress = Address
            };

            _context.Orders.Add(order);
            _context.SaveChanges();

            _context.CartDetails.RemoveRange(cartItems);
            _context.SaveChanges();

            return RedirectToAction("OrderSuccess");
        }
    }
}
using masterPiece.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace masterPiece.Controllers
{
    public class UsersController : Controller
    {
        private readonly MyDbContext _context;

        public UsersController(MyDbContext context)
        {
            _context = context;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email && u.PasswordHash == password && u.IsActive == true);

            if (user != null)
            {
                HttpContext.Session.SetInt32("userId", user.Id);
                HttpContext.Session.SetString("username", user.Username);
                HttpContext.Session.SetString("userType", user.UserType);

                // دمج السلة من السيشن إلى قاعدة البيانات
                var sessionCart = HttpContext.Session.GetString("Cart");
                if (!string.IsNullOrEmpty(sessionCart))
                {
                    var cartItems = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(sessionCart);

                    var cart = _context.Carts.FirstOrDefault(c => c.UserId == user.Id);
                    if (cart == null)
                    {
                        cart = new Cart
                        {
                            UserId = user.Id
                        };
                        _context.Carts.Add(cart);
                        _context.SaveChanges(); // لحفظ ID السلة الجديدة
                    }

                    foreach (var item in cartItems)
                    {
                        int productId = Convert.ToInt32(item["ProductId"]);
                        int quantity = Convert.ToInt32(item["Quantity"]);

                        var existingDetail = _context.CartDetails.FirstOrDefault(cd => cd.CartId == cart.Id && cd.ProductId == productId);
                        if (existingDetail != null)
                        {
                            existingDetail.Quantity += quantity;
                        }
                        else
                        {
                            _context.CartDetails.Add(new CartDetail
                            {
                                CartId = cart.Id,
                                ProductId = productId,
                                Quantity = quantity
                            });
                        }
                    }

                    _context.SaveChanges();
                    HttpContext.Session.Remove("Cart"); // حذف السلة من الجلسة
                }

                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Invalid email or password.";
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(string username, string email, string password, string phoneNumber)
        {
            if (_context.Users.Any(u => u.Email == email))
            {
                ViewBag.Error = "This email is already registered.";
                return View();
            }

            var user = new User
            {
                Username = username,
                Email = email,
                PasswordHash = password,
                PhoneNumber = phoneNumber,
                UserType = "Customer",
                IsActive = true
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            HttpContext.Session.SetInt32("userId", user.Id);
            HttpContext.Session.SetString("username", user.Username);
            HttpContext.Session.SetString("userType", user.UserType);

            return RedirectToAction("Index", "Home");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}

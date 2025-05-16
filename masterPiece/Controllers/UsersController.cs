using masterPiece.Models;
using masterPiece.ViewModels;
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

                // دمج السلة من السيشن إلى قاعدة البيانات (نفس الكود اللي عندك تماماً)
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
                        _context.SaveChanges();
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
                    HttpContext.Session.Remove("Cart");
                }

                // 🔁 التوجيه حسب المستخدم
                if (user.Email == "admin@gmail.com" && user.PasswordHash == "12345678")
                {
                    return RedirectToAction("Index", "AdminDashboard");
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
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
        public IActionResult Profile()
        {
            int? userId = HttpContext.Session.GetInt32("userId");
            if (userId == null)
                return RedirectToAction("Login");

            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            return View(user);
        }
        public IActionResult MyOrders()
        {
            int? userId = HttpContext.Session.GetInt32("userId");
            if (userId == null)
                return RedirectToAction("Login");

            var orders = _context.Orders
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            return View(orders);
        }
        public IActionResult Farmers()
        {
            var farmers = _context.Users
                .Where(u => u.UserType.ToLower() == "farmer")
                .ToList();

            return View(farmers);
        }
        public IActionResult EditProfile()
        {
            int? userId = HttpContext.Session.GetInt32("userId");
            if (userId == null)
                return RedirectToAction("Login");

            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            return View(user);
        }

        [HttpPost]
        public IActionResult EditProfile(User model)
        {
            int? userId = HttpContext.Session.GetInt32("userId");
            if (userId == null)
                return RedirectToAction("Login");

            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (user != null)
            {
                user.Username = model.Username;
                user.Email = model.Email;
                user.PhoneNumber = model.PhoneNumber;

                _context.SaveChanges();
                TempData["Success"] = "Profile updated successfully!";
            }

            return RedirectToAction("Profile");
        }

        public IActionResult ChangePassword()
        {
            int? userId = HttpContext.Session.GetInt32("userId");
            if (userId == null)
                return RedirectToAction("Login");

            return View(); // View بدون موديل
        }

        [HttpPost]
        public IActionResult ChangePassword(string CurrentPassword, string NewPassword, string ConfirmPassword)
        {
            int? userId = HttpContext.Session.GetInt32("userId");
            if (userId == null)
                return RedirectToAction("Login");

            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
                return RedirectToAction("Login");

            if (user.PasswordHash != CurrentPassword)
            {
                TempData["PasswordError"] = "Current password is incorrect.";
                return RedirectToAction("ChangePassword");
            }

            if (NewPassword != ConfirmPassword)
            {
                TempData["PasswordError"] = "New passwords do not match.";
                return RedirectToAction("ChangePassword");
            }

            user.PasswordHash = NewPassword;
            _context.SaveChanges();

            TempData["PasswordSuccess"] = "Password updated successfully!";
            return RedirectToAction("EditProfile");
        }
        //public IActionResult AllUsers()
        //{
        //    //if (HttpContext.Session.GetString("userType") != "Admin")
        //    //    return RedirectToAction("Login");

        //    var users = _context.Users.ToList();
        //    return View(users);
        //}
        public IActionResult AllUsers(string search = "", int page = 1)
        {
            int pageSize = 5;

            var query = _context.Users.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(u =>
                    u.Username.Contains(search) ||
                    u.Email.Contains(search) ||
                    u.UserType.Contains(search)
                );
            }

            int totalUsers = query.Count();
            int totalPages = (int)Math.Ceiling(totalUsers / (double)pageSize);

            var users = query
                .OrderBy(u => u.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var vm = new UserListViewModel
            {
                Users = users,
                CurrentPage = page,
                TotalPages = totalPages,
                SearchQuery = search
            };

            return View(vm);
        }


    }
}

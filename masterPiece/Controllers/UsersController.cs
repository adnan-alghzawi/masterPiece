﻿using masterPiece.Models;
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

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            var user = _context.Users.FirstOrDefault(u =>
                u.Email == email &&
                u.PasswordHash == password &&
                u.IsActive == true
            );

            if (user != null)
            {
                // ✅ تخزين بيانات الجلسة
                HttpContext.Session.SetInt32("UserID", user.ID);
                HttpContext.Session.SetString("UserType", user.UserType.ToLowerInvariant());

                // ✅ دمج سلة المشتريات إن وُجدت
                var sessionCart = HttpContext.Session.GetString("Cart");
                if (!string.IsNullOrEmpty(sessionCart))
                {
                    var cartItems = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(sessionCart);
                    var cart = _context.Carts.FirstOrDefault(c => c.UserID == user.ID);

                    if (cart == null)
                    {
                        cart = new Cart { UserID = user.ID };
                        _context.Carts.Add(cart);
                        _context.SaveChanges();
                    }

                    foreach (var item in cartItems)
                    {
                        int productId = Convert.ToInt32(item["ProductId"]);
                        int quantity = Convert.ToInt32(item["Quantity"]);

                        var existingDetail = _context.CartDetails
                            .FirstOrDefault(cd => cd.CartID == cart.ID && cd.ProductID == productId);

                        if (existingDetail != null)
                            existingDetail.Quantity += quantity;
                        else
                            _context.CartDetails.Add(new CartDetail
                            {
                                CartID = cart.ID,
                                ProductID = productId,
                                Quantity = quantity
                            });
                    }

                    _context.SaveChanges();
                    HttpContext.Session.Remove("Cart");
                }

                // ✅ التوجيه حسب نوع المستخدم
                var type = user.UserType.ToLowerInvariant();

                if (user.Email == "admin@gmail.com" && user.PasswordHash == "12345678")
                    return RedirectToAction("Index", "AdminDashboard");

                if (type == "farmer")
                    return RedirectToAction("Dashboard", "Farmer");

                if (type == "customer")
                    return RedirectToAction("Index", "Home");

                // fallback (في حالة نوع غير معروف)
                return RedirectToAction("Login");
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
                UserType = "customer",
                IsActive = true
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            HttpContext.Session.SetInt32("UserID", user.ID);
            HttpContext.Session.SetString("UserType", "customer");

            return RedirectToAction("Index", "Home");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        public IActionResult Profile()
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            if (userId == null) return RedirectToAction("Login");

            var user = _context.Users.FirstOrDefault(u => u.ID == userId);
            return View(user);
        }

        public IActionResult MyOrders()
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            if (userId == null) return RedirectToAction("Login");

            var orders = _context.Orders
                .Where(o => o.UserID == userId)
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

        public IActionResult FarmerProfile(int id)
        {
            var farmer = _context.Users.FirstOrDefault(u => u.ID == id && u.UserType.ToLower() == "farmer");
            if (farmer == null) return NotFound();

            var works = _context.FarmerWorks
                .Where(w => w.UserId == id)
                .OrderByDescending(w => w.CreatedAt)
                .ToList();

            var vm = new FarmerProfileViewModel
            {
                Farmer = farmer,
                Works = works
            };

            return View(vm);
        }

        public IActionResult EditProfile()
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            if (userId == null) return RedirectToAction("Login");

            var user = _context.Users.FirstOrDefault(u => u.ID == userId);
            return View(user);
        }

        [HttpPost]
        public IActionResult EditProfile(User model)
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            if (userId == null) return RedirectToAction("Login");

            var user = _context.Users.FirstOrDefault(u => u.ID == userId);
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
            var userId = HttpContext.Session.GetInt32("UserID");
            if (userId == null) return RedirectToAction("Login");

            return View();
        }

        [HttpPost]
        public IActionResult ChangePassword(string CurrentPassword, string NewPassword, string ConfirmPassword)
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            if (userId == null) return RedirectToAction("Login");

            var user = _context.Users.FirstOrDefault(u => u.ID == userId);
            if (user == null) return RedirectToAction("Login");

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
                .OrderBy(u => u.ID)
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
        [HttpPost]
        public IActionResult ReserveFarmer(int farmerId, DateTime startDate, DateTime endDate)
        {
            HttpContext.Session.SetString($"FarmerReserved_{farmerId}", $"{startDate:yyyy-MM-dd}|{endDate:yyyy-MM-dd}");
            TempData["Success"] = "✅ Reservation completed successfully!";
            return RedirectToAction("Farmers");
        }


    }
}

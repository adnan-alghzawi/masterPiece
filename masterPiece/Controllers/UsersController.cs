using masterPiece.Models;
using Microsoft.AspNetCore.Mvc;

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
                UserType = "User",
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

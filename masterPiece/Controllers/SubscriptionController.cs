using Microsoft.AspNetCore.Mvc;
using masterPiece.Models;
using System;

namespace masterPiece.Controllers
{
    public class SubscriptionController : Controller
    {
        private readonly MyDbContext _context;

        public SubscriptionController(MyDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Subscribe(string planName)
        {
            // ✅ تحقق من تسجيل الدخول
            var userId = HttpContext.Session.GetInt32("UserID");
            if (userId == null)
                return RedirectToAction("Login", "Users");

            // ✅ تحقق من وجود اشتراك فعّال
            var activeSubscription = _context.Subscriptions
                .FirstOrDefault(s => s.UserId == userId && s.IsActive == true && s.EndDate > DateTime.Now);

            if (activeSubscription != null)
            {
                TempData["Error"] = "You already have an active subscription.";
                return RedirectToAction("Index");
            }

            // ✅ إنشاء اشتراك جديد
            var start = DateTime.Now;
            var end = planName switch
            {
                "Monthly" => start.AddDays(30),
                "Quarterly" => start.AddDays(90),
                "Yearly" => start.AddDays(365),
                _ => start.AddDays(30)
            };

            var subscription = new Subscription
            {
                UserId = userId.Value,
                PlanName = planName,
                StartDate = start,
                EndDate = end,
                IsActive = true
            };

            _context.Subscriptions.Add(subscription);
            _context.SaveChanges();

            // ✅ هنا تقدر تضيف أي ميزات إضافية للمستخدم
            TempData["Success"] = "Subscription successful! Your benefits have been activated.";
            return RedirectToAction("Index");
        }
    }
}

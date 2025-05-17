using Microsoft.AspNetCore.Mvc;
using masterPiece.Models;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;

namespace masterPiece.Components
{
    public class CartItemCountViewComponent : ViewComponent
    {
        private readonly MyDbContext _context;

        public CartItemCountViewComponent(MyDbContext context)
        {
            _context = context;
        }

        public IViewComponentResult Invoke()
        {
            int itemCount = 0;

            // ✅ تأكدنا من اسم الجلسة الصحيح (UserID)
            int? userId = HttpContext.Session.GetInt32("UserID");

            if (userId != null)
            {
                // ✅ مستخدم مسجل دخول → من قاعدة البيانات
                var cart = _context.Carts
                    .Include(c => c.CartDetails)
                    .FirstOrDefault(c => c.UserID == userId);

                if (cart != null)
                {
                    itemCount = cart.CartDetails.Sum(cd => cd.Quantity);
                }
            }
            else
            {
                // ✅ مستخدم غير مسجل → نقرأ من Session
                var cartJson = HttpContext.Session.GetString("Cart");
                if (!string.IsNullOrEmpty(cartJson))
                {
                    var items = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(cartJson);
                    itemCount = items.Sum(item => Convert.ToInt32(item["Quantity"]));
                }
            }

            // ✅ العرض يتم من View باسم Default.cshtml
            return View("Default", itemCount);
        }
    }
}

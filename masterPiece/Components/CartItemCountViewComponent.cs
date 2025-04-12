using Microsoft.AspNetCore.Mvc;
using masterPiece.Models;

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

            int? userId = HttpContext.Session.GetInt32("userId");

            if (userId != null)
            {
                // مستخدم مسجّل دخول → نجيب السلة من الداتابيس
                var cart = _context.Carts.FirstOrDefault(c => c.UserId == userId);
                if (cart != null)
                {
                    itemCount = _context.CartDetails
                        .Where(cd => cd.CartId == cart.Id)
                        .Sum(cd => cd.Quantity);
                }
            }
            else
            {
                // مستخدم غير مسجّل دخول → نقرأ من السيشن
                var cartJson = HttpContext.Session.GetString("Cart");
                if (!string.IsNullOrEmpty(cartJson))
                {
                    var items = Newtonsoft.Json.JsonConvert
                        .DeserializeObject<List<Dictionary<string, object>>>(cartJson);
                    itemCount = items.Sum(item => Convert.ToInt32(item["Quantity"]));
                }
            }

            return View("_CartItemCount", itemCount);
        }


    }
}

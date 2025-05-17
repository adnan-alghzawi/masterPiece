using masterPiece.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace masterPiece.Controllers
{
    public class AdminOrdersController : Controller
    {
        private readonly MyDbContext _context;

        public AdminOrdersController(MyDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var orders = _context.Orders
                .Include(o => o.User)
                .Include(o => o.Payments)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            return View(orders);
        }

        [HttpPost]
        public IActionResult UpdateStatus(int id, string newStatus)
        {
            var order = _context.Orders.FirstOrDefault(o => o.ID == id);
            if (order != null)
            {
                order.Status = newStatus;
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }
    }
}

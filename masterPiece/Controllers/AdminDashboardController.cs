using masterPiece.Models;
using masterPiece.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace masterPiece.Controllers
{
    public class AdminDashboardController : Controller
    {
        private readonly MyDbContext _context;

        public AdminDashboardController(MyDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var viewModel = new AdminDashboardViewModel
            {
                TotalUsers = _context.Users.Count(),
                TotalProducts = _context.Products.Count(),
                TotalOrders = _context.Orders.Count(),
                TotalSales = _context.Payments.Sum(p => p.Amount),
                ActiveSubscriptions = _context.Subscriptions.Count(s => s.IsActive == true),
                LatestOrders = _context.Orders
                    .OrderByDescending(o => o.OrderDate)
                    .Include(o => o.User)
                    .Take(5)
                    .Select(o => new RecentOrder
                    {
                        OrderId = o.Id,
                        CustomerName = o.User.Username,
                        OrderDate = o.OrderDate,
                        TotalAmount = o.TotalAmount,
                        Status = o.Status
                    }).ToList()
            };

            return View(viewModel);
        }
    }
}

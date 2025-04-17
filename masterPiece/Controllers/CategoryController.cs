using masterPiece.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace masterPiece.Controllers
{
    public class CategoryController : Controller
    {
        private readonly MyDbContext _context;

        public CategoryController(MyDbContext context)
        {
            _context = context;
        }

        // عرض جميع الكاتيجوريز
        public IActionResult Index()
        {
            var categories = _context.Categories.ToList();
            return View(categories);
        }

        // عرض المنتجات المرتبطة بكاتيجوري معيّن
        public IActionResult CategoryDetails(int id)
        {
            var category = _context.Categories
                .Include(c => c.Products)
                .FirstOrDefault(c => c.Id == id);

            if (category == null)
                return NotFound();

            var products = category.Products
                .Where(p => p.IsActive == true)
                .ToList();

            ViewBag.CategoryName = category.Name;
            return View(products);
        }
    }
}

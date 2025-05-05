using masterPiece.Models;
using Microsoft.AspNetCore.Mvc;

namespace masterPiece.Controllers
{
    public class ProductsController : Controller
    {
        private readonly MyDbContext _context;
        public ProductsController(MyDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            var products = _context.Products.Where(p => p.IsActive == true).ToList();
            return View(products);
        }
        //public IActionResult ProductDetails(int id)
        //{
        //    var product = _context.Products.FirstOrDefault(p => p.Id == id);
        //    if (product == null)
        //    {
        //        return NotFound();
        //    }
        //    return View(product);
        //}
        public async Task<IActionResult> ProductDetails(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }


    }
}

using masterPiece.Models;
using masterPiece.ViewModels;
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
        public IActionResult Index1()
        {
            var categories = _context.Categories.Include(c => c.Products).ToList();
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
        // GET: Edit Category
        public IActionResult Edit(int id)
        {
            var category = _context.Categories.FirstOrDefault(c => c.Id == id);
            if (category == null)
                return NotFound();

            return View(category);
        }

        // POST: Edit Category
        [HttpPost]
        public IActionResult Edit(Category model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var category = _context.Categories.FirstOrDefault(c => c.Id == model.Id);
            if (category == null)
                return NotFound();

            category.Name = model.Name;
            category.Description = model.Description;

            _context.SaveChanges();

            return RedirectToAction("Index1");
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CategoryFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            string imageName = "default.jpg";
            if (model.Image != null)
            {
                var imagePath = Path.Combine("wwwroot/images", model.Image.FileName);
                using (var stream = new FileStream(imagePath, FileMode.Create))
                {
                    await model.Image.CopyToAsync(stream);
                }

                imageName = model.Image.FileName;
            }

            var category = new Category
            {
                Name = model.Name,
                Description = model.Description,
                ImagePath = imageName
            };

            _context.Categories.Add(category);
            _context.SaveChanges();

            return RedirectToAction("Index1");
        }

    }
}

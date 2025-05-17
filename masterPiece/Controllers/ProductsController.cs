using masterPiece.Models;
using masterPiece.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace masterPiece.Controllers
{
    public class ProductsController : Controller
    {
        private readonly MyDbContext _context;

        public ProductsController(MyDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(string status = "all")
        {
            var productsQuery = _context.Products
                .Select(p => new Product
                {
                    UserID = p.ID,
                    Name = p.Name,
                    Price = p.Price,
                    QuantityAvailable = p.QuantityAvailable,
                    ImagePath = p.ImagePath,
                    IsActive = p.IsActive,
                    Category = p.Category
                });

            if (status == "active")
                productsQuery = productsQuery.Where(p => p.IsActive == true);
            else if (status == "unavailable")
                productsQuery = productsQuery.Where(p => p.IsActive == false);

            ViewBag.CurrentStatus = status;
            return View(productsQuery.ToList());
        }


        public async Task<IActionResult> ProductDetails(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        public IActionResult Create()
        {
            var categories = _context.Categories
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                })
                .ToList();

            var vm = new ProductFormViewModel
            {
                Categories = categories
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Create(ProductFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Categories = _context.Categories
                    .Select(c => new SelectListItem
                    {
                        Value = c.Id.ToString(),
                        Text = c.Name
                    })
                    .ToList();

                return View(model);
            }

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

            var product = new Product
            {
                Name = model.Name,
                Description = model.Description,
                Price = model.Price,
                QuantityAvailable = model.QuantityAvailable,
                IsActive = model.IsActive,
                CategoryId = model.CategoryId,
                ImagePath = imageName

                // ⚠️ لا تقم بتحديد UserId هنا إذا ما كان مربوط فعليًا
            };

            _context.Products.Add(product);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult ChangeStatus(int id, bool newStatus)
        {
            var product = _context.Products.FirstOrDefault(p => p.ID == id);
            if (product == null)
                return NotFound();

            product.IsActive = newStatus;
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        public IActionResult Edit(int id)
        {
            var product = _context.Products.FirstOrDefault(p => p.ID == id);
            if (product == null) return NotFound();

            var vm = new ProductFormViewModel
            {
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                QuantityAvailable = product.QuantityAvailable,
                IsActive = product.IsActive ?? true,
                CategoryId = product.CategoryId,
                Categories = _context.Categories.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                }).ToList()
            };

            ViewBag.ProductId = id;
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, ProductFormViewModel model)
        {
            var product = _context.Products.FirstOrDefault(p => p.ID == id);
            if (product == null) return NotFound();

            if (!ModelState.IsValid)
            {
                model.Categories = _context.Categories.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                }).ToList();
                ViewBag.ProductId = id;
                return View(model);
            }

            product.Name = model.Name;
            product.Description = model.Description;
            product.Price = model.Price;
            product.QuantityAvailable = model.QuantityAvailable;
            product.IsActive = model.IsActive;
            product.CategoryId = model.CategoryId;

            if (model.Image != null)
            {
                var imagePath = Path.Combine("wwwroot/images", model.Image.FileName);
                using (var stream = new FileStream(imagePath, FileMode.Create))
                {
                    await model.Image.CopyToAsync(stream);
                }

                product.ImagePath = model.Image.FileName;
            }

            _context.SaveChanges();
            return RedirectToAction("Index");
        }

    }
}

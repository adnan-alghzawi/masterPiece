using Microsoft.AspNetCore.Mvc.Rendering;

namespace masterPiece.ViewModels
{
    public class ProductFormViewModel
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int QuantityAvailable { get; set; }
        public bool IsActive { get; set; }
        public IFormFile? Image { get; set; }
        public int? CategoryId { get; set; }

        public List<SelectListItem> Categories { get; set; } = new();
    }
}

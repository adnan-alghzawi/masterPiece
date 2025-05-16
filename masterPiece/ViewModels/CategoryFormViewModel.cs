using System.ComponentModel.DataAnnotations;

namespace masterPiece.ViewModels
{
    public class CategoryFormViewModel
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        public IFormFile? Image { get; set; } // صورة جديدة

        public string? ExistingImagePath { get; set; } // عند التعديل
    }
}

using masterPiece.Models;

namespace masterPiece.ViewModels
{
    public class FarmerProfileViewModel
    {
        public User Farmer { get; set; } = null!;
        public List<FarmerWork> Works { get; set; } = new();
    }
}

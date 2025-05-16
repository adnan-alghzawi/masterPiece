using masterPiece.Models;

namespace masterPiece.ViewModels
{
    public class UserListViewModel
    {
        public List<User> Users { get; set; } = new();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public string SearchQuery { get; set; } = "";
    }
}

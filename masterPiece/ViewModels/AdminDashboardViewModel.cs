namespace masterPiece.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalProducts { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalSales { get; set; }
        public int ActiveSubscriptions { get; set; }

        public List<RecentOrder> LatestOrders { get; set; } = new();
    }

    public class RecentOrder
    {
        public int OrderId { get; set; }
        public string CustomerName { get; set; }
        public DateTime? OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
    }
}

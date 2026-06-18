#nullable enable
namespace Warehouse.Models.ViewModels
{
    public class DashboardViewModel
    {
        // Counts
        public int TotalProducts { get; set; }
        public int TotalCategories { get; set; }
        public int TotalWarehouses { get; set; }
        public int TotalSuppliers { get; set; }
        public int TotalOrders { get; set; }
        public int TotalInventoryStocks { get; set; }

        // Alerts
        public int LowStockCount { get; set; }
        public int PendingOrdersCount { get; set; }
        public int ActiveShipmentsCount { get; set; }

        // Totals
        public decimal TotalInventoryValue { get; set; }

        // Recent data
        public List<RecentOrderInfo> RecentOrders { get; set; } = new();
        public List<LowStockInfo> LowStockProducts { get; set; } = new();
        public List<RecentTransactionInfo> RecentTransactions { get; set; } = new();
    }

    public class RecentOrderInfo
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class LowStockInfo
    {
        public string ProductName { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public string WarehouseName { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
    }

    public class RecentTransactionInfo
    {
        public string ProductName { get; set; } = string.Empty;
        public string WarehouseName { get; set; } = string.Empty;
        public string TransactionType { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

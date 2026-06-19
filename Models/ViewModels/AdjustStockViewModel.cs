#nullable enable
using System.ComponentModel.DataAnnotations;
using Warehouse.Models.Enums;

namespace Warehouse.Models.ViewModels
{
    /// <summary>
    /// View model for the dedicated AdjustStock workflow.
    /// The GET action populates the read-only product/stock detail section.
    /// The POST action reads only the adjustment input fields.
    /// </summary>
    public class AdjustStockViewModel
    {
        // ── Read-only product & stock details (populated by GET) ──────────────

        public int StockId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductSKU { get; set; } = string.Empty;
        public string? ProductBarcode { get; set; }
        public string? CategoryName { get; set; }
        public int WarehouseEntityId { get; set; }
        public string WarehouseName { get; set; } = string.Empty;
        public string? WarehouseLocation { get; set; }
        public int? BinId { get; set; }
        public string? BinCode { get; set; }
        public decimal CurrentQuantity { get; set; }
        public decimal ReservedQuantity { get; set; }
        public decimal AvailableQuantity => CurrentQuantity - ReservedQuantity;
        public DateTime? LastStockMovement { get; set; }
        public DateTime? LastUpdated { get; set; }

        // ── Adjustment inputs (populated by user on POST) ─────────────────────

        [Required(ErrorMessage = "Please select Increase or Decrease.")]
        public StockAdjustmentType AdjustmentType { get; set; }

        [Required(ErrorMessage = "Quantity is required.")]
        [Range(0.0001, double.MaxValue, ErrorMessage = "Quantity must be greater than zero.")]
        public decimal QuantityChange { get; set; }

        [Required(ErrorMessage = "Adjustment reason is required.")]
        [StringLength(500)]
        public string Reason { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Notes { get; set; }
    }
}

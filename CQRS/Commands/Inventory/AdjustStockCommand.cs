#nullable enable
using MediatR;
using Warehouse.Models.DTOs;
using Warehouse.Models.Enums;

namespace Warehouse.CQRS.Commands.Inventory
{
    /// <summary>
    /// Command for the dedicated stock adjustment workflow.
    /// Supports Increase / Decrease of stock with mandatory reason.
    /// Creates an InventoryTransaction of type Adjustment.
    /// </summary>
    public class AdjustStockCommand : IRequest<BaseResponse<bool>>
    {
        public int StockId { get; set; }
        public StockAdjustmentType AdjustmentType { get; set; }

        /// <summary>Positive quantity to add or remove.</summary>
        public decimal QuantityChange { get; set; }

        /// <summary>Required structured reason (e.g. "Damaged Goods", "Inventory Count").</summary>
        public string Reason { get; set; } = string.Empty;

        public string? Notes { get; set; }

        /// <summary>Identity user ID from ClaimTypes.NameIdentifier.</summary>
        public string? UserId { get; set; }
    }
}

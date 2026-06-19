#nullable enable
using MediatR;
using Microsoft.EntityFrameworkCore;
using Warehouse.CQRS.Commands.Inventory;
using Warehouse.Data;
using Warehouse.Models.DTOs;
using Warehouse.Models.Entities;
using Warehouse.Models.Enums;

namespace Warehouse.CQRS.Handlers.Inventory
{
    /// <summary>
    /// Handles the dedicated stock adjustment workflow.
    ///
    /// Business rules enforced:
    ///   • Cannot reduce stock below reserved quantity
    ///   • Cannot create negative inventory
    ///   • Reason is required (enforced at view model level + handler)
    ///
    /// Always creates an InventoryTransaction (Type = Adjustment) recording:
    ///   PreviousQuantity, NewQuantity, Difference (in Notes), Reason, UserId, Date
    /// </summary>
    public class AdjustStockHandler : IRequestHandler<AdjustStockCommand, BaseResponse<bool>>
    {
        private readonly ApplicationDbContext _context;

        public AdjustStockHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<BaseResponse<bool>> Handle(AdjustStockCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Reason))
                return Result.Fail<bool>("Adjustment reason is required.");

            if (request.QuantityChange <= 0)
                return Result.Fail<bool>("Quantity change must be greater than zero.");

            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var stock = await _context.InventoryStocks
                    .Include(s => s.Product)
                    .Include(s => s.WarehouseEntity)
                    .Include(s => s.Bin)
                    .FirstOrDefaultAsync(s => s.Id == request.StockId && !s.IsDeleted, cancellationToken);

                if (stock == null)
                    return Result.Fail<bool>("Inventory stock record not found.");

                decimal previousQty = stock.Quantity;
                decimal newQty;

                if (request.AdjustmentType == StockAdjustmentType.Increase)
                {
                    newQty = previousQty + request.QuantityChange;
                }
                else // Decrease
                {
                    // Validate: cannot go below reserved quantity
                    if (request.QuantityChange > (previousQty - stock.ReservedQuantity))
                    {
                        return Result.Fail<bool>(
                            $"Cannot reduce stock by {request.QuantityChange:N2}. " +
                            $"Current available quantity is {(previousQty - stock.ReservedQuantity):N2} " +
                            $"({stock.ReservedQuantity:N2} units reserved).");
                    }

                    // Validate: cannot go negative
                    newQty = previousQty - request.QuantityChange;
                    if (newQty < 0)
                        return Result.Fail<bool>("Stock quantity cannot be negative.");
                }

                // Apply the adjustment
                stock.Quantity = newQty;
                stock.UpdatedAt = DateTime.UtcNow;

                // Create full audit transaction record
                var inventoryTx = new InventoryTransaction
                {
                    ProductId       = stock.ProductId,
                    WarehouseEntityId = stock.WarehouseEntityId,
                    BinId           = stock.BinId,
                    TransactionType = InventoryTransactionType.Adjustment,
                    Quantity        = request.QuantityChange,
                    PreviousQuantity = previousQty,
                    NewQuantity     = newQty,
                    Reason          = request.Reason,
                    Notes           = $"[{request.AdjustmentType}] Qty changed by {request.QuantityChange:N2} " +
                                      $"(Prev: {previousQty:N2} → New: {newQty:N2}). {request.Notes}".TrimEnd('.', ' '),
                    CreatedByUserId = request.UserId
                };

                _context.InventoryTransactions.Add(inventoryTx);
                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return Result.Ok(true);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Result.Fail<bool>($"Adjustment failed: {ex.Message}");
            }
        }
    }
}

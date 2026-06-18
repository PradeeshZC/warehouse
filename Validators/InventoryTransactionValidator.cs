#nullable enable
using FluentValidation;
using Warehouse.Models.Entities;

namespace Warehouse.Validators
{
    public class InventoryTransactionValidator : AbstractValidator<InventoryTransaction>
    {
        public InventoryTransactionValidator()
        {
            RuleFor(x => x.ProductId).GreaterThan(0);
            RuleFor(x => x.WarehouseEntityId).GreaterThan(0);
            RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("Quantity must be > 0");
            RuleFor(x => x.NewQuantity).GreaterThanOrEqualTo(0);
            RuleFor(x => x.PreviousQuantity).GreaterThanOrEqualTo(0);
        }
    }
}

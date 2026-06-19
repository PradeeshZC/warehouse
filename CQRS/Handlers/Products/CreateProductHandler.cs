#nullable enable
using MediatR;
using Warehouse.CQRS.Commands.Products;
using Warehouse.Models.DTOs;
using Warehouse.Models.Entities;
using Warehouse.Models.Enums;
using Warehouse.Repositories.Interfaces;

namespace Warehouse.CQRS.Handlers.Products
{
    /// <summary>
    /// Creates a product, and optionally creates an initial InventoryStock + StockIn transaction
    /// when InitialQuantity > 0 and a WarehouseEntityId is provided.
    /// All operations run inside one UnitOfWork transaction for atomicity.
    /// </summary>
    public class CreateProductHandler : IRequestHandler<CreateProductCommand, BaseResponse<int>>
    {
        private readonly IUnitOfWorkAsync _unitOfWork;
        private readonly IRepository<Product> _repository;

        public CreateProductHandler(IUnitOfWorkAsync unitOfWork, IRepository<Product> repository)
        {
            _unitOfWork = unitOfWork;
            _repository = repository;
        }

        public async Task<BaseResponse<int>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // 1. Create the product
                var entity = new Product
                {
                    Name        = request.Name,
                    SKU         = request.SKU,
                    Barcode     = string.IsNullOrWhiteSpace(request.Barcode) ? null : request.Barcode.Trim(),
                    Description = request.Description,
                    UnitPrice   = request.UnitPrice,
                    CategoryId  = request.CategoryId
                };

                await _repository.AddAsync(entity);
                await _unitOfWork.CompleteAsync();  // flush so entity.Id is populated

                // 2. Optionally create initial InventoryStock + StockIn transaction
                bool hasInitialInventory =
                    request.InitialQuantity.HasValue &&
                    request.InitialQuantity.Value > 0 &&
                    request.WarehouseEntityId.HasValue &&
                    request.WarehouseEntityId.Value > 0;

                if (hasInitialInventory)
                {
                    var stockRepo = _unitOfWork.Repository<InventoryStock>();
                    var stock = new InventoryStock
                    {
                        ProductId         = entity.Id,
                        WarehouseEntityId = request.WarehouseEntityId!.Value,
                        BinId             = request.BinId.HasValue && request.BinId.Value > 0 ? request.BinId : null,
                        Quantity          = request.InitialQuantity!.Value,
                        ReservedQuantity  = 0
                    };
                    await stockRepo.AddAsync(stock);

                    var txRepo = _unitOfWork.Repository<InventoryTransaction>();
                    var inventoryTx = new InventoryTransaction
                    {
                        ProductId         = entity.Id,
                        WarehouseEntityId = request.WarehouseEntityId!.Value,
                        BinId             = stock.BinId,
                        TransactionType   = InventoryTransactionType.StockIn,
                        Quantity          = request.InitialQuantity!.Value,
                        PreviousQuantity  = 0,
                        NewQuantity       = request.InitialQuantity!.Value,
                        Reason            = "Initial stock on product creation",
                        Notes             = $"Product '{request.Name}' created with initial stock of {request.InitialQuantity.Value:N2} units."
                    };
                    await txRepo.AddAsync(inventoryTx);

                    await _unitOfWork.CompleteAsync();
                }

                await _unitOfWork.CommitAsync();
                return Result.Ok(entity.Id);
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }
    }
}

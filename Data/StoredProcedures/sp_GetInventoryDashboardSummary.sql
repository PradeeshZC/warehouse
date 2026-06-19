-- ============================================================
-- Stored Procedure: sp_GetInventoryDashboardSummary
-- Purpose: Aggregate dashboard summary stats for inventory.
--          Called via EF Core FromSqlRaw in a reporting service.
-- Usage:   EXEC sp_GetInventoryDashboardSummary
-- ============================================================
CREATE OR ALTER PROCEDURE sp_GetInventoryDashboardSummary
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        -- Total active products
        (SELECT COUNT(*) FROM Products WHERE IsDeleted = 0) AS TotalProducts,

        -- Total stock value = sum(Quantity * UnitPrice) across all active stock
        ISNULL((
            SELECT SUM(s.Quantity * p.UnitPrice)
            FROM InventoryStocks s
            INNER JOIN Products p ON p.Id = s.ProductId
            WHERE s.IsDeleted = 0 AND p.IsDeleted = 0
        ), 0) AS TotalStockValue,

        -- Low stock count (AvailableQty = Quantity - ReservedQuantity < 10 but > 0)
        (SELECT COUNT(*)
         FROM InventoryStocks
         WHERE IsDeleted = 0
           AND (Quantity - ReservedQuantity) > 0
           AND (Quantity - ReservedQuantity) < 10) AS LowStockCount,

        -- Out of stock count
        (SELECT COUNT(*)
         FROM InventoryStocks
         WHERE IsDeleted = 0
           AND (Quantity - ReservedQuantity) <= 0) AS OutOfStockCount,

        -- Total active warehouses
        (SELECT COUNT(*) FROM Warehouses WHERE IsDeleted = 0) AS WarehouseCount,

        -- Total active categories
        (SELECT COUNT(*) FROM Categories WHERE IsDeleted = 0) AS CategoryCount,

        -- Today's transaction count
        (SELECT COUNT(*) FROM InventoryTransactions WHERE IsDeleted = 0 AND CAST(CreatedAt AS DATE) = CAST(GETUTCDATE() AS DATE)) AS TodayTransactionCount;
END;
GO

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace warehouse.Migrations
{
    /// <inheritdoc />
    public partial class AddBarcodeReservedQtyAndReason : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IX_InventoryStocks_ProductId_WarehouseEntityId_BinId",
                table: "InventoryStocks",
                newName: "IX_InventoryStocks_Product_Warehouse_Bin");

            migrationBuilder.AddColumn<string>(
                name: "Barcode",
                table: "Products",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Reason",
                table: "InventoryTransactions",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ReservedQuantity",
                table: "InventoryStocks",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_Products_Barcode",
                table: "Products",
                column: "Barcode",
                unique: true,
                filter: "[Barcode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryStocks_ProductId",
                table: "InventoryStocks",
                column: "ProductId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Products_Barcode",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_InventoryStocks_ProductId",
                table: "InventoryStocks");

            migrationBuilder.DropColumn(
                name: "Barcode",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Reason",
                table: "InventoryTransactions");

            migrationBuilder.DropColumn(
                name: "ReservedQuantity",
                table: "InventoryStocks");

            migrationBuilder.RenameIndex(
                name: "IX_InventoryStocks_Product_Warehouse_Bin",
                table: "InventoryStocks",
                newName: "IX_InventoryStocks_ProductId_WarehouseEntityId_BinId");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrderFlow.Infrastructure.Persistence.Migrations;

public partial class ReplaceProductInventoryWithInventory : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.RenameTable(name: "ProductInventories", newName: "Inventories");
        migrationBuilder.RenameIndex(name: "IX_ProductInventories_ProductId", table: "Inventories", newName: "IX_Inventories_ProductId");
        migrationBuilder.RenameColumn(name: "AvailableQuantity", table: "Inventories", newName: "Quantity");
        migrationBuilder.AddColumn<int>(name: "ReservedQuantity", table: "Inventories", type: "int", nullable: false, defaultValue: 0);
        migrationBuilder.AddColumn<int>(name: "Version", table: "Inventories", type: "int", nullable: false, defaultValue: 0);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "ReservedQuantity", table: "Inventories");
        migrationBuilder.DropColumn(name: "Version", table: "Inventories");
        migrationBuilder.RenameColumn(name: "Quantity", table: "Inventories", newName: "AvailableQuantity");
        migrationBuilder.RenameIndex(name: "IX_Inventories_ProductId", table: "Inventories", newName: "IX_ProductInventories_ProductId");
        migrationBuilder.RenameTable(name: "Inventories", newName: "ProductInventories");
    }
}

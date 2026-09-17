using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrderFlow.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCatalogPerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PricingRules_ProductId_Tier",
                table: "PricingRules");

            migrationBuilder.CreateIndex(
                name: "IX_Products_IsActive_Name",
                table: "Products",
                columns: new[] { "IsActive", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_PricingRules_ProductId_Tier_ValidFromUtc",
                table: "PricingRules",
                columns: new[] { "ProductId", "Tier", "ValidFromUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Products_IsActive_Name",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_PricingRules_ProductId_Tier_ValidFromUtc",
                table: "PricingRules");

            migrationBuilder.CreateIndex(
                name: "IX_PricingRules_ProductId_Tier",
                table: "PricingRules",
                columns: new[] { "ProductId", "Tier" });
        }
    }
}

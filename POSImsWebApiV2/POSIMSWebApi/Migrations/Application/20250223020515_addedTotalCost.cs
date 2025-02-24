using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace POSIMSWebApi.Migrations.Application
{
    /// <inheritdoc />
    public partial class addedTotalCost : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ProductCostTotalAmount",
                table: "ProductCostDetails",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProductCostTotalAmount",
                table: "ProductCostDetails");
        }
    }
}

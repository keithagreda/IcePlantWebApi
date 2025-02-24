using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace POSIMSWebApi.Migrations.Application
{
    /// <inheritdoc />
    public partial class changesOnPCDStocksReciving : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductCostDetails_SalesHeaders_SalesHeaderId",
                table: "ProductCostDetails");

            migrationBuilder.RenameColumn(
                name: "SalesHeaderId",
                table: "ProductCostDetails",
                newName: "StocksReceivingId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductCostDetails_SalesHeaderId",
                table: "ProductCostDetails",
                newName: "IX_ProductCostDetails_StocksReceivingId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductCostDetails_StocksReceivings_StocksReceivingId",
                table: "ProductCostDetails",
                column: "StocksReceivingId",
                principalTable: "StocksReceivings",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductCostDetails_StocksReceivings_StocksReceivingId",
                table: "ProductCostDetails");

            migrationBuilder.RenameColumn(
                name: "StocksReceivingId",
                table: "ProductCostDetails",
                newName: "SalesHeaderId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductCostDetails_StocksReceivingId",
                table: "ProductCostDetails",
                newName: "IX_ProductCostDetails_SalesHeaderId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductCostDetails_SalesHeaders_SalesHeaderId",
                table: "ProductCostDetails",
                column: "SalesHeaderId",
                principalTable: "SalesHeaders",
                principalColumn: "Id");
        }
    }
}

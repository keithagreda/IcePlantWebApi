using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace POSIMSWebApi.Migrations.Application
{
    /// <inheritdoc />
    public partial class addedSalesHeaderToProductCostDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SalesHeaderId",
                table: "ProductCostDetails",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductCostDetails_SalesHeaderId",
                table: "ProductCostDetails",
                column: "SalesHeaderId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductCostDetails_SalesHeaders_SalesHeaderId",
                table: "ProductCostDetails",
                column: "SalesHeaderId",
                principalTable: "SalesHeaders",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductCostDetails_SalesHeaders_SalesHeaderId",
                table: "ProductCostDetails");

            migrationBuilder.DropIndex(
                name: "IX_ProductCostDetails_SalesHeaderId",
                table: "ProductCostDetails");

            migrationBuilder.DropColumn(
                name: "SalesHeaderId",
                table: "ProductCostDetails");
        }
    }
}

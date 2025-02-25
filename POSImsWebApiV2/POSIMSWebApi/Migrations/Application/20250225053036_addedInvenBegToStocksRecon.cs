using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace POSIMSWebApi.Migrations.Application
{
    /// <inheritdoc />
    public partial class addedInvenBegToStocksRecon : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "InventoryBeginningId",
                table: "StockReconciliations",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_StockReconciliations_InventoryBeginningId",
                table: "StockReconciliations",
                column: "InventoryBeginningId");

            migrationBuilder.AddForeignKey(
                name: "FK_StockReconciliations_InventoryBeginnings_InventoryBeginning~",
                table: "StockReconciliations",
                column: "InventoryBeginningId",
                principalTable: "InventoryBeginnings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StockReconciliations_InventoryBeginnings_InventoryBeginning~",
                table: "StockReconciliations");

            migrationBuilder.DropIndex(
                name: "IX_StockReconciliations_InventoryBeginningId",
                table: "StockReconciliations");

            migrationBuilder.DropColumn(
                name: "InventoryBeginningId",
                table: "StockReconciliations");
        }
    }
}

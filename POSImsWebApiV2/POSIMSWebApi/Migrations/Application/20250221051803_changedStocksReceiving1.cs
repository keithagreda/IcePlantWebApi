using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace POSIMSWebApi.Migrations.Application
{
    /// <inheritdoc />
    public partial class changedStocksReceiving1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MachineProductions_StocksReceivings_StocksReceivingId",
                table: "MachineProductions");

            migrationBuilder.RenameColumn(
                name: "StocksReceivingId",
                table: "MachineProductions",
                newName: "InventoryBeginningId");

            migrationBuilder.RenameIndex(
                name: "IX_MachineProductions_StocksReceivingId",
                table: "MachineProductions",
                newName: "IX_MachineProductions_InventoryBeginningId");

            migrationBuilder.AddColumn<int>(
                name: "ProductId",
                table: "MachineProductions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "Quantity",
                table: "MachineProductions",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "TransNum",
                table: "MachineProductions",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_MachineProductions_ProductId",
                table: "MachineProductions",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_MachineProductions_InventoryBeginnings_InventoryBeginningId",
                table: "MachineProductions",
                column: "InventoryBeginningId",
                principalTable: "InventoryBeginnings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MachineProductions_Products_ProductId",
                table: "MachineProductions",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MachineProductions_InventoryBeginnings_InventoryBeginningId",
                table: "MachineProductions");

            migrationBuilder.DropForeignKey(
                name: "FK_MachineProductions_Products_ProductId",
                table: "MachineProductions");

            migrationBuilder.DropIndex(
                name: "IX_MachineProductions_ProductId",
                table: "MachineProductions");

            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "MachineProductions");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "MachineProductions");

            migrationBuilder.DropColumn(
                name: "TransNum",
                table: "MachineProductions");

            migrationBuilder.RenameColumn(
                name: "InventoryBeginningId",
                table: "MachineProductions",
                newName: "StocksReceivingId");

            migrationBuilder.RenameIndex(
                name: "IX_MachineProductions_InventoryBeginningId",
                table: "MachineProductions",
                newName: "IX_MachineProductions_StocksReceivingId");

            migrationBuilder.AddForeignKey(
                name: "FK_MachineProductions_StocksReceivings_StocksReceivingId",
                table: "MachineProductions",
                column: "StocksReceivingId",
                principalTable: "StocksReceivings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

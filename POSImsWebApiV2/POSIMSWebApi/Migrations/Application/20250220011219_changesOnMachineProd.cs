using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace POSIMSWebApi.Migrations.Application
{
    /// <inheritdoc />
    public partial class changesOnMachineProd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MachineProductions_StocksReceivings_StocksReceivingId",
                table: "MachineProductions");

            migrationBuilder.AlterColumn<Guid>(
                name: "StocksReceivingId",
                table: "MachineProductions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_MachineProductions_StocksReceivings_StocksReceivingId",
                table: "MachineProductions",
                column: "StocksReceivingId",
                principalTable: "StocksReceivings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MachineProductions_StocksReceivings_StocksReceivingId",
                table: "MachineProductions");

            migrationBuilder.AlterColumn<Guid>(
                name: "StocksReceivingId",
                table: "MachineProductions",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddForeignKey(
                name: "FK_MachineProductions_StocksReceivings_StocksReceivingId",
                table: "MachineProductions",
                column: "StocksReceivingId",
                principalTable: "StocksReceivings",
                principalColumn: "Id");
        }
    }
}

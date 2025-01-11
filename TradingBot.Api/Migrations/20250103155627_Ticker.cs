using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TradingBot.Api.Migrations
{
    /// <inheritdoc />
    public partial class Ticker : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Candles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TickerId",
                table: "Candles",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Tickers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ShortName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LongName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tickers", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Candles_TickerId",
                table: "Candles",
                column: "TickerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Candles_Tickers_TickerId",
                table: "Candles",
                column: "TickerId",
                principalTable: "Tickers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Candles_Tickers_TickerId",
                table: "Candles");

            migrationBuilder.DropTable(
                name: "Tickers");

            migrationBuilder.DropIndex(
                name: "IX_Candles_TickerId",
                table: "Candles");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Candles");

            migrationBuilder.DropColumn(
                name: "TickerId",
                table: "Candles");
        }
    }
}

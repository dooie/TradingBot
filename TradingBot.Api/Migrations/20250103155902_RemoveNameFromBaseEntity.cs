using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TradingBot.Api.Migrations
{
    /// <inheritdoc />
    public partial class RemoveNameFromBaseEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "Tickers");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Candles");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Tickers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Candles",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerce.API.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddReservedStockAndOrderStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ReservedStock",
                schema: "catalog",
                table: "Products",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                schema: "order",
                table: "Orders",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReservedStock",
                schema: "catalog",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "order",
                table: "Orders");
        }
    }
}

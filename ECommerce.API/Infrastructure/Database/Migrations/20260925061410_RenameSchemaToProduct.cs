using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerce.API.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class RenameSchemaToProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "product");

            migrationBuilder.RenameTable(
                name: "Products",
                schema: "catalog",
                newName: "Products",
                newSchema: "product");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "catalog");

            migrationBuilder.RenameTable(
                name: "Products",
                schema: "product",
                newName: "Products",
                newSchema: "catalog");
        }
    }
}

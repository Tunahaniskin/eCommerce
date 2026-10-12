using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerce.API.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                schema: "product",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            var defaultCategoryId = Guid.NewGuid();
            migrationBuilder.Sql($"INSERT INTO product.\"Categories\" (\"Id\", \"Name\", \"Description\", \"CreatedAt\", \"IsDeleted\") VALUES ('{defaultCategoryId}', 'Genel Kategori', 'Sistem tarafından oluşturulan varsayılan kategori.', NOW(), false);");

            migrationBuilder.AddColumn<Guid>(
                name: "CategoryId",
                schema: "product",
                table: "Products",
                type: "uuid",
                nullable: false,
                defaultValue: defaultCategoryId);

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId",
                schema: "product",
                table: "Products",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Categories_CategoryId",
                schema: "product",
                table: "Products",
                column: "CategoryId",
                principalSchema: "product",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Categories_CategoryId",
                schema: "product",
                table: "Products");

            migrationBuilder.DropTable(
                name: "Categories",
                schema: "product");

            migrationBuilder.DropIndex(
                name: "IX_Products_CategoryId",
                schema: "product",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                schema: "product",
                table: "Products");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerce.API.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class MigrateImagesToFileSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageData",
                schema: "product",
                table: "ProductImages");

            migrationBuilder.DropColumn(
                name: "ThumbnailData",
                schema: "product",
                table: "ProductImages");

            migrationBuilder.AddColumn<string>(
                name: "ImagePath",
                schema: "product",
                table: "ProductImages",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ThumbnailPath",
                schema: "product",
                table: "ProductImages",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImagePath",
                schema: "product",
                table: "ProductImages");

            migrationBuilder.DropColumn(
                name: "ThumbnailPath",
                schema: "product",
                table: "ProductImages");

            migrationBuilder.AddColumn<byte[]>(
                name: "ImageData",
                schema: "product",
                table: "ProductImages",
                type: "bytea",
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<byte[]>(
                name: "ThumbnailData",
                schema: "product",
                table: "ProductImages",
                type: "bytea",
                nullable: false,
                defaultValue: new byte[0]);
        }
    }
}

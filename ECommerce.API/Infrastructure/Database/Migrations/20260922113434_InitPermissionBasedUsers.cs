using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerce.API.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class InitPermissionBasedUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "auth",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<List<string>>(
                name: "Permissions",
                schema: "auth",
                table: "Users",
                type: "text[]",
                nullable: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "auth",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Permissions",
                schema: "auth",
                table: "Users");
        }
    }
}

using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerce.API.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddDynamicRbacModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Permissions",
                schema: "auth",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Role",
                schema: "auth",
                table: "Users");

            migrationBuilder.AddColumn<Guid>(
                name: "RoleId",
                schema: "auth",
                table: "Users",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "Roles",
                schema: "auth",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserTemporaryPermissions",
                schema: "auth",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Permission = table.Column<string>(type: "text", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    GrantedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTemporaryPermissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserTemporaryPermissions_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "auth",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RolePermissions",
                schema: "auth",
                columns: table => new
                {
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    Permission = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermissions", x => new { x.RoleId, x.Permission });
                    table.ForeignKey(
                        name: "FK_RolePermissions_Roles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "auth",
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_RoleId",
                schema: "auth",
                table: "Users",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserTemporaryPermissions_UserId",
                schema: "auth",
                table: "UserTemporaryPermissions",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Roles_RoleId",
                schema: "auth",
                table: "Users",
                column: "RoleId",
                principalSchema: "auth",
                principalTable: "Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Roles_RoleId",
                schema: "auth",
                table: "Users");

            migrationBuilder.DropTable(
                name: "RolePermissions",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "UserTemporaryPermissions",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "Roles",
                schema: "auth");

            migrationBuilder.DropIndex(
                name: "IX_Users_RoleId",
                schema: "auth",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "RoleId",
                schema: "auth",
                table: "Users");

            migrationBuilder.AddColumn<List<string>>(
                name: "Permissions",
                schema: "auth",
                table: "Users",
                type: "text[]",
                nullable: false);

            migrationBuilder.AddColumn<int>(
                name: "Role",
                schema: "auth",
                table: "Users",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}

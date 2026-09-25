using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerce.API.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCatalogSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP SCHEMA IF EXISTS catalog CASCADE;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}

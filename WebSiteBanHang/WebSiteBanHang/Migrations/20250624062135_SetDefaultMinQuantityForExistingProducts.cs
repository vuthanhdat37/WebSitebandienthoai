using Microsoft.EntityFrameworkCore.Migrations;

namespace Migrations
{
    /// <inheritdoc />
    public partial class SetDefaultMinQuantityForExistingProducts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE Products SET MinQuantity = 10 WHERE MinQuantity = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE Products SET MinQuantity = 0 WHERE MinQuantity = 10");
        }
    }
} 
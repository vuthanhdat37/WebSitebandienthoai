using Microsoft.EntityFrameworkCore.Migrations;

namespace Migrations
{
    /// <inheritdoc />
    public partial class FixMinQuantityDefaultValue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE Products SET MinQuantity = 10 WHERE MinQuantity = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // This is a data correction migration, a rollback is not typically needed
            // but can be added if required.
        }
    }
} 
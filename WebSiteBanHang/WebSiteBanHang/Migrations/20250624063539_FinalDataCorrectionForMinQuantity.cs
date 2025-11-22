using Microsoft.EntityFrameworkCore.Migrations;

namespace Migrations
{
    /// <inheritdoc />
    public partial class FinalDataCorrectionForMinQuantity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE Products SET MinQuantity = 10 WHERE MinQuantity = 0;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No need to revert this data correction
        }
    }
} 
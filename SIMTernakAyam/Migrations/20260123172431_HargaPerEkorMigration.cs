using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIMTernakAyam.Migrations
{
    /// <inheritdoc />
    public partial class HargaPerEkorMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Remove HargaPerKg column from HargaPasar table
            migrationBuilder.DropColumn(
                name: "HargaPerKg",
                table: "HargaPasar");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Add back HargaPerKg column if rollback is needed
            migrationBuilder.AddColumn<decimal>(
                name: "HargaPerKg",
                table: "HargaPasar",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }
    }
}

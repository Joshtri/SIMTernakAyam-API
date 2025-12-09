using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIMTernakAyam.Migrations
{
    /// <inheritdoc />
    public partial class AddKategoriBiayaColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "KategoriBiaya",
                table: "Biayas",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "KategoriBiaya",
                table: "Biayas");
        }
    }
}

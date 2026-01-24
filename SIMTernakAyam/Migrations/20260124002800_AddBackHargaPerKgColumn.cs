using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIMTernakAyam.Migrations
{
    /// <inheritdoc />
    public partial class AddBackHargaPerKgColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Tambahkan kembali kolom HargaPerKg yang sebelumnya dihapus
            migrationBuilder.AddColumn<decimal>(
                name: "HargaPerKg",
                table: "HargaPasar",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Hapus kolom HargaPerKg jika rollback
            migrationBuilder.DropColumn(
                name: "HargaPerKg",
                table: "HargaPasar");
        }
    }
}

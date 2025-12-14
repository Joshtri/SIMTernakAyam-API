using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIMTernakAyam.Migrations
{
    /// <inheritdoc />
    public partial class AddHargaPasarTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HargaPasar",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HargaPerKg = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TanggalMulai = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    TanggalBerakhir = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Keterangan = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsAktif = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Wilayah = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdateAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HargaPasar", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HargaPasar_Period",
                table: "HargaPasar",
                columns: new[] { "IsAktif", "TanggalMulai", "TanggalBerakhir" });

            migrationBuilder.CreateIndex(
                name: "IX_HargaPasar_Wilayah",
                table: "HargaPasar",
                columns: new[] { "Wilayah", "IsAktif" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HargaPasar");
        }
    }
}

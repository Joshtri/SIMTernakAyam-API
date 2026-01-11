using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIMTernakAyam.Migrations
{
    /// <inheritdoc />
    public partial class AddAyamSisaAndAutoFifoFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AlasanSisa",
                table: "Ayams",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsAyamSisa",
                table: "Ayams",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "TanggalDitandaiSisa",
                table: "Ayams",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "LogPeriodeKandangs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    KandangId = table.Column<Guid>(type: "uuid", nullable: false),
                    Periode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TanggalInput = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    JumlahInputBaru = table.Column<int>(type: "integer", nullable: false),
                    SisaDariPeriodeSebelumnya = table.Column<int>(type: "integer", nullable: false),
                    AlasanAdaSisa = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PetugasId = table.Column<Guid>(type: "uuid", nullable: false),
                    KapasitasKandang = table.Column<int>(type: "integer", nullable: false),
                    TotalAyamSetelahInput = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdateAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogPeriodeKandangs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LogPeriodeKandangs_Kandangs_KandangId",
                        column: x => x.KandangId,
                        principalTable: "Kandangs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LogPeriodeKandangs_Users_PetugasId",
                        column: x => x.PetugasId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LogPeriodeKandangs_KandangId",
                table: "LogPeriodeKandangs",
                column: "KandangId");

            migrationBuilder.CreateIndex(
                name: "IX_LogPeriodeKandangs_PetugasId",
                table: "LogPeriodeKandangs",
                column: "PetugasId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LogPeriodeKandangs");

            migrationBuilder.DropColumn(
                name: "AlasanSisa",
                table: "Ayams");

            migrationBuilder.DropColumn(
                name: "IsAyamSisa",
                table: "Ayams");

            migrationBuilder.DropColumn(
                name: "TanggalDitandaiSisa",
                table: "Ayams");
        }
    }
}

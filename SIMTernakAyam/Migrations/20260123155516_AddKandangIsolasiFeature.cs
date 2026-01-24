using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIMTernakAyam.Migrations
{
    /// <inheritdoc />
    public partial class AddKandangIsolasiFeature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TipeKandang",
                table: "Kandangs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "RelokasiAyams",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    KandangAsalId = table.Column<Guid>(type: "uuid", nullable: false),
                    KandangTujuanId = table.Column<Guid>(type: "uuid", nullable: false),
                    AyamAsalId = table.Column<Guid>(type: "uuid", nullable: false),
                    AyamTujuanId = table.Column<Guid>(type: "uuid", nullable: true),
                    JumlahEkor = table.Column<int>(type: "integer", nullable: false),
                    TanggalRelokasi = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    AlasanRelokasi = table.Column<int>(type: "integer", nullable: false),
                    StatusRelokasi = table.Column<int>(type: "integer", nullable: false),
                    Catatan = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    PetugasId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdateAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RelokasiAyams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RelokasiAyams_Ayams_AyamAsalId",
                        column: x => x.AyamAsalId,
                        principalTable: "Ayams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RelokasiAyams_Ayams_AyamTujuanId",
                        column: x => x.AyamTujuanId,
                        principalTable: "Ayams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_RelokasiAyams_Kandangs_KandangAsalId",
                        column: x => x.KandangAsalId,
                        principalTable: "Kandangs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RelokasiAyams_Kandangs_KandangTujuanId",
                        column: x => x.KandangTujuanId,
                        principalTable: "Kandangs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RelokasiAyams_Users_PetugasId",
                        column: x => x.PetugasId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RelokasiAyams_AyamAsalId",
                table: "RelokasiAyams",
                column: "AyamAsalId");

            migrationBuilder.CreateIndex(
                name: "IX_RelokasiAyams_AyamTujuanId",
                table: "RelokasiAyams",
                column: "AyamTujuanId");

            migrationBuilder.CreateIndex(
                name: "IX_RelokasiAyams_KandangAsalId",
                table: "RelokasiAyams",
                column: "KandangAsalId");

            migrationBuilder.CreateIndex(
                name: "IX_RelokasiAyams_KandangTujuanId",
                table: "RelokasiAyams",
                column: "KandangTujuanId");

            migrationBuilder.CreateIndex(
                name: "IX_RelokasiAyams_PetugasId",
                table: "RelokasiAyams",
                column: "PetugasId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RelokasiAyams");

            migrationBuilder.DropColumn(
                name: "TipeKandang",
                table: "Kandangs");
        }
    }
}

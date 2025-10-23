using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIMTernakAyam.Migrations
{
    /// <inheritdoc />
    public partial class AddKandangAsistenTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "KandangAsistens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    KandangId = table.Column<Guid>(type: "uuid", nullable: false),
                    AsistenId = table.Column<Guid>(type: "uuid", nullable: false),
                    Catatan = table.Column<string>(type: "text", nullable: true),
                    IsAktif = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdateAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KandangAsistens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KandangAsistens_Kandangs_KandangId",
                        column: x => x.KandangId,
                        principalTable: "Kandangs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_KandangAsistens_Users_AsistenId",
                        column: x => x.AsistenId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_KandangAsistens_AsistenId",
                table: "KandangAsistens",
                column: "AsistenId");

            migrationBuilder.CreateIndex(
                name: "IX_KandangAsistens_KandangId_AsistenId",
                table: "KandangAsistens",
                columns: new[] { "KandangId", "AsistenId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KandangAsistens");
        }
    }
}

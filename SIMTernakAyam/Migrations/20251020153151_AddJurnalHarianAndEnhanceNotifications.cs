using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIMTernakAyam.Migrations
{
    /// <inheritdoc />
    public partial class AddJurnalHarianAndEnhanceNotifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Category",
                table: "Notifications",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "NotificationType",
                table: "Notifications",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Priority",
                table: "Notifications",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ReceiverRole",
                table: "Notifications",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "JurnalHarians",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PetugasId = table.Column<Guid>(type: "uuid", nullable: false),
                    Tanggal = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    JudulKegiatan = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DeskripsiKegiatan = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    WaktuMulai = table.Column<TimeSpan>(type: "interval", nullable: false),
                    WaktuSelesai = table.Column<TimeSpan>(type: "interval", nullable: false),
                    KandangId = table.Column<Guid>(type: "uuid", nullable: true),
                    Catatan = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    FotoKegiatan = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdateAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JurnalHarians", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JurnalHarians_Kandangs_KandangId",
                        column: x => x.KandangId,
                        principalTable: "Kandangs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_JurnalHarians_Users_PetugasId",
                        column: x => x.PetugasId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_JurnalHarians_KandangId",
                table: "JurnalHarians",
                column: "KandangId");

            migrationBuilder.CreateIndex(
                name: "IX_JurnalHarians_PetugasId",
                table: "JurnalHarians",
                column: "PetugasId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JurnalHarians");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "NotificationType",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "ReceiverRole",
                table: "Notifications");
        }
    }
}

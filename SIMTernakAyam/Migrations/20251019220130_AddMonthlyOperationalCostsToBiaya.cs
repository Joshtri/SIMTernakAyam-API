using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIMTernakAyam.Migrations
{
    /// <inheritdoc />
    public partial class AddMonthlyOperationalCostsToBiaya : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Bulan",
                table: "Biayas",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Catatan",
                table: "Biayas",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "KandangId",
                table: "Biayas",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Tahun",
                table: "Biayas",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Biayas_KandangId",
                table: "Biayas",
                column: "KandangId");

            migrationBuilder.AddForeignKey(
                name: "FK_Biayas_Kandangs_KandangId",
                table: "Biayas",
                column: "KandangId",
                principalTable: "Kandangs",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Biayas_Kandangs_KandangId",
                table: "Biayas");

            migrationBuilder.DropIndex(
                name: "IX_Biayas_KandangId",
                table: "Biayas");

            migrationBuilder.DropColumn(
                name: "Bulan",
                table: "Biayas");

            migrationBuilder.DropColumn(
                name: "Catatan",
                table: "Biayas");

            migrationBuilder.DropColumn(
                name: "KandangId",
                table: "Biayas");

            migrationBuilder.DropColumn(
                name: "Tahun",
                table: "Biayas");
        }
    }
}

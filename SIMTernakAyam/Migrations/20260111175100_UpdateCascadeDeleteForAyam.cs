using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIMTernakAyam.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCascadeDeleteForAyam : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Mortalitas_Ayams_AyamId",
                table: "Mortalitas");

            migrationBuilder.DropForeignKey(
                name: "FK_Panens_Ayams_AyamId",
                table: "Panens");

            migrationBuilder.AddForeignKey(
                name: "FK_Mortalitas_Ayams_AyamId",
                table: "Mortalitas",
                column: "AyamId",
                principalTable: "Ayams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Panens_Ayams_AyamId",
                table: "Panens",
                column: "AyamId",
                principalTable: "Ayams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Mortalitas_Ayams_AyamId",
                table: "Mortalitas");

            migrationBuilder.DropForeignKey(
                name: "FK_Panens_Ayams_AyamId",
                table: "Panens");

            migrationBuilder.AddForeignKey(
                name: "FK_Mortalitas_Ayams_AyamId",
                table: "Mortalitas",
                column: "AyamId",
                principalTable: "Ayams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Panens_Ayams_AyamId",
                table: "Panens",
                column: "AyamId",
                principalTable: "Ayams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

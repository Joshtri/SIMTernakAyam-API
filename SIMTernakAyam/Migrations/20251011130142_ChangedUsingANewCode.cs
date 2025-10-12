using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIMTernakAyam.Migrations
{
    /// <inheritdoc />
    public partial class ChangedUsingANewCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "Kandangs",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "petugasId",
                table: "Kandangs",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Kandangs_UserId",
                table: "Kandangs",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Kandangs_Users_UserId",
                table: "Kandangs",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Kandangs_Users_UserId",
                table: "Kandangs");

            migrationBuilder.DropIndex(
                name: "IX_Kandangs_UserId",
                table: "Kandangs");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Kandangs");

            migrationBuilder.DropColumn(
                name: "petugasId",
                table: "Kandangs");
        }
    }
}

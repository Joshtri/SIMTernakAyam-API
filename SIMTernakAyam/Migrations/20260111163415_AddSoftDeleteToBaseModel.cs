using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIMTernakAyam.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftDeleteToBaseModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Vaksins",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedBy",
                table: "Vaksins",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Vaksins",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Users",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedBy",
                table: "Users",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Panens",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedBy",
                table: "Panens",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Panens",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Pakans",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedBy",
                table: "Pakans",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Pakans",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Operasionals",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedBy",
                table: "Operasionals",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Operasionals",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Notifications",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedBy",
                table: "Notifications",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Notifications",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Mortalitas",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedBy",
                table: "Mortalitas",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Mortalitas",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "LogPeriodeKandangs",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedBy",
                table: "LogPeriodeKandangs",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "LogPeriodeKandangs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Kandangs",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedBy",
                table: "Kandangs",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Kandangs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "KandangAsistens",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedBy",
                table: "KandangAsistens",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "KandangAsistens",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "JurnalHarians",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedBy",
                table: "JurnalHarians",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "JurnalHarians",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "JenisKegiatans",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedBy",
                table: "JenisKegiatans",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "JenisKegiatans",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "HargaPasar",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedBy",
                table: "HargaPasar",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "HargaPasar",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Biayas",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedBy",
                table: "Biayas",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Biayas",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Ayams",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedBy",
                table: "Ayams",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Ayams",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Vaksins");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Vaksins");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Vaksins");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Panens");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Panens");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Panens");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Pakans");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Pakans");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Pakans");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Operasionals");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Operasionals");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Operasionals");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Mortalitas");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Mortalitas");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Mortalitas");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "LogPeriodeKandangs");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "LogPeriodeKandangs");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "LogPeriodeKandangs");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Kandangs");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Kandangs");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Kandangs");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "KandangAsistens");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "KandangAsistens");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "KandangAsistens");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "JurnalHarians");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "JurnalHarians");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "JurnalHarians");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "JenisKegiatans");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "JenisKegiatans");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "JenisKegiatans");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "HargaPasar");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "HargaPasar");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "HargaPasar");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Biayas");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Biayas");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Biayas");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Ayams");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Ayams");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Ayams");
        }
    }
}

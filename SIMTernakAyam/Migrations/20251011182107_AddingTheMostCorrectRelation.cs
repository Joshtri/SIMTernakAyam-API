using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIMTernakAyam.Migrations
{
    /// <inheritdoc />
    public partial class AddingTheMostCorrectRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Ayams",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    KandangId = table.Column<Guid>(type: "uuid", nullable: false),
                    TanggalMasuk = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    JumlahMasuk = table.Column<int>(type: "integer", nullable: false),
                    KandangId1 = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ayams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ayams_Kandangs_KandangId",
                        column: x => x.KandangId,
                        principalTable: "Kandangs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Ayams_Kandangs_KandangId1",
                        column: x => x.KandangId1,
                        principalTable: "Kandangs",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "JenisKegiatans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NamaKegiatan = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Deskripsi = table.Column<string>(type: "text", nullable: true),
                    Satuan = table.Column<string>(type: "text", nullable: true),
                    BiayaDefault = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JenisKegiatans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Pakans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NamaPakan = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Stok = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pakans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Vaksins",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NamaVaksin = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Stok = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vaksins", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Mortalitas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AyamId = table.Column<Guid>(type: "uuid", nullable: false),
                    TanggalKematian = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    JumlahKematian = table.Column<int>(type: "integer", nullable: false),
                    PenyebabKematian = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mortalitas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Mortalitas_Ayams_AyamId",
                        column: x => x.AyamId,
                        principalTable: "Ayams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Panens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AyamId = table.Column<Guid>(type: "uuid", nullable: false),
                    TanggalPanen = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    JumlahEkorPanen = table.Column<int>(type: "integer", nullable: false),
                    BeratRataRata = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Panens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Panens_Ayams_AyamId",
                        column: x => x.AyamId,
                        principalTable: "Ayams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Operasionals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    JenisKegiatanId = table.Column<Guid>(type: "uuid", nullable: false),
                    Tanggal = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Jumlah = table.Column<int>(type: "integer", nullable: false),
                    PetugasId = table.Column<Guid>(type: "uuid", nullable: false),
                    KandangId = table.Column<Guid>(type: "uuid", nullable: false),
                    PakanId = table.Column<Guid>(type: "uuid", nullable: true),
                    VaksinId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Operasionals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Operasionals_JenisKegiatans_JenisKegiatanId",
                        column: x => x.JenisKegiatanId,
                        principalTable: "JenisKegiatans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Operasionals_Kandangs_KandangId",
                        column: x => x.KandangId,
                        principalTable: "Kandangs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Operasionals_Pakans_PakanId",
                        column: x => x.PakanId,
                        principalTable: "Pakans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Operasionals_Users_PetugasId",
                        column: x => x.PetugasId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Operasionals_Vaksins_VaksinId",
                        column: x => x.VaksinId,
                        principalTable: "Vaksins",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Biayas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    JenisBiaya = table.Column<string>(type: "text", nullable: false),
                    Tanggal = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Jumlah = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PetugasId = table.Column<Guid>(type: "uuid", nullable: false),
                    OperasionalId = table.Column<Guid>(type: "uuid", nullable: true),
                    BuktiUrl = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Biayas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Biayas_Operasionals_OperasionalId",
                        column: x => x.OperasionalId,
                        principalTable: "Operasionals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Biayas_Users_PetugasId",
                        column: x => x.PetugasId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Ayams_KandangId",
                table: "Ayams",
                column: "KandangId");

            migrationBuilder.CreateIndex(
                name: "IX_Ayams_KandangId1",
                table: "Ayams",
                column: "KandangId1");

            migrationBuilder.CreateIndex(
                name: "IX_Biayas_OperasionalId",
                table: "Biayas",
                column: "OperasionalId");

            migrationBuilder.CreateIndex(
                name: "IX_Biayas_PetugasId",
                table: "Biayas",
                column: "PetugasId");

            migrationBuilder.CreateIndex(
                name: "IX_Mortalitas_AyamId",
                table: "Mortalitas",
                column: "AyamId");

            migrationBuilder.CreateIndex(
                name: "IX_Operasionals_JenisKegiatanId",
                table: "Operasionals",
                column: "JenisKegiatanId");

            migrationBuilder.CreateIndex(
                name: "IX_Operasionals_KandangId",
                table: "Operasionals",
                column: "KandangId");

            migrationBuilder.CreateIndex(
                name: "IX_Operasionals_PakanId",
                table: "Operasionals",
                column: "PakanId");

            migrationBuilder.CreateIndex(
                name: "IX_Operasionals_PetugasId",
                table: "Operasionals",
                column: "PetugasId");

            migrationBuilder.CreateIndex(
                name: "IX_Operasionals_VaksinId",
                table: "Operasionals",
                column: "VaksinId");

            migrationBuilder.CreateIndex(
                name: "IX_Panens_AyamId",
                table: "Panens",
                column: "AyamId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Biayas");

            migrationBuilder.DropTable(
                name: "Mortalitas");

            migrationBuilder.DropTable(
                name: "Panens");

            migrationBuilder.DropTable(
                name: "Operasionals");

            migrationBuilder.DropTable(
                name: "Ayams");

            migrationBuilder.DropTable(
                name: "JenisKegiatans");

            migrationBuilder.DropTable(
                name: "Pakans");

            migrationBuilder.DropTable(
                name: "Vaksins");
        }
    }
}

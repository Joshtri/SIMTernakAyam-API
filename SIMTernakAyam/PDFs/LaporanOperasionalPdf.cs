using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SIMTernakAyam.DTOs.Laporan;

namespace SIMTernakAyam.PDFs
{
    public class LaporanOperasionalPdf
    {
        private readonly LaporanOperasionalResponseDto _data;

        public LaporanOperasionalPdf(LaporanOperasionalResponseDto data)
        {
            _data = data;
        }

        public byte[] GeneratePdf()
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                    page.Header()
                        .Text("LAPORAN OPERASIONAL KANDANG")
                        .SemiBold().FontSize(16);

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(column =>
                        {
                            column.Spacing(10);

                            // Header Information
                            column.Item().Row(row =>
                            {
                                row.RelativeItem().Column(col =>
                                {
                                    col.Item().Text($"Kandang: {_data.KandangNama}").SemiBold();
                                    col.Item().Text($"Periode: {_data.TanggalMulai:dd/MM/yyyy} - {_data.TanggalAkhir:dd/MM/yyyy}");
                                    col.Item().Text($"Petugas: {_data.PetugasNama}");
                                });
                                row.RelativeItem().Column(col =>
                                {
                                    col.Item().AlignRight().Text($"Tanggal Cetak: {DateTime.Now:dd/MM/yyyy HH:mm}");
                                    col.Item().AlignRight().Text($"Total Kegiatan: {_data.TotalKegiatan}");
                                });
                            });

                            column.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten1);

                            // Summary Cards
                            column.Item().Row(row =>
                            {
                                row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Column(col =>
                                {
                                    col.Item().Text("Total Pakan").FontSize(9);
                                    col.Item().Text($"{_data.TotalPakanDigunakan:N2} Kg").SemiBold().FontSize(12);
                                });
                                row.Spacing(10);
                                row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Column(col =>
                                {
                                    col.Item().Text("Total Vaksin").FontSize(9);
                                    col.Item().Text($"{_data.TotalVaksinDigunakan} Dosis").SemiBold().FontSize(12);
                                });
                                row.Spacing(10);
                                row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Column(col =>
                                {
                                    col.Item().Text("Total Biaya").FontSize(9);
                                    col.Item().Text($"Rp {_data.TotalBiaya:N0}").SemiBold().FontSize(12);
                                });
                            });

                            column.Item().PaddingTop(15).Text("Detail Kegiatan Operasional").SemiBold().FontSize(12);

                            // Table
                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(30);  // No
                                    columns.RelativeColumn(2);   // Tanggal
                                    columns.RelativeColumn(3);   // Kegiatan
                                    columns.RelativeColumn(2);   // Jumlah
                                    columns.RelativeColumn(2);   // Item
                                    columns.RelativeColumn(2);   // Biaya
                                });

                                // Header
                                table.Header(header =>
                                {
                                    header.Cell().Element(CellStyle).Text("No");
                                    header.Cell().Element(CellStyle).Text("Tanggal");
                                    header.Cell().Element(CellStyle).Text("Kegiatan");
                                    header.Cell().Element(CellStyle).Text("Jumlah");
                                    header.Cell().Element(CellStyle).Text("Item");
                                    header.Cell().Element(CellStyle).Text("Biaya (Rp)");

                                    static IContainer CellStyle(IContainer container)
                                    {
                                        return container.Background(Colors.Blue.Darken2)
                                            .Padding(5);
                                    }
                                });

                                // Rows
                                int no = 1;
                                foreach (var item in _data.DetailKegiatan)
                                {
                                    var bgColor = no % 2 == 0 ? Colors.Grey.Lighten3 : Colors.White;

                                    table.Cell().Background(bgColor).Padding(5).Text(no.ToString());
                                    table.Cell().Background(bgColor).Padding(5).Text(item.Tanggal.ToString("dd/MM/yyyy"));
                                    table.Cell().Background(bgColor).Padding(5).Text(item.JenisKegiatanNama);
                                    table.Cell().Background(bgColor).Padding(5).Text($"{item.Jumlah} {item.Satuan}");
                                    table.Cell().Background(bgColor).Padding(5).Text(item.ItemNama ?? "-");
                                    table.Cell().Background(bgColor).Padding(5).Text(item.Biaya?.ToString("N0") ?? "-");

                                    no++;
                                }
                            });

                            // Catatan Pengeluaran
                            if (_data.CatatanPengeluaran?.Any() == true)
                            {
                                column.Item().PaddingTop(15).Text("Catatan Pengeluaran").SemiBold().FontSize(12);
                                column.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Column(col =>
                                {
                                    foreach (var catatan in _data.CatatanPengeluaran)
                                    {
                                        col.Item().Text($"• {catatan}").FontSize(9);
                                    }
                                });
                            }
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Halaman ");
                            x.CurrentPageNumber();
                            x.Span(" dari ");
                            x.TotalPages();
                        });
                });
            });

            return document.GeneratePdf();
        }
    }
}

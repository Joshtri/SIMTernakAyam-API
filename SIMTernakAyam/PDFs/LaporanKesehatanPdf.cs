using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SIMTernakAyam.DTOs.Laporan;

namespace SIMTernakAyam.PDFs
{
    public class LaporanKesehatanPdf
    {
        private readonly LaporanKesehatanResponseDto _data;

        public LaporanKesehatanPdf(LaporanKesehatanResponseDto data)
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
                        .Text("LAPORAN KESEHATAN AYAM")
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
                                });
                                row.RelativeItem().Column(col =>
                                {
                                    col.Item().AlignRight().Text($"Tanggal Cetak: {DateTime.Now:dd/MM/yyyy HH:mm}");
                                    col.Item().AlignRight().Text($"Total Ayam: {_data.JumlahAyam} ekor");
                                });
                            });

                            column.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten1);

                            // Summary Statistics
                            column.Item().Row(row =>
                            {
                                row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Column(col =>
                                {
                                    col.Item().Text("Total Mortalitas").FontSize(9);
                                    col.Item().Text($"{_data.TotalMortalitas} ekor").SemiBold().FontSize(12);
                                    col.Item().Text($"{_data.PersentaseMortalitas:N2}%").FontSize(9);
                                });
                                row.Spacing(10);
                                row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Column(col =>
                                {
                                    col.Item().Text("Vaksinasi").FontSize(9);
                                    col.Item().Text($"{_data.TotalVaksinasi} kali").SemiBold().FontSize(12);
                                });
                                row.Spacing(10);
                                row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Column(col =>
                                {
                                    col.Item().Text("Status").FontSize(9);
                                    col.Item().Text(_data.StatusKesehatan).SemiBold().FontSize(12);
                                });
                            });

                            // Riwayat Mortalitas
                            if (_data.RiwayatMortalitas?.Any() == true)
                            {
                                column.Item().PaddingTop(15).Text("Riwayat Mortalitas").SemiBold().FontSize(12);

                                column.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.ConstantColumn(30);  // No
                                        columns.RelativeColumn(2);   // Tanggal
                                        columns.RelativeColumn(1);   // Jumlah
                                        columns.RelativeColumn(3);   // Penyebab
                                        columns.RelativeColumn(3);   // Keterangan
                                    });

                                    // Header
                                    table.Header(header =>
                                    {
                                        header.Cell().Element(CellStyle).Text("No");
                                        header.Cell().Element(CellStyle).Text("Tanggal");
                                        header.Cell().Element(CellStyle).Text("Jumlah");
                                        header.Cell().Element(CellStyle).Text("Penyebab");
                                        header.Cell().Element(CellStyle).Text("Keterangan");

                                        static IContainer CellStyle(IContainer container)
                                        {
                                            return container.Background(Colors.Green.Darken2)
                                                .Padding(5);
                                        }
                                    });

                                    // Rows
                                    int no = 1;
                                    foreach (var item in _data.RiwayatMortalitas)
                                    {
                                        var bgColor = no % 2 == 0 ? Colors.Grey.Lighten3 : Colors.White;

                                        table.Cell().Background(bgColor).Padding(5).Text(no.ToString());
                                        table.Cell().Background(bgColor).Padding(5).Text(item.Tanggal.ToString("dd/MM/yyyy"));
                                        table.Cell().Background(bgColor).Padding(5).Text($"{item.JumlahMati} ekor");
                                        table.Cell().Background(bgColor).Padding(5).Text(item.PenyebabKematian);
                                        table.Cell().Background(bgColor).Padding(5).Text(item.Keterangan ?? "-");

                                        no++;
                                    }
                                });
                            }

                            // Riwayat Vaksinasi
                            if (_data.RiwayatVaksinasi?.Any() == true)
                            {
                                column.Item().PaddingTop(15).Text("Riwayat Vaksinasi").SemiBold().FontSize(12);

                                column.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.ConstantColumn(30);  // No
                                        columns.RelativeColumn(2);   // Tanggal
                                        columns.RelativeColumn(3);   // Jenis Vaksin
                                        columns.RelativeColumn(2);   // Jumlah
                                        columns.RelativeColumn(2);   // Petugas
                                    });

                                    // Header
                                    table.Header(header =>
                                    {
                                        header.Cell().Element(CellStyle).Text("No");
                                        header.Cell().Element(CellStyle).Text("Tanggal");
                                        header.Cell().Element(CellStyle).Text("Jenis Vaksin");
                                        header.Cell().Element(CellStyle).Text("Jumlah");
                                        header.Cell().Element(CellStyle).Text("Petugas");

                                        static IContainer CellStyle(IContainer container)
                                        {
                                            return container.Background(Colors.Blue.Darken2)
                                                .Padding(5);
                                        }
                                    });

                                    // Rows
                                    int no = 1;
                                    foreach (var item in _data.RiwayatVaksinasi)
                                    {
                                        var bgColor = no % 2 == 0 ? Colors.Grey.Lighten3 : Colors.White;

                                        table.Cell().Background(bgColor).Padding(5).Text(no.ToString());
                                        table.Cell().Background(bgColor).Padding(5).Text(item.Tanggal.ToString("dd/MM/yyyy"));
                                        table.Cell().Background(bgColor).Padding(5).Text(item.JenisVaksin);
                                        table.Cell().Background(bgColor).Padding(5).Text($"{item.Jumlah} dosis");
                                        table.Cell().Background(bgColor).Padding(5).Text(item.PetugasNama);

                                        no++;
                                    }
                                });
                            }

                            // Rekomendasi
                            if (!string.IsNullOrEmpty(_data.Rekomendasi))
                            {
                                column.Item().PaddingTop(15).Text("Rekomendasi").SemiBold().FontSize(12);
                                column.Item().Border(1).BorderColor(Colors.Orange.Lighten2)
                                    .Background(Colors.Orange.Lighten4).Padding(10)
                                    .Text(_data.Rekomendasi).FontSize(9);
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

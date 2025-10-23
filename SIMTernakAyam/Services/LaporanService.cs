using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using SIMTernakAyam.Data;
using SIMTernakAyam.DTOs.Laporan;
using SIMTernakAyam.Enums;
using SIMTernakAyam.PDFs;
using SIMTernakAyam.Services.Interfaces;

namespace SIMTernakAyam.Services
{
    public class LaporanService : ILaporanService
    {
        private readonly ApplicationDbContext _context;

        public LaporanService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<KesehatanKandangDto>> GetKesehatanKandangAsync()
        {
            var kandangs = await _context.Kandangs
                .Include(k => k.User)
                .ToListAsync();

            var result = new List<KesehatanKandangDto>();

            foreach (var kandang in kandangs)
            {
                var kesehatanDto = await BuildKesehatanKandangDto(kandang);
                result.Add(kesehatanDto);
            }

            return result.OrderByDescending(k => k.PersentaseMortalitas).ToList();
        }

        public async Task<KesehatanKandangDto?> GetKesehatanKandangByIdAsync(Guid kandangId)
        {
            var kandang = await _context.Kandangs
                .Include(k => k.User)
                .FirstOrDefaultAsync(k => k.Id == kandangId);

            if (kandang == null)
                return null;

            return await BuildKesehatanKandangDto(kandang);
        }

        private async Task<KesehatanKandangDto> BuildKesehatanKandangDto(Models.Kandang kandang)
        {
            // Ambil semua ayam untuk kandang ini langsung dari database
            var ayamList = await _context.Ayams
                .Where(a => a.KandangId == kandang.Id)
                .ToListAsync();

            // Total ayam yang masuk
            var totalAyamMasuk = ayamList.Sum(a => a.JumlahMasuk);

            // Ambil mortalitas untuk ayam di kandang ini
            var ayamIds = ayamList.Select(a => a.Id).ToList();
            var mortalitasList = await _context.Mortalitas
                .Where(m => ayamIds.Contains(m.AyamId))
                .OrderByDescending(m => m.TanggalKematian)
                .ToListAsync();

            var totalMortalitas = mortalitasList.Sum(m => m.JumlahKematian);
            var ayamHidup = totalAyamMasuk - totalMortalitas;
            var persentaseMortalitas = totalAyamMasuk > 0 ? (decimal)totalMortalitas / totalAyamMasuk * 100 : 0;
            var tingkatKesehatanPersen = totalAyamMasuk > 0 ? (decimal)ayamHidup / totalAyamMasuk * 100 : 0;

            // Tentukan status kesehatan
            string statusKesehatan;
            if (persentaseMortalitas < 5)
                statusKesehatan = "Baik";
            else if (persentaseMortalitas < 10)
                statusKesehatan = "Sedang";
            else
                statusKesehatan = "Buruk";

            // Detail mortalitas terbaru (5 terakhir)
            var mortalitasTerbaru = mortalitasList.Take(5).Select(m => new MortalitasDetailDto
            {
                TanggalKematian = m.TanggalKematian,
                JumlahKematian = m.JumlahKematian,
                PenyebabKematian = m.PenyebabKematian
            }).ToList();

            return new KesehatanKandangDto
            {
                KandangId = kandang.Id,
                NamaKandang = kandang.NamaKandang,
                Lokasi = kandang.Lokasi,
                Kapasitas = kandang.Kapasitas,
                PetugasId = kandang.petugasId,
                NamaPetugas = kandang.User?.FullName ?? "-",
                UsernamePetugas = kandang.User?.Username ?? "-",
                EmailPetugas = kandang.User?.Email ?? "-",
                NoWAPetugas = kandang.User?.NoWA ?? "-",
                TotalAyamMasuk = totalAyamMasuk,
                TotalMortalitas = totalMortalitas,
                PersentaseMortalitas = Math.Round(persentaseMortalitas, 2),
                AyamHidup = ayamHidup,
                TingkatKesehatanPersen = Math.Round(tingkatKesehatanPersen, 2),
                StatusKesehatan = statusKesehatan,
                MortalitasTerbaru = mortalitasTerbaru
            };
        }

        public async Task<LaporanOperasionalDto> GetLaporanOperasionalAsync(DateTime startDate, DateTime endDate)
        {
            var operasionals = await _context.Operasionals
                .Include(o => o.JenisKegiatan)
                .Include(o => o.Kandang)
                .Include(o => o.Petugas)
                .Where(o => o.Tanggal >= startDate && o.Tanggal <= endDate)
                .ToListAsync();

            // Tentukan periode
            var periode = GetPeriodeName(startDate, endDate);

            // Detail per Jenis Kegiatan
            var detailPerJenis = operasionals
                .GroupBy(o => o.JenisKegiatan.NamaKegiatan)
                .Select(g => new OperasionalPerJenisDto
                {
                    NamaJenisKegiatan = g.Key,
                    JumlahKegiatan = g.Count(),
                    TotalJumlah = g.Sum(o => o.Jumlah)
                })
                .ToList();

            // Detail per Kandang
            var detailPerKandang = operasionals
                .GroupBy(o => o.Kandang)
                .Select(g => new OperasionalPerKandangDto
                {
                    NamaKandang = g.Key.NamaKandang,
                    Lokasi = g.Key.Lokasi,
                    JumlahOperasional = g.Count(),
                    NamaPetugas = g.Key.User?.FullName ?? "-"
                })
                .ToList();

            // Detail per Petugas
            var detailPerPetugas = operasionals
                .GroupBy(o => o.Petugas)
                .Select(g => new OperasionalPerPetugasDto
                {
                    NamaPetugas = g.Key.FullName ?? "-",
                    Username = g.Key.Username,
                    JumlahOperasional = g.Count(),
                    KandangDikelola = g.Select(o => o.Kandang.NamaKandang).Distinct().ToList()
                })
                .ToList();

            return new LaporanOperasionalDto
            {
                Periode = periode,
                TanggalMulai = startDate,
                TanggalSelesai = endDate,
                TotalOperasional = operasionals.Count,
                TotalKandang = operasionals.Select(o => o.KandangId).Distinct().Count(),
                TotalPetugas = operasionals.Select(o => o.PetugasId).Distinct().Count(),
                DetailPerJenis = detailPerJenis,
                DetailPerKandang = detailPerKandang,
                DetailPerPetugas = detailPerPetugas
            };
        }

        public async Task<List<AnalisisProduktivitasDto>> GetAnalisisProduktivitasAsync()
        {
            // Ambil semua petugas
            var petugasList = await _context.Users
                .Where(u => u.Role == RoleEnum.Petugas)
                .ToListAsync();

            var result = new List<AnalisisProduktivitasDto>();

            foreach (var petugas in petugasList)
            {
                var produktivitasDto = await BuildAnalisisProduktivitasDto(petugas);
                result.Add(produktivitasDto);
            }

            return result.OrderByDescending(p => p.SkorProduktivitas).ToList();
        }

        public async Task<AnalisisProduktivitasDto?> GetAnalisisProduktivitasByPetugasAsync(Guid petugasId)
        {
            var petugas = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == petugasId && u.Role == RoleEnum.Petugas);

            if (petugas == null)
                return null;

            return await BuildAnalisisProduktivitasDto(petugas);
        }

        private async Task<AnalisisProduktivitasDto> BuildAnalisisProduktivitasDto(Models.User petugas)
        {
            // Ambil kandang yang dikelola
            var kandangs = await _context.Kandangs
                .Where(k => k.petugasId == petugas.Id)
                .ToListAsync();

            var produktivitasKandangList = new List<ProduktivitasKandangDto>();

            int totalAyamDikelola = 0;
            int totalMortalitas = 0;
            int totalOperasional = 0;

            foreach (var kandang in kandangs)
            {
                // Ambil semua ayam untuk kandang ini
                var ayamList = await _context.Ayams
                    .Where(a => a.KandangId == kandang.Id)
                    .ToListAsync();

                // Total ayam yang masuk ke kandang ini
                var totalAyamKandang = ayamList.Sum(a => a.JumlahMasuk);
                totalAyamDikelola += totalAyamKandang;

                // Hitung mortalitas untuk kandang ini
                var ayamIds = ayamList.Select(a => a.Id).ToList();
                var mortalitasKandang = 0;
                if (ayamIds.Any())
                {
                    mortalitasKandang = await _context.Mortalitas
                        .Where(m => ayamIds.Contains(m.AyamId))
                        .SumAsync(m => m.JumlahKematian);
                }
                totalMortalitas += mortalitasKandang;

                // Operasional untuk kandang ini
                var operasionalKandang = await _context.Operasionals
                    .Where(o => o.KandangId == kandang.Id && o.PetugasId == petugas.Id)
                    .CountAsync();
                totalOperasional += operasionalKandang;

                // Kegiatan terakhir
                var operasionalTerakhir = await _context.Operasionals
                    .Include(o => o.JenisKegiatan)
                    .Where(o => o.KandangId == kandang.Id && o.PetugasId == petugas.Id)
                    .OrderByDescending(o => o.Tanggal)
                    .FirstOrDefaultAsync();

                // Hitung persentase
                var persentaseMortalitas = totalAyamKandang > 0 ? (decimal)mortalitasKandang / totalAyamKandang * 100 : 0;
                var tingkatPengisian = kandang.Kapasitas > 0 ? (decimal)totalAyamKandang / kandang.Kapasitas * 100 : 0;

                produktivitasKandangList.Add(new ProduktivitasKandangDto
                {
                    KandangId = kandang.Id,
                    NamaKandang = kandang.NamaKandang,
                    Lokasi = kandang.Lokasi,
                    Kapasitas = kandang.Kapasitas,
                    TotalAyam = totalAyamKandang,
                    TotalOperasional = operasionalKandang,
                    TotalMortalitas = mortalitasKandang,
                    PersentaseMortalitas = Math.Round(persentaseMortalitas, 2),
                    TingkatPengisianPersen = Math.Round(tingkatPengisian, 2),
                    TanggalOperasionalTerakhir = operasionalTerakhir?.Tanggal,
                    JenisKegiatanTerakhir = operasionalTerakhir?.JenisKegiatan?.NamaKegiatan
                });
            }

            // Hitung rata-rata mortalitas keseluruhan
            var rataMortalitas = totalAyamDikelola > 0 ? (decimal)totalMortalitas / totalAyamDikelola * 100 : 0;

            // Hitung skor produktivitas (0-100)
            // Faktor: jumlah operasional (50%), tingkat kesehatan/rendah mortalitas (50%)
            var skorOperasional = Math.Min(totalOperasional * 2, 50); // Max 50 poin
            var skorKesehatan = Math.Max(50 - (rataMortalitas * 5), 0); // Max 50 poin, berkurang seiring mortalitas
            var skorProduktivitas = skorOperasional + skorKesehatan;

            // Rating performa
            string ratingPerforma;
            if (skorProduktivitas >= 80)
                ratingPerforma = "Sangat Baik";
            else if (skorProduktivitas >= 60)
                ratingPerforma = "Baik";
            else if (skorProduktivitas >= 40)
                ratingPerforma = "Cukup";
            else
                ratingPerforma = "Kurang";

            return new AnalisisProduktivitasDto
            {
                PetugasId = petugas.Id,
                NamaPetugas = petugas.FullName ?? "-",
                Username = petugas.Username,
                Email = petugas.Email,
                KandangDikelola = produktivitasKandangList,
                TotalKandang = kandangs.Count,
                TotalOperasional = totalOperasional,
                TotalAyamDikelola = totalAyamDikelola,
                TotalMortalitas = totalMortalitas,
                RataMortalitasPersen = Math.Round(rataMortalitas, 2),
                RatingPerforma = ratingPerforma,
                SkorProduktivitas = Math.Round(skorProduktivitas, 2)
            };
        }

        private string GetPeriodeName(DateTime startDate, DateTime endDate)
        {
            var daysDiff = (endDate - startDate).Days;

            if (daysDiff <= 7)
            {
                // Mingguan
                var weekNumber = (startDate.Day - 1) / 7 + 1;
                return $"Minggu {weekNumber} {startDate:MMMM yyyy}";
            }
            else if (daysDiff <= 31)
            {
                // Bulanan
                return startDate.ToString("MMMM yyyy");
            }
            else
            {
                // Custom range
                return $"{startDate:dd MMM yyyy} - {endDate:dd MMM yyyy}";
            }
        }

        public async Task<byte[]> GenerateLaporanOperasionalPdfAsync(Guid kandangId, DateTime startDate, DateTime endDate)
        {
            // Inisialisasi QuestPDF
            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

            var kandang = await _context.Kandangs
                .Include(k => k.User)
                .FirstOrDefaultAsync(k => k.Id == kandangId);

            if (kandang == null)
                throw new Exception("Kandang tidak ditemukan");

            // Ambil data operasional
            var operasionals = await _context.Operasionals
                .Include(o => o.JenisKegiatan)
                .Include(o => o.Petugas)
                .Include(o => o.Pakan)
                .Include(o => o.Vaksin)
                .Where(o => o.KandangId == kandangId && o.Tanggal >= startDate && o.Tanggal <= endDate)
                .OrderBy(o => o.Tanggal)
                .ToListAsync();

            // Ambil data biaya
            var biayaList = await _context.Biayas
                .Where(b => b.KandangId == kandangId && b.Tanggal >= startDate && b.Tanggal <= endDate)
                .ToListAsync();

            // Build DTO
            var laporan = new LaporanOperasionalResponseDto
            {
                KandangNama = kandang.NamaKandang,
                PetugasNama = kandang.User?.FullName ?? "N/A",
                TanggalMulai = startDate,
                TanggalAkhir = endDate,
                TotalKegiatan = operasionals.Count,
                TotalPakanDigunakan = operasionals.Where(o => o.PakanId != null).Sum(o => o.Jumlah),
                TotalVaksinDigunakan = operasionals.Where(o => o.VaksinId != null).Sum(o => o.Jumlah),
                TotalBiaya = biayaList.Sum(b => b.Jumlah)
            };

            // Detail kegiatan
            foreach (var op in operasionals)
            {
                laporan.DetailKegiatan.Add(new DetailKegiatanDto
                {
                    Tanggal = op.Tanggal,
                    JenisKegiatanNama = op.JenisKegiatan.NamaKegiatan,
                    Jumlah = op.Jumlah,
                    Satuan = op.JenisKegiatan.Satuan ?? "",
                    ItemNama = op.Pakan?.NamaPakan ?? op.Vaksin?.NamaVaksin,
                    Biaya = biayaList.FirstOrDefault(b => b.OperasionalId == op.Id)?.Jumlah
                });
            }

            // Catatan pengeluaran
            foreach (var biaya in biayaList.Where(b => !string.IsNullOrEmpty(b.Catatan)))
            {
                laporan.CatatanPengeluaran.Add($"{biaya.JenisBiaya}: {biaya.Catatan} (Rp {biaya.Jumlah:N0})");
            }

            // Generate PDF
            var pdfGenerator = new LaporanOperasionalPdf(laporan);
            return pdfGenerator.GeneratePdf();
        }

        public async Task<byte[]> GenerateLaporanKesehatanPdfAsync(Guid kandangId, DateTime startDate, DateTime endDate)
        {
            // Inisialisasi QuestPDF
            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

            var kandang = await _context.Kandangs
                .FirstOrDefaultAsync(k => k.Id == kandangId);

            if (kandang == null)
                throw new Exception("Kandang tidak ditemukan");

            // Ambil semua ayam untuk kandang ini langsung dari database
            var ayamList = await _context.Ayams
                .Where(a => a.KandangId == kandangId)
                .ToListAsync();

            // Hitung jumlah ayam
            var jumlahAyam = ayamList.Sum(a => a.JumlahMasuk);

            // Ambil data mortalitas
            var ayamIds = ayamList.Select(a => a.Id).ToList();
            var mortalitasList = await _context.Mortalitas
                .Where(m => ayamIds.Contains(m.AyamId) &&
                            m.TanggalKematian >= startDate && m.TanggalKematian <= endDate)
                .OrderBy(m => m.TanggalKematian)
                .ToListAsync();

            var totalMortalitas = mortalitasList.Sum(m => m.JumlahKematian);
            var persentaseMortalitas = jumlahAyam > 0 ? (decimal)totalMortalitas / jumlahAyam * 100 : 0;

            // Ambil data vaksinasi
            var vaksinasiList = await _context.Operasionals
                .Include(o => o.Vaksin)
                .Include(o => o.Petugas)
                .Where(o => o.KandangId == kandangId && o.VaksinId != null &&
                            o.Tanggal >= startDate && o.Tanggal <= endDate)
                .OrderBy(o => o.Tanggal)
                .ToListAsync();

            // Build DTO
            var laporan = new LaporanKesehatanResponseDto
            {
                KandangNama = kandang.NamaKandang,
                TanggalMulai = startDate,
                TanggalAkhir = endDate,
                JumlahAyam = jumlahAyam,
                TotalMortalitas = totalMortalitas,
                PersentaseMortalitas = persentaseMortalitas,
                TotalVaksinasi = vaksinasiList.Count,
                StatusKesehatan = persentaseMortalitas > 5 ? "Perlu Perhatian" : "Sehat"
            };

            // Riwayat Mortalitas
            foreach (var m in mortalitasList)
            {
                laporan.RiwayatMortalitas.Add(new RiwayatMortalitasDto
                {
                    Tanggal = m.TanggalKematian,
                    JumlahMati = m.JumlahKematian,
                    PenyebabKematian = m.PenyebabKematian,
                    Keterangan = null // Model tidak punya field Keterangan
                });
            }

            // Riwayat Vaksinasi
            foreach (var v in vaksinasiList)
            {
                laporan.RiwayatVaksinasi.Add(new RiwayatVaksinasiDto
                {
                    Tanggal = v.Tanggal,
                    JenisVaksin = v.Vaksin?.NamaVaksin ?? "N/A",
                    Jumlah = v.Jumlah,
                    PetugasNama = v.Petugas?.FullName ?? "N/A"
                });
            }

            // Rekomendasi
            if (persentaseMortalitas > 10)
            {
                laporan.Rekomendasi = "PERHATIAN: Tingkat mortalitas tinggi (>10%). Segera lakukan pemeriksaan kesehatan menyeluruh dan konsultasi dengan dokter hewan.";
            }
            else if (persentaseMortalitas > 5)
            {
                laporan.Rekomendasi = "Tingkat mortalitas sedikit meningkat. Perhatikan kondisi kandang, kebersihan, dan pemberian pakan yang cukup.";
            }
            else
            {
                laporan.Rekomendasi = "Kondisi kesehatan kandang baik. Lanjutkan perawatan rutin dan vaksinasi sesuai jadwal.";
            }

            // Generate PDF
            var pdfGenerator = new LaporanKesehatanPdf(laporan);
            return pdfGenerator.GeneratePdf();
        }
    }
}

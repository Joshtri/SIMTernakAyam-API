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
                    KandangId = g.Key.Id,
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

        public async Task<List<AnalisisProduktivitasDto>> GetAnalisisProduktivitasAsync(int? year = null, int? month = null, bool? hasKandang = null)
        {
            // Ambil semua petugas
            var petugasList = await _context.Users
                .Where(u => u.Role == RoleEnum.Petugas)
                .ToListAsync();

            var result = new List<AnalisisProduktivitasDto>();

            foreach (var petugas in petugasList)
            {
                var produktivitasDto = await BuildAnalisisProduktivitasDto(petugas, year, month);
                result.Add(produktivitasDto);
            }

            // Filter berdasarkan hasKandang jika parameter ada
            if (hasKandang.HasValue)
            {
                if (hasKandang.Value)
                {
                    // Hanya petugas yang mengelola kandang (totalKandang > 0)
                    result = result.Where(p => p.TotalKandang > 0).ToList();
                }
                else
                {
                    // Hanya petugas yang tidak mengelola kandang (totalKandang == 0)
                    result = result.Where(p => p.TotalKandang == 0).ToList();
                }
            }

            return result.OrderByDescending(p => p.SkorProduktivitas).ToList();
        }

        public async Task<AnalisisProduktivitasDto?> GetAnalisisProduktivitasByPetugasAsync(Guid petugasId, int? year = null, int? month = null, bool? hasKandang = null)
        {
            var petugas = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == petugasId && u.Role == RoleEnum.Petugas);

            if (petugas == null)
                return null;

            var result = await BuildAnalisisProduktivitasDto(petugas, year, month);

            // Filter berdasarkan hasKandang jika parameter ada
            if (hasKandang.HasValue)
            {
                if (hasKandang.Value && result.TotalKandang == 0)
                {
                    // Diminta petugas yang mengelola kandang tapi petugas ini tidak mengelola
                    return null;
                }
                else if (!hasKandang.Value && result.TotalKandang > 0)
                {
                    // Diminta petugas yang tidak mengelola kandang tapi petugas ini mengelola
                    return null;
                }
            }

            return result;
        }

        private async Task<AnalisisProduktivitasDto> BuildAnalisisProduktivitasDto(Models.User petugas, int? year = null, int? month = null)
        {
            // Setup filter date range
            DateTime? startDate = null;
            DateTime? endDate = null;

            if (year.HasValue)
            {
                if (month.HasValue)
                {
                    // Filter by specific month
                    startDate = DateTime.SpecifyKind(new DateTime(year.Value, month.Value, 1), DateTimeKind.Utc);
                    endDate = startDate.Value.AddMonths(1);
                }
                else
                {
                    // Filter by year
                    startDate = DateTime.SpecifyKind(new DateTime(year.Value, 1, 1), DateTimeKind.Utc);
                    endDate = startDate.Value.AddYears(1);
                }
            }

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
                    var mortalitasQuery = _context.Mortalitas.Where(m => ayamIds.Contains(m.AyamId));

                    // Apply date filter if specified
                    if (startDate.HasValue && endDate.HasValue)
                    {
                        mortalitasQuery = mortalitasQuery.Where(m => m.TanggalKematian >= startDate.Value && m.TanggalKematian < endDate.Value);
                    }

                    mortalitasKandang = await mortalitasQuery.SumAsync(m => m.JumlahKematian);
                }
                totalMortalitas += mortalitasKandang;

                // Operasional untuk kandang ini
                var operasionalQuery = _context.Operasionals
                    .Where(o => o.KandangId == kandang.Id && o.PetugasId == petugas.Id);

                // Apply date filter if specified
                if (startDate.HasValue && endDate.HasValue)
                {
                    operasionalQuery = operasionalQuery.Where(o => o.Tanggal >= startDate.Value && o.Tanggal < endDate.Value);
                }

                var operasionalKandang = await operasionalQuery.CountAsync();
                totalOperasional += operasionalKandang;

                // Kegiatan terakhir
                var operasionalTerakhirQuery = _context.Operasionals
                    .Include(o => o.JenisKegiatan)
                    .Where(o => o.KandangId == kandang.Id && o.PetugasId == petugas.Id);

                // Apply date filter if specified
                if (startDate.HasValue && endDate.HasValue)
                {
                    operasionalTerakhirQuery = operasionalTerakhirQuery.Where(o => o.Tanggal >= startDate.Value && o.Tanggal < endDate.Value);
                }

                var operasionalTerakhir = await operasionalTerakhirQuery
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
            decimal skorProduktivitas = 0;
            string ratingPerforma;

            // Jika petugas tidak mengelola kandang sama sekali, skor = 0
            if (kandangs.Count == 0)
            {
                skorProduktivitas = 0;
                ratingPerforma = "Tidak Ada Data";
            }
            // Jika tidak ada operasional sama sekali (di periode tertentu atau secara keseluruhan), skor = 0
            else if (totalOperasional == 0)
            {
                skorProduktivitas = 0;
                ratingPerforma = "Tidak Ada Aktivitas";
            }
            else
            {
                // Faktor: jumlah operasional (50%), tingkat kesehatan/rendah mortalitas (50%)
                var skorOperasional = Math.Min(totalOperasional * 2, 50); // Max 50 poin
                var skorKesehatan = Math.Max(50 - (rataMortalitas * 5), 0); // Max 50 poin, berkurang seiring mortalitas
                skorProduktivitas = skorOperasional + skorKesehatan;

                // Rating performa
                if (skorProduktivitas >= 80)
                    ratingPerforma = "Sangat Baik";
                else if (skorProduktivitas >= 60)
                    ratingPerforma = "Baik";
                else if (skorProduktivitas >= 40)
                    ratingPerforma = "Cukup";
                else
                    ratingPerforma = "Kurang";
            }

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

        #region Batch Reporting

        public async Task<List<BatchOptionDto>> GetBatchesAsync()
        {
            var batches = await _context.Ayams
                .Include(a => a.Kandang)
                .OrderByDescending(a => a.TanggalMasuk)
                .Select(a => new { a.Id, a.TanggalMasuk, KandangNama = a.Kandang.NamaKandang })
                .ToListAsync();

            return batches.Select(b => new BatchOptionDto
            {
                Id = b.Id,
                Name = $"Batch {b.TanggalMasuk:MMM yyyy} - {b.KandangNama}"
            }).ToList();
        }

        public async Task<LaporanBatchDto?> GetLaporanBatchAsync(Guid batchId)
        {
            var ayam = await _context.Ayams
                .Include(a => a.Kandang)
                .FirstOrDefaultAsync(a => a.Id == batchId);

            if (ayam == null) return null;

            var panens = await _context.Panens
                .Where(p => p.AyamId == batchId)
                .OrderBy(p => p.TanggalPanen)
                .ToListAsync();

            var tanggalMulai = ayam.TanggalMasuk;
            var tanggalSelesai = panens.Any() ? panens.Max(p => p.TanggalPanen) : (DateTime?)null;
            var calculationEndDate = tanggalSelesai ?? DateTime.Now;

            // Calculate Expenses
            var expenses = await _context.Biayas
                .Where(b => b.KandangId == ayam.KandangId &&
                            b.Tanggal >= tanggalMulai &&
                            b.Tanggal <= calculationEndDate)
                .ToListAsync();

            var totalBiaya = expenses.Sum(b => b.Jumlah);

            // Breakdown Expenses
            // Use Contains for simple keyword matching if explicit ID is missing (fallback)
            var biayaPakan = expenses.Where(b => b.JenisBiaya.Contains("Pakan", StringComparison.OrdinalIgnoreCase)).Sum(b => b.Jumlah);
            var biayaVaksin = expenses.Where(b => b.JenisBiaya.Contains("Vaksin", StringComparison.OrdinalIgnoreCase)).Sum(b => b.Jumlah);
            var biayaLain = totalBiaya - biayaPakan - biayaVaksin;

            // Calculate Revenue
            decimal totalPendapatan = 0;
            if (panens.Any())
            {
                // Get latest market price or average valid for the harvest date
                // Ideally, we should get price at harvest date.
                // Since this is a simple implementation, we take the latest price available.
                var latestPrice = await _context.HargaPasar
                    .OrderByDescending(h => h.TanggalMulai)
                    .FirstOrDefaultAsync();
                
                var hargaPerEkor = latestPrice?.HargaPerEkor ?? 25000; // Default fallback

                // Use Per Ekor calculation as requested by recent migration
                foreach (var p in panens)
                {
                    totalPendapatan += p.JumlahEkorPanen * hargaPerEkor;
                }
            }

            var populasiAwal = ayam.JumlahMasuk;
            var totalKematian = await _context.Mortalitas
                .Where(m => m.AyamId == batchId)
                .SumAsync(m => m.JumlahKematian);

            var totalPanen = panens.Sum(p => p.JumlahEkorPanen);
            var populasiSaatIni = populasiAwal - totalKematian - totalPanen;
            if (populasiSaatIni < 0) populasiSaatIni = 0;

            // Calculate Performance Metrics
            double mortalityRate = populasiAwal > 0 ? (double)totalKematian / populasiAwal * 100 : 0;
            decimal avgWeight = panens.Any() ? panens.Average(p => p.BeratRataRata) : 0;
            
            // FCR Calculation (Total Feed / Total Weight Gained)
            // We need Pakan usage specifically from Operasionals or Biaya??
            // Ideally from Operasional which tracks quantity (kg). Biaya tracks money (Rp).
            // Let's try to query Operasional for FCR accuracy
            double fcr = 0;
            
            try 
            {
                var pakanUsedKg = await _context.Operasionals
                     .Where(o => o.KandangId == ayam.KandangId && 
                                 o.Tanggal >= tanggalMulai && 
                                 o.Tanggal <= calculationEndDate &&
                                 o.PakanId != null)
                     .SumAsync(o => o.Jumlah);

                var totalWeightHarvested = panens.Sum(p => p.JumlahEkorPanen * p.BeratRataRata);
                
                if (totalWeightHarvested > 0)
                {
                    fcr = (double)pakanUsedKg / (double)totalWeightHarvested;
                }
            }
            catch 
            {
                // Ignore FCR errors if Operasional table has issues
                fcr = 0;
            }

            return new LaporanBatchDto
            {
                BatchId = ayam.Id,
                KandangId = ayam.KandangId,
                NamaKandang = ayam.Kandang?.NamaKandang ?? "-",
                Periode = $"{tanggalMulai:dd MMM yyyy} - {(tanggalSelesai.HasValue ? tanggalSelesai.Value.ToString("dd MMM yyyy") : "Sekarang")}",
                TanggalMulai = tanggalMulai,
                TanggalSelesai = tanggalSelesai,
                Status = tanggalSelesai.HasValue ? "Selesai" : "Aktif",
                
                PopulasiAwal = populasiAwal,
                PopulasiSaatIni = populasiSaatIni,
                TotalKematian = totalKematian,
                TotalPanen = totalPanen,
                
                TotalPendapatan = totalPendapatan,
                TotalBiaya = totalBiaya,
                Keuntungan = totalPendapatan - totalBiaya,
                
                BiayaPakan = biayaPakan,
                BiayaVaksin = biayaVaksin,
                BiayaOperasionalLain = biayaLain,

                FCR = Math.Round(fcr, 2),
                MortalityRate = Math.Round(mortalityRate, 2),
                AverageWeight = Math.Round(avgWeight, 2)
            };
        }

        #endregion
    }
}

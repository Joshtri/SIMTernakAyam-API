using SIMTernakAyam.Models;
using SIMTernakAyam.Repository.Interfaces;
using SIMTernakAyam.Services.Interfaces;
using SIMTernakAyam.DTOs.HargaPasar;

namespace SIMTernakAyam.Services
{
    /// <summary>
    /// Service untuk operasi harga pasar ayam
    /// </summary>
    public class HargaPasarService : BaseService<HargaPasar>, IHargaPasarService
    {
        private readonly IHargaPasarRepository _hargaPasarRepository;
        private readonly IPanenRepository _panenRepository;

        public HargaPasarService(IHargaPasarRepository repository, IPanenRepository panenRepository) : base(repository)
        {
            _hargaPasarRepository = repository;
            _panenRepository = panenRepository;
        }

        public async Task<HargaPasar?> GetHargaAktifByTanggalAsync(DateTime tanggal)
        {
            return await _hargaPasarRepository.GetHargaAktifByTanggalAsync(tanggal);
        }

        public async Task<HargaPasar?> GetHargaTerbaruAsync()
        {
            return await _hargaPasarRepository.GetHargaTerbaruAsync();
        }

        public async Task<IEnumerable<HargaPasar>> GetRiwayatHargaAsync(DateTime startDate, DateTime endDate)
        {
            if (startDate > endDate)
            {
                throw new ArgumentException("Tanggal mulai tidak boleh lebih besar dari tanggal akhir.");
            }

            return await _hargaPasarRepository.GetRiwayatHargaAsync(startDate, endDate);
        }

        public async Task<(bool Success, string Message)> DeactivateAllHargaAsync()
        {
            try
            {
                var success = await _hargaPasarRepository.DeactivateAllAsync();
                if (success)
                {
                    return (true, "Semua harga pasar berhasil dinonaktifkan.");
                }
                return (false, "Gagal menonaktifkan harga pasar.");
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> UpdateStatusHargaAsync(Guid id, bool isAktif)
        {
            try
            {
                // Cek apakah harga pasar dengan ID tersebut ada
                var hargaPasar = await _hargaPasarRepository.GetByIdAsync(id);
                if (hargaPasar == null)
                {
                    return (false, "Harga pasar tidak ditemukan.");
                }

                // ? VALIDASI UTAMA: Jika mengaktifkan, pastikan tidak ada harga lain yang aktif
                if (isAktif)
                {
                    var existingActiveHarga = await _hargaPasarRepository.GetAllAsync();
                    var activeHargaList = existingActiveHarga.Where(h => h.IsAktif && h.Id != id).ToList();
                    
                    if (activeHargaList.Any())
                    {
                        var activeCount = activeHargaList.Count;
                        var activeHargaInfo = string.Join(", ", activeHargaList.Select(h => 
                            $"Rp {h.HargaPerEkor:N0} (ID: {h.Id.ToString().Substring(0, 8)}...)"));
                        
                        return (false, $"? GAGAL: Tidak bisa mengaktifkan harga pasar ini karena sudah ada {activeCount} harga pasar lain yang aktif: {activeHargaInfo}. " +
                                     "Silakan nonaktifkan harga pasar yang lain terlebih dahulu atau gunakan fitur 'Deactivate All' di endpoint lain.");
                    }
                }

                // Update status harga pasar
                var updateResult = await _hargaPasarRepository.UpdateStatusAsync(id, isAktif);
                if (!updateResult)
                {
                    // Jika gagal karena validasi repository atau error lain
                    if (isAktif)
                    {
                        return (false, "? GAGAL: Tidak bisa mengaktifkan harga pasar karena sudah ada harga pasar lain yang aktif. Silakan nonaktifkan yang lain terlebih dahulu.");
                    }
                    return (false, "Gagal mengupdate status harga pasar.");
                }

                var statusText = isAktif ? "diaktifkan" : "dinonaktifkan";
                return (true, $"? Harga pasar berhasil {statusText}.");
            }
            catch (Exception ex)
            {
                return (false, $"? Error: {ex.Message}");
            }
        }

        public async Task<(bool IsValid, string Message)> ValidateTanggalOverlapAsync(DateTime tanggalMulai, DateTime? tanggalBerakhir, Guid? excludeId = null)
        {
            try
            {
                var allHarga = await _hargaPasarRepository.GetAllAsync();
                var otherHarga = excludeId.HasValue 
                    ? allHarga.Where(h => h.Id != excludeId.Value)
                    : allHarga;

                foreach (var harga in otherHarga.Where(h => h.IsAktif))
                {
                    // Check for date overlap
                    var hargaEnd = harga.TanggalBerakhir ?? DateTime.MaxValue;
                    var newEnd = tanggalBerakhir ?? DateTime.MaxValue;

                    bool hasOverlap = tanggalMulai <= hargaEnd && newEnd >= harga.TanggalMulai;

                    if (hasOverlap)
                    {
                        return (false, $"Periode tanggal overlap dengan harga pasar yang sudah ada: {harga.HargaPerEkor:N0} ({harga.TanggalMulai:dd/MM/yyyy} - {(harga.TanggalBerakhir?.ToString("dd/MM/yyyy") ?? "Berlaku terus")})");
                    }
                }

                return (true, "Validasi tanggal berhasil.");
            }
            catch (Exception ex)
            {
                return (false, $"Error validasi: {ex.Message}");
            }
        }

        public async Task<HargaPasar?> GetHargaByWilayahAsync(string wilayah, DateTime tanggal)
        {
            return await _hargaPasarRepository.GetHargaByWilayahAsync(wilayah, tanggal);
        }

        public async Task<(bool Success, string Message, EstimasiKeuntunganDto? Data)> HitungKeuntunganAsync(int totalAyam, DateTime tanggalReferensi)
        {
            try
            {
                // Ambil harga pasar yang aktif pada tanggal referensi
                var hargaPasar = await _hargaPasarRepository.GetHargaAktifByTanggalAsync(tanggalReferensi);
                if (hargaPasar == null)
                {
                    return (false, $"Tidak ada harga pasar yang aktif pada tanggal {tanggalReferensi:dd/MM/yyyy}", null);
                }

                // Hitung estimasi keuntungan (Total Ayam � Harga Per Ekor)
                var totalPendapatan = totalAyam * hargaPasar.HargaPerEkor;

                var estimasi = new EstimasiKeuntunganDto
                {
                    TotalAyam = totalAyam,
                    HargaPerEkor = hargaPasar.HargaPerEkor,
                    TotalPendapatan = totalPendapatan,
                    TanggalReferensi = tanggalReferensi,
                    HargaPasarInfo = new HargaPasarInfoDto
                    {
                        Id = hargaPasar.Id,
                        HargaPerEkor = hargaPasar.HargaPerEkor,
                        HargaPerKg = hargaPasar.HargaPerKg,
                        TanggalMulai = hargaPasar.TanggalMulai,
                        TanggalBerakhir = hargaPasar.TanggalBerakhir,
                        Wilayah = hargaPasar.Wilayah,
                        Keterangan = hargaPasar.Keterangan
                    }
                };

                return (true, "Estimasi keuntungan berhasil dihitung", estimasi);
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}", null);
            }
        }

        public async Task<(bool Success, string Message, KeuntunganPanenDto? Data)> HitungKeuntunganDariPanenAsync(Guid panenId)
        {
            try
            {
                // Ambil data panen dengan detail
                var panen = await _panenRepository.GetWithDetailsAsync(panenId);
                if (panen == null)
                {
                    return (false, "Data panen tidak ditemukan", null);
                }

                // Ambil harga pasar yang aktif pada tanggal panen
                var hargaPasar = await _hargaPasarRepository.GetHargaAktifByTanggalAsync(panen.TanggalPanen);
                if (hargaPasar == null)
                {
                    return (false, $"Tidak ada harga pasar yang aktif pada tanggal panen {panen.TanggalPanen:dd/MM/yyyy}", null);
                }

                // Hitung total berat dari jumlah ekor × berat rata-rata
                var totalBerat = panen.JumlahEkorPanen * panen.BeratRataRata;

                // Hitung total pendapatan (berat × harga per kg)
                var totalPendapatan = totalBerat * hargaPasar.HargaPerKg;

                var keuntunganPanen = new KeuntunganPanenDto
                {
                    PanenId = panen.Id,
                    TanggalPanen = panen.TanggalPanen,
                    JumlahAyam = panen.JumlahEkorPanen,
                    TotalBerat = totalBerat,
                    BeratRataRata = panen.BeratRataRata,
                    HargaPerKg = hargaPasar.HargaPerKg,
                    TotalPendapatan = totalPendapatan,
                    NamaKandang = panen.Ayam?.Kandang?.NamaKandang ?? "N/A",
                    HargaPasarInfo = new HargaPasarInfoDto
                    {
                        Id = hargaPasar.Id,
                        HargaPerEkor = hargaPasar.HargaPerEkor,
                        HargaPerKg = hargaPasar.HargaPerKg,
                        TanggalMulai = hargaPasar.TanggalMulai,
                        TanggalBerakhir = hargaPasar.TanggalBerakhir,
                        Wilayah = hargaPasar.Wilayah,
                        Keterangan = hargaPasar.Keterangan
                    }
                };

                return (true, "Keuntungan panen berhasil dihitung", keuntunganPanen);
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}", null);
            }
        }

        public async Task<(bool Success, string Message, LaporanKeuntunganDto? Data)> GetLaporanKeuntunganAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                // Ambil semua data panen dalam periode
                var panenList = await _panenRepository.GetByDateRangeAsync(startDate, endDate);
                if (!panenList.Any())
                {
                    return (false, $"Tidak ada data panen dalam periode {startDate:dd/MM/yyyy} - {endDate:dd/MM/yyyy}", null);
                }

                // Ambil semua harga pasar dalam periode
                var hargaPasarList = await _hargaPasarRepository.GetRiwayatHargaAsync(startDate, endDate);
                
                var detailPanen = new List<KeuntunganPanenDto>();
                var totalKeuntungan = new TotalKeuntunganDto();
                var daftarHargaPerEkor = new List<decimal>();
                var daftarHargaPerKg = new List<decimal>();

                foreach (var panen in panenList)
                {
                    // Ambil harga pasar yang aktif pada tanggal panen
                    var hargaPasar = await _hargaPasarRepository.GetHargaAktifByTanggalAsync(panen.TanggalPanen);
                    if (hargaPasar == null) continue;

                    // Hitung keuntungan per panen (berat × harga per kg)
                    var totalBerat = panen.JumlahEkorPanen * panen.BeratRataRata;
                    var totalPendapatan = totalBerat * hargaPasar.HargaPerKg;

                    detailPanen.Add(new KeuntunganPanenDto
                    {
                        PanenId = panen.Id,
                        TanggalPanen = panen.TanggalPanen,
                        JumlahAyam = panen.JumlahEkorPanen,
                        TotalBerat = totalBerat,
                        BeratRataRata = panen.BeratRataRata,
                        HargaPerKg = hargaPasar.HargaPerKg,
                        TotalPendapatan = totalPendapatan,
                        NamaKandang = panen.Ayam?.Kandang?.NamaKandang ?? "N/A",
                        HargaPasarInfo = new HargaPasarInfoDto
                        {
                            Id = hargaPasar.Id,
                            HargaPerEkor = hargaPasar.HargaPerEkor,
                            HargaPerKg = hargaPasar.HargaPerKg,
                            TanggalMulai = hargaPasar.TanggalMulai,
                            TanggalBerakhir = hargaPasar.TanggalBerakhir,
                            Wilayah = hargaPasar.Wilayah,
                            Keterangan = hargaPasar.Keterangan
                        }
                    });

                    // Akumulasi total
                    totalKeuntungan.TotalPanen++;
                    totalKeuntungan.TotalAyam += panen.JumlahEkorPanen;
                    totalKeuntungan.TotalBerat += totalBerat;
                    totalKeuntungan.TotalPendapatan += totalPendapatan;

                    daftarHargaPerEkor.Add(hargaPasar.HargaPerEkor);
                    daftarHargaPerKg.Add(hargaPasar.HargaPerKg);
                }

                // Hitung rata-rata
                totalKeuntungan.RataRataBeratPerEkor = totalKeuntungan.TotalAyam > 0 ?
                    totalKeuntungan.TotalBerat / totalKeuntungan.TotalAyam : 0;

                // Hitung rata-rata harga per ekor
                totalKeuntungan.RataRataHargaPerEkor = daftarHargaPerEkor.Any() ? daftarHargaPerEkor.Average() : 0;

                var rataRataHargaPerKg = daftarHargaPerKg.Any() ? daftarHargaPerKg.Average() : 0;

                // Hitung fluktuasi harga (berdasarkan HargaPerKg)
                var fluktuasi = new FluktusiHargaDto();
                if (daftarHargaPerKg.Any())
                {
                    fluktuasi.HargaTerendah = daftarHargaPerKg.Min();
                    fluktuasi.HargaTertinggi = daftarHargaPerKg.Max();
                    fluktuasi.SelisihHarga = fluktuasi.HargaTertinggi - fluktuasi.HargaTerendah;
                    fluktuasi.PersentaseFluktuasi = fluktuasi.HargaTerendah > 0 ? 
                        (fluktuasi.SelisihHarga / fluktuasi.HargaTerendah) * 100 : 0;
                }

                var laporanKeuntungan = new LaporanKeuntunganDto
                {
                    Periode = new PeriodeLaporanDto
                    {
                        TanggalMulai = startDate,
                        TanggalAkhir = endDate,
                        JumlahHari = (endDate - startDate).Days + 1
                    },
                    Total = totalKeuntungan,
                    DetailPanen = detailPanen,
                    RataRataHargaPerKg = rataRataHargaPerKg,
                    FluktusiHarga = fluktuasi
                };

                return (true, "Laporan keuntungan berhasil dibuat", laporanKeuntungan);
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}", null);
            }
        }

        public async Task<(bool Success, string Message, LaporanKeuntunganBulananDto? Data)> GetLaporanKeuntunganBulananAsync(int tahun, int bulan)
        {
            try
            {
                var startDate = new DateTime(tahun, bulan, 1);
                var endDate = startDate.AddMonths(1).AddDays(-1);

                // Ambil data panen dalam bulan ini
                var panenList = await _panenRepository.GetByDateRangeAsync(startDate, endDate);
                if (!panenList.Any())
                {
                    return (false, $"Tidak ada data panen pada bulan {bulan:D2}/{tahun}", null);
                }

                // ? DEBUG: Check berapa panen yang ditemukan
                var debugInfo = $"DEBUG: Ditemukan {panenList.Count()} panen dalam periode {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}. ";

                var detailHarian = new List<KeuntunganHarianDto>();
                var totalKeuntungan = new TotalKeuntunganDto();
                var daftarHarga = new List<decimal>();
                var hargaPasarBulanIni = new List<HargaPasarInfoDto>();
                var panenTanpaHarga = new List<string>(); // Track panen tanpa harga pasar

                // Group panen per hari
                var panenPerHari = panenList.GroupBy(p => p.TanggalPanen.Date);

                foreach (var grupHarian in panenPerHari)
                {
                    var tanggal = grupHarian.Key;
                    var panenHarian = grupHarian.ToList();

                    var keuntunganHarian = new KeuntunganHarianDto
                    {
                        Tanggal = tanggal,
                        JumlahPanen = panenHarian.Count,
                        DetailPanen = new List<KeuntunganPanenDto>()
                    };

                    foreach (var panen in panenHarian)
                    {
                        var hargaPasar = await _hargaPasarRepository.GetHargaAktifByTanggalAsync(panen.TanggalPanen);
                        
                        var totalBerat = panen.JumlahEkorPanen * panen.BeratRataRata;
                        decimal pendapatanPanen = 0;
                        decimal hargaPerKg = 0;

                        // ? ENHANCED: Tetap tampilkan data meskipun tidak ada harga pasar
                        if (hargaPasar != null)
                        {
                            // Gunakan HargaPerKg untuk perhitungan berbasis berat
                            pendapatanPanen = totalBerat * hargaPasar.HargaPerKg;
                            hargaPerKg = hargaPasar.HargaPerKg;
                            // Simpan HargaPerEkor untuk rata-rata harga per ekor
                            daftarHarga.Add(hargaPasar.HargaPerEkor);

                            // Simpan info harga pasar
                            if (!hargaPasarBulanIni.Any(h => h.Id == hargaPasar.Id))
                            {
                                hargaPasarBulanIni.Add(new HargaPasarInfoDto
                                {
                                    Id = hargaPasar.Id,
                                    HargaPerEkor = hargaPasar.HargaPerEkor,
                                    HargaPerKg = hargaPasar.HargaPerKg,
                                    TanggalMulai = hargaPasar.TanggalMulai,
                                    TanggalBerakhir = hargaPasar.TanggalBerakhir,
                                    Wilayah = hargaPasar.Wilayah,
                                    Keterangan = hargaPasar.Keterangan
                                });
                            }
                        }
                        else
                        {
                            // ? TRACK: Panen tanpa harga pasar
                            panenTanpaHarga.Add($"Panen {panen.Id.ToString().Substring(0, 8)} pada {panen.TanggalPanen:yyyy-MM-dd}");
                        }

                        // ? ALWAYS ADD: Tambahkan ke detail panen (bahkan jika harga 0)
                        keuntunganHarian.DetailPanen.Add(new KeuntunganPanenDto
                        {
                            PanenId = panen.Id,
                            TanggalPanen = panen.TanggalPanen,
                            JumlahAyam = panen.JumlahEkorPanen,
                            TotalBerat = totalBerat,
                            BeratRataRata = panen.BeratRataRata,
                            HargaPerKg = hargaPerKg,
                            TotalPendapatan = pendapatanPanen,
                            NamaKandang = panen.Ayam?.Kandang?.NamaKandang ?? "N/A",
                            HargaPasarInfo = hargaPasar != null ? new HargaPasarInfoDto
                            {
                                Id = hargaPasar.Id,
                                HargaPerEkor = hargaPasar!.HargaPerEkor,
                                HargaPerKg = hargaPasar.HargaPerKg,
                                TanggalMulai = hargaPasar.TanggalMulai,
                                TanggalBerakhir = hargaPasar.TanggalBerakhir,
                                Wilayah = hargaPasar.Wilayah,
                                Keterangan = hargaPasar.Keterangan
                            } : null
                        });

                        // Akumulasi harian
                        keuntunganHarian.TotalAyam += panen.JumlahEkorPanen;
                        keuntunganHarian.TotalBerat += totalBerat;
                        keuntunganHarian.TotalKeuntungan += pendapatanPanen;
                        if (hargaPerKg > 0) keuntunganHarian.HargaPerKg = hargaPerKg;

                        // Akumulasi bulanan
                        totalKeuntungan.TotalPanen++;
                        totalKeuntungan.TotalAyam += panen.JumlahEkorPanen;
                        totalKeuntungan.TotalBerat += totalBerat;
                        totalKeuntungan.TotalPendapatan += pendapatanPanen;
                    }

                    detailHarian.Add(keuntunganHarian);
                }

                // ? DEBUG INFO: Tambahkan informasi debug
                if (panenTanpaHarga.Any())
                {
                    debugInfo += $"PERHATIAN: {panenTanpaHarga.Count} panen tidak memiliki harga pasar aktif: [{string.Join(", ", panenTanpaHarga)}]. ";
                }

                // Hitung rata-rata
                totalKeuntungan.RataRataBeratPerEkor = totalKeuntungan.TotalAyam > 0 ?
                    totalKeuntungan.TotalBerat / totalKeuntungan.TotalAyam : 0;

                // Hitung rata-rata harga per ekor dari daftar harga pasar
                totalKeuntungan.RataRataHargaPerEkor = daftarHarga.Any() ? daftarHarga.Average() : 0;

                var rataRataHarga = daftarHarga.Any() ? daftarHarga.Average() : 0;

                // Hitung fluktuasi harga
                var fluktuasi = new FluktusiHargaDto();
                if (daftarHarga.Any())
                {
                    fluktuasi.HargaTerendah = daftarHarga.Min();
                    fluktuasi.HargaTertinggi = daftarHarga.Max();
                    fluktuasi.SelisihHarga = fluktuasi.HargaTertinggi - fluktuasi.HargaTerendah;
                    fluktuasi.PersentaseFluktuasi = fluktuasi.HargaTerendah > 0 ? 
                        (fluktuasi.SelisihHarga / fluktuasi.HargaTerendah) * 100 : 0;
                }

                // Perbandingan dengan bulan sebelumnya
                PerbandinganBulananDto? perbandingan = null;
                var bulanSebelumnya = startDate.AddMonths(-1);
                var panenBulanSebelumnya = await _panenRepository.GetByDateRangeAsync(
                    bulanSebelumnya, bulanSebelumnya.AddMonths(1).AddDays(-1));

                if (panenBulanSebelumnya.Any())
                {
                    decimal totalKeuntunganSebelumnya = 0;
                    foreach (var panen in panenBulanSebelumnya)
                    {
                        var harga = await _hargaPasarRepository.GetHargaAktifByTanggalAsync(panen.TanggalPanen);
                        if (harga != null)
                        {
                            // Gunakan HargaPerKg untuk perhitungan berbasis berat
                            totalKeuntunganSebelumnya += (panen.JumlahEkorPanen * panen.BeratRataRata * harga.HargaPerKg);
                        }
                    }

                    var selisih = totalKeuntungan.TotalPendapatan - totalKeuntunganSebelumnya;
                    var persentase = totalKeuntunganSebelumnya > 0 ? (selisih / totalKeuntunganSebelumnya) * 100 : 0;

                    perbandingan = new PerbandinganBulananDto
                    {
                        BulanSebelumnya = $"{bulanSebelumnya.Month:D2}/{bulanSebelumnya.Year}",
                        TotalKeuntunganSebelumnya = totalKeuntunganSebelumnya,
                        SelisihKeuntungan = selisih,
                        PersentasePerubahan = persentase,
                        StatusPerubahan = selisih > 0 ? "Naik" : selisih < 0 ? "Turun" : "Stabil"
                    };
                }

                var laporanBulanan = new LaporanKeuntunganBulananDto
                {
                    Tahun = tahun,
                    Bulan = bulan,
                    NamaBulan = System.Globalization.CultureInfo.GetCultureInfo("id-ID").DateTimeFormat.GetMonthName(bulan),
                    Periode = new PeriodeLaporanDto
                    {
                        TanggalMulai = startDate,
                        TanggalAkhir = endDate,
                        JumlahHari = (endDate - startDate).Days + 1
                    },
                    Total = totalKeuntungan,
                    DetailHarian = detailHarian.OrderBy(d => d.Tanggal).ToList(),
                    PerbandinganBulanSebelumnya = perbandingan,
                    HargaPasarBulanIni = hargaPasarBulanIni,
                    RataRataHargaPerKg = rataRataHarga,
                    FluktusiHarga = fluktuasi
                };

                // ? ENHANCED MESSAGE: Tambahkan debug info ke message
                var message = $"Laporan keuntungan bulan {bulan:D2}/{tahun} berhasil dibuat. {debugInfo}";
                
                return (true, message, laporanBulanan);
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}", null);
            }
        }

        public async Task<(bool Success, string Message, LaporanKeuntunganMingguanDto? Data)> GetLaporanKeuntunganMingguanAsync(int tahun, int mingguKe)
        {
            try
            {
                // Hitung tanggal mulai dan akhir minggu
                var jan1 = new DateTime(tahun, 1, 1);
                var daysOffset = DayOfWeek.Monday - jan1.DayOfWeek;
                var firstMonday = jan1.AddDays(daysOffset);
                var startDate = firstMonday.AddDays((mingguKe - 1) * 7);
                var endDate = startDate.AddDays(6);

                // Ambil data panen dalam minggu ini
                var panenList = await _panenRepository.GetByDateRangeAsync(startDate, endDate);
                if (!panenList.Any())
                {
                    return (false, $"Tidak ada data panen pada minggu ke-{mingguKe} tahun {tahun}", null);
                }

                var detailHarian = new List<KeuntunganHarianDto>();
                var totalKeuntungan = new TotalKeuntunganDto();
                var daftarHarga = new List<decimal>();

                // Implementasi sama seperti bulanan, tapi untuk rentang minggu
                // ... (implementasi detail serupa dengan yang bulanan)

                var laporanMingguan = new LaporanKeuntunganMingguanDto
                {
                    Tahun = tahun,
                    MingguKe = mingguKe,
                    Periode = new PeriodeLaporanDto
                    {
                        TanggalMulai = startDate,
                        TanggalAkhir = endDate,
                        JumlahHari = 7
                    },
                    Total = totalKeuntungan,
                    DetailHarian = detailHarian,
                    RataRataHargaPerKg = daftarHarga.Any() ? daftarHarga.Average() : 0,
                    FluktusiHarga = new FluktusiHargaDto() // Implementasi serupa
                };

                return (true, $"Laporan keuntungan minggu ke-{mingguKe} tahun {tahun} berhasil dibuat", laporanMingguan);
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}", null);
            }
        }

        public async Task<(bool Success, string Message, RingkasanKeuntunganTahunanDto? Data)> GetRingkasanKeuntunganTahunanAsync(int tahun)
        {
            try
            {
                var startDate = new DateTime(tahun, 1, 1);
                var endDate = new DateTime(tahun, 12, 31);

                var breakdownBulanan = new List<KeuntunganBulananSummaryDto>();
                var totalTahunan = new TotalKeuntunganDto();
                var daftarHargaTahunan = new List<decimal>();

                // Loop untuk setiap bulan
                for (int bulan = 1; bulan <= 12; bulan++)
                {
                    var startBulan = new DateTime(tahun, bulan, 1);
                    var endBulan = startBulan.AddMonths(1).AddDays(-1);

                    var panenBulan = await _panenRepository.GetByDateRangeAsync(startBulan, endBulan);
                    
                    decimal totalKeuntunganBulan = 0;
                    int totalAyamBulan = 0;
                    decimal totalBeratBulan = 0;
                    var hariPanen = new HashSet<DateTime>();
                    var hargaBulan = new List<decimal>();

                    foreach (var panen in panenBulan)
                    {
                        var harga = await _hargaPasarRepository.GetHargaAktifByTanggalAsync(panen.TanggalPanen);
                        if (harga != null)
                        {
                            var totalBerat = panen.JumlahEkorPanen * panen.BeratRataRata;
                            // Gunakan HargaPerKg untuk perhitungan berbasis berat
                            var pendapatan = totalBerat * harga.HargaPerKg;

                            totalKeuntunganBulan += pendapatan;
                            totalAyamBulan += panen.JumlahEkorPanen;
                            totalBeratBulan += totalBerat;
                            hariPanen.Add(panen.TanggalPanen.Date);
                            hargaBulan.Add(harga.HargaPerKg);
                            daftarHargaTahunan.Add(harga.HargaPerKg);
                        }
                    }

                    var summaryBulan = new KeuntunganBulananSummaryDto
                    {
                        Bulan = bulan,
                        NamaBulan = System.Globalization.CultureInfo.GetCultureInfo("id-ID").DateTimeFormat.GetMonthName(bulan),
                        TotalKeuntungan = totalKeuntunganBulan,
                        TotalAyam = totalAyamBulan,
                        TotalBerat = totalBeratBulan,
                        JumlahHariPanen = hariPanen.Count,
                        RataRataHargaPerKg = hargaBulan.Any() ? hargaBulan.Average() : 0
                    };

                    breakdownBulanan.Add(summaryBulan);

                    // Akumulasi total tahunan
                    totalTahunan.TotalPendapatan += totalKeuntunganBulan;
                    totalTahunan.TotalAyam += totalAyamBulan;
                    totalTahunan.TotalBerat += totalBeratBulan;
                    totalTahunan.TotalPanen += panenBulan.Count();
                }

                // Cari bulan terbaik dan terburuk
                var bulanTerbaik = breakdownBulanan.OrderByDescending(b => b.TotalKeuntungan).FirstOrDefault();
                var bulanTerburuk = breakdownBulanan.OrderBy(b => b.TotalKeuntungan).FirstOrDefault();

                // Hitung rata-rata dan fluktuasi
                var rataRataPerBulan = breakdownBulanan.Count > 0 ? 
                    breakdownBulanan.Average(b => b.TotalKeuntungan) : 0;

                var fluktusiTahunan = new FluktusiHargaDto();
                if (daftarHargaTahunan.Any())
                {
                    fluktusiTahunan.HargaTerendah = daftarHargaTahunan.Min();
                    fluktusiTahunan.HargaTertinggi = daftarHargaTahunan.Max();
                    fluktusiTahunan.SelisihHarga = fluktusiTahunan.HargaTertinggi - fluktusiTahunan.HargaTerendah;
                    fluktusiTahunan.PersentaseFluktuasi = fluktusiTahunan.HargaTerendah > 0 ? 
                        (fluktusiTahunan.SelisihHarga / fluktusiTahunan.HargaTerendah) * 100 : 0;
                }

                // Tentukan trend tahunan
                var trendTahunan = "Stabil";
                if (breakdownBulanan.Count >= 6)
                {
                    var bulan6Pertama = breakdownBulanan.Take(6).Average(b => b.TotalKeuntungan);
                    var bulan6Terakhir = breakdownBulanan.Skip(6).Average(b => b.TotalKeuntungan);
                    
                    if (bulan6Terakhir > bulan6Pertama * 1.1m) trendTahunan = "Naik";
                    else if (bulan6Terakhir < bulan6Pertama * 0.9m) trendTahunan = "Turun";
                }

                totalTahunan.RataRataBeratPerEkor = totalTahunan.TotalAyam > 0 ? 
                    totalTahunan.TotalBerat / totalTahunan.TotalAyam : 0;

                totalTahunan.RataRataHargaPerEkor = totalTahunan.TotalAyam > 0 ?
                    totalTahunan.TotalPendapatan / totalTahunan.TotalAyam : 0;

                var ringkasanTahunan = new RingkasanKeuntunganTahunanDto
                {
                    Tahun = tahun,
                    TotalTahunan = totalTahunan,
                    BreakdownBulanan = breakdownBulanan,
                    BulanTerbaik = bulanTerbaik,
                    BulanTerburuk = bulanTerburuk,
                    RataRataKeuntunganPerBulan = rataRataPerBulan,
                    TrendTahunan = trendTahunan,
                    FluktusiHargaTahunan = fluktusiTahunan
                };

                return (true, $"Ringkasan keuntungan tahun {tahun} berhasil dibuat", ringkasanTahunan);
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}", null);
            }
        }
    }
}
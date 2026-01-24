using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIMTernakAyam.DTOs.HargaPasar;
using SIMTernakAyam.Models;
using SIMTernakAyam.Services.Interfaces;

namespace SIMTernakAyam.Controllers
{
    /// <summary>
    /// Controller untuk manajemen harga pasar ayam
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class HargaPasarController : BaseController
    {
        private readonly IHargaPasarService _hargaPasarService;

        public HargaPasarController(IHargaPasarService hargaPasarService)
        {
            _hargaPasarService = hargaPasarService ?? throw new ArgumentNullException(nameof(hargaPasarService));
        }

        /// <summary>
        /// Mendapatkan semua data harga pasar
        /// </summary>
        /// <returns>List harga pasar</returns>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var hargaPasarList = await _hargaPasarService.GetAllAsync();
                var responseDtos = HargaPasarResponseDto.FromEntities(hargaPasarList);
                return Success(responseDtos, "Data harga pasar berhasil diambil");
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Mendapatkan harga pasar berdasarkan ID
        /// </summary>
        /// <param name="id">ID harga pasar</param>
        /// <returns>Detail harga pasar</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var hargaPasar = await _hargaPasarService.GetByIdAsync(id);
                if (hargaPasar == null)
                {
                    return NotFound("Harga pasar tidak ditemukan");
                }

                var responseDto = HargaPasarResponseDto.FromEntity(hargaPasar);
                return Success(responseDto, "Data harga pasar berhasil diambil");
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Mendapatkan harga pasar terbaru yang aktif
        /// </summary>
        /// <returns>Harga pasar terbaru</returns>
        [HttpGet("terbaru")]
        public async Task<IActionResult> GetHargaTerbaru()
        {
            try
            {
                var hargaPasar = await _hargaPasarService.GetHargaTerbaruAsync();
                if (hargaPasar == null)
                {
                    return NotFound("Tidak ada harga pasar yang aktif");
                }

                var responseDto = HargaPasarResponseDto.FromEntity(hargaPasar);
                return Success(responseDto, "Harga pasar terbaru berhasil diambil");
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Mendapatkan harga pasar berdasarkan tanggal
        /// </summary>
        /// <param name="tanggal">Tanggal referensi (format: yyyy-MM-dd)</param>
        /// <returns>Harga pasar yang berlaku pada tanggal tersebut</returns>
        [HttpGet("by-tanggal")]
        public async Task<IActionResult> GetHargaByTanggal([FromQuery] DateTime tanggal)
        {
            try
            {
                var hargaPasar = await _hargaPasarService.GetHargaAktifByTanggalAsync(tanggal);
                if (hargaPasar == null)
                {
                    return NotFound($"Tidak ada harga pasar yang berlaku pada tanggal {tanggal:dd/MM/yyyy}");
                }

                var responseDto = HargaPasarResponseDto.FromEntity(hargaPasar);
                return Success(responseDto, $"Harga pasar untuk tanggal {tanggal:dd/MM/yyyy} berhasil diambil");
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Mendapatkan riwayat harga dalam rentang waktu tertentu
        /// </summary>
        /// <param name="startDate">Tanggal mulai</param>
        /// <param name="endDate">Tanggal akhir</param>
        /// <returns>List riwayat harga pasar</returns>
        [HttpGet("riwayat")]
        public async Task<IActionResult> GetRiwayatHarga([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                if (startDate > endDate)
                {
                    return Error("Tanggal mulai tidak boleh lebih besar dari tanggal akhir");
                }

                var hargaPasarList = await _hargaPasarService.GetRiwayatHargaAsync(startDate, endDate);
                var responseDtos = HargaPasarResponseDto.FromEntities(hargaPasarList);
                return Success(responseDtos, $"Riwayat harga pasar dari {startDate:dd/MM/yyyy} sampai {endDate:dd/MM/yyyy} berhasil diambil");
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Membuat harga pasar baru
        /// </summary>
        /// <param name="createDto">Data harga pasar baru</param>
        /// <returns>Harga pasar yang dibuat</returns>
        [HttpPost]
        [Authorize(Roles = "Admin,Manager,Operator,Pemilik")]
        public async Task<IActionResult> Create([FromBody] CreateHargaPasarDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return ValidationError(ModelState);
                }

                // Validasi business logic
                if (createDto.HargaPerEkor <= 0)
                {
                    return Error("Harga per ekor harus lebih besar dari 0");
                }

                if (createDto.HargaPerEkor < 10000 || createDto.HargaPerEkor > 100000)
                {
                    return Error("Harga per ekor harus antara Rp 10.000 - Rp 100.000");
                }

                if (createDto.TanggalBerakhir.HasValue && createDto.TanggalBerakhir <= createDto.TanggalMulai)
                {
                    return Error("Tanggal berakhir harus lebih besar dari tanggal mulai");
                }

                // Auto deactivate previous prices if requested
                if (createDto.AutoDeactivatePrevious)
                {
                    await _hargaPasarService.DeactivateAllHargaAsync();
                }

                var hargaPasar = new HargaPasar
                {
                    HargaPerEkor = createDto.HargaPerEkor,
                    TanggalMulai = createDto.TanggalMulai,
                    TanggalBerakhir = createDto.TanggalBerakhir,
                    Keterangan = createDto.Keterangan,
                    Wilayah = createDto.Wilayah,
                    IsAktif = true
                };

                var result = await _hargaPasarService.CreateAsync(hargaPasar);
                if (!result.Success)
                {
                    return Error(result.Message);
                }

                var responseDto = HargaPasarResponseDto.FromEntity(result.Data!);
                return Created(responseDto, "Harga pasar berhasil dibuat");
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Mengupdate harga pasar
        /// </summary>
        /// <param name="id">ID harga pasar</param>
        /// <param name="updateDto">Data update harga pasar</param>
        /// <returns>Hasil update</returns>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager,Operator,Pemilik")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateHargaPasarDto updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return ValidationError(ModelState);
                }

                var existingHarga = await _hargaPasarService.GetByIdAsync(id);
                if (existingHarga == null)
                {
                    return NotFound("Harga pasar tidak ditemukan");
                }

                // Validasi business logic
                if (updateDto.HargaPerEkor <= 0)
                {
                    return Error("Harga per ekor harus lebih besar dari 0");
                }

                if (updateDto.HargaPerEkor < 10000 || updateDto.HargaPerEkor > 100000)
                {
                    return Error("Harga per ekor harus antara Rp 10.000 - Rp 100.000");
                }

                if (updateDto.TanggalBerakhir.HasValue && updateDto.TanggalBerakhir <= updateDto.TanggalMulai)
                {
                    return Error("Tanggal berakhir harus lebih besar dari tanggal mulai");
                }

                existingHarga.HargaPerEkor = updateDto.HargaPerEkor;
                existingHarga.TanggalMulai = updateDto.TanggalMulai;
                existingHarga.TanggalBerakhir = updateDto.TanggalBerakhir;
                existingHarga.Keterangan = updateDto.Keterangan;
                existingHarga.Wilayah = updateDto.Wilayah;
                existingHarga.IsAktif = updateDto.IsAktif;

                var result = await _hargaPasarService.UpdateAsync(existingHarga);
                if (!result.Success)
                {
                    return Error(result.Message);
                }

                return Success<object?>(null, "Harga pasar berhasil diupdate");
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Mengupdate status aktif harga pasar
        /// </summary>
        /// <param name="id">ID harga pasar</param>
        /// <param name="isAktif">Status aktif baru</param>
        /// <returns>Hasil update status</returns>
        [HttpPatch("{id}/status")]
        [Authorize(Roles = "Admin,Manager,Operator,Pemilik")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromQuery] bool isAktif)
        {
            try
            {
                var result = await _hargaPasarService.UpdateStatusHargaAsync(id, isAktif);
                if (!result.Success)
                {
                    return Error(result.Message);
                }

                return Success<object?>(null, result.Message);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Menonaktifkan semua harga pasar yang aktif
        /// </summary>
        /// <returns>Hasil operasi</returns>
        [HttpPost("deactivate-all")]
        [Authorize(Roles = "Admin,Manager,Operator,Pemilik")]
        public async Task<IActionResult> DeactivateAll()
        {
            try
            {
                var result = await _hargaPasarService.DeactivateAllHargaAsync();
                if (!result.Success)
                {
                    return Error(result.Message);
                }

                return Success<object?>(null, result.Message);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Menghapus harga pasar
        /// </summary>
        /// <param name="id">ID harga pasar</param>
        /// <returns>Hasil penghapusan</returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var result = await _hargaPasarService.DeleteAsync(id);
                if (!result.Success)
                {
                    return Error(result.Message);
                }

                return Success<object?>(null, "Harga pasar berhasil dihapus");
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Menghitung estimasi keuntungan berdasarkan harga pasar aktif
        /// </summary>
        /// <param name="totalAyam">Total jumlah ayam yang dipanen</param>
        /// <param name="tanggalPanen">Tanggal panen (opsional, default hari ini)</param>
        /// <returns>Estimasi keuntungan</returns>
        [HttpGet("estimasi-keuntungan")]
        public async Task<IActionResult> HitungKeuntungan(
            [FromQuery] int totalAyam,
            [FromQuery] DateTime? tanggalReferensi = null)
        {
            try
            {
                if (totalAyam <= 0)
                {
                    return Error("Total ayam harus lebih besar dari 0");
                }

                var tanggalRef = tanggalReferensi ?? DateTime.Now;
                var result = await _hargaPasarService.HitungKeuntunganAsync(totalAyam, tanggalRef);
                
                if (!result.Success)
                {
                    return Error(result.Message);
                }

                return Success(result.Data, "Estimasi keuntungan berhasil dihitung");
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Menghitung keuntungan berdasarkan data panen yang sudah ada
        /// </summary>
        /// <param name="panenId">ID data panen</param>
        /// <returns>Estimasi keuntungan berdasarkan data panen</returns>
        [HttpGet("keuntungan-panen/{panenId}")]
        public async Task<IActionResult> HitungKeuntunganDariPanen(Guid panenId)
        {
            try
            {
                var result = await _hargaPasarService.HitungKeuntunganDariPanenAsync(panenId);
                
                if (!result.Success)
                {
                    return Error(result.Message);
                }

                return Success(result.Data, "Keuntungan berdasarkan data panen berhasil dihitung");
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Mendapatkan laporan keuntungan dalam periode tertentu
        /// </summary>
        /// <param name="startDate">Tanggal mulai</param>
        /// <param name="endDate">Tanggal akhir</param>
        /// <returns>Laporan keuntungan periode</returns>
        [HttpGet("laporan-keuntungan")]
        public async Task<IActionResult> GetLaporanKeuntungan(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            try
            {
                if (startDate > endDate)
                {
                    return Error("Tanggal mulai tidak boleh lebih besar dari tanggal akhir");
                }

                var result = await _hargaPasarService.GetLaporanKeuntunganAsync(startDate, endDate);
                
                if (!result.Success)
                {
                    return Error(result.Message);
                }

                return Success(result.Data, $"Laporan keuntungan periode {startDate:dd/MM/yyyy} - {endDate:dd/MM/yyyy} berhasil diambil");
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Mendapatkan laporan keuntungan bulanan
        /// </summary>
        /// <param name="tahun">Tahun (default: tahun sekarang)</param>
        /// <param name="bulan">Bulan (1-12, default: bulan sekarang)</param>
        /// <returns>Laporan keuntungan bulanan</returns>
        [HttpGet("laporan-keuntungan-bulanan")]
        public async Task<IActionResult> GetLaporanKeuntunganBulanan(
            [FromQuery] int? tahun = null,
            [FromQuery] int? bulan = null)
        {
            try
            {
                var targetYear = tahun ?? DateTime.Now.Year;
                var targetMonth = bulan ?? DateTime.Now.Month;

                if (targetMonth < 1 || targetMonth > 12)
                {
                    return Error("Bulan harus antara 1-12");
                }

                var startDate = new DateTime(targetYear, targetMonth, 1);
                var endDate = startDate.AddMonths(1).AddDays(-1);

                var result = await _hargaPasarService.GetLaporanKeuntunganBulananAsync(targetYear, targetMonth);
                
                if (!result.Success)
                {
                    return Error(result.Message);
                }

                return Success(result.Data, $"Laporan keuntungan bulan {targetMonth:D2}/{targetYear} berhasil diambil");
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Mendapatkan laporan keuntungan mingguan
        /// </summary>
        /// <param name="tahun">Tahun (default: tahun sekarang)</param>
        /// <param name="mingguKe">Minggu ke- dalam tahun (1-53, default: minggu sekarang)</param>
        /// <returns>Laporan keuntungan mingguan</returns>
        [HttpGet("laporan-keuntungan-mingguan")]
        public async Task<IActionResult> GetLaporanKeuntunganMingguan(
            [FromQuery] int? tahun = null,
            [FromQuery] int? mingguKe = null)
        {
            try
            {
                var targetYear = tahun ?? DateTime.Now.Year;
                var targetWeek = mingguKe ?? GetWeekOfYear(DateTime.Now);

                if (targetWeek < 1 || targetWeek > 53)
                {
                    return Error("Minggu harus antara 1-53");
                }

                var result = await _hargaPasarService.GetLaporanKeuntunganMingguanAsync(targetYear, targetWeek);
                
                if (!result.Success)
                {
                    return Error(result.Message);
                }

                return Success(result.Data, $"Laporan keuntungan minggu ke-{targetWeek} tahun {targetYear} berhasil diambil");
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Mendapatkan ringkasan keuntungan tahunan dengan breakdown per bulan
        /// </summary>
        /// <param name="tahun">Tahun (default: tahun sekarang)</param>
        /// <returns>Ringkasan keuntungan tahunan</returns>
        [HttpGet("ringkasan-keuntungan-tahunan")]
        public async Task<IActionResult> GetRingkasanKeuntunganTahunan([FromQuery] int? tahun = null)
        {
            try
            {
                var targetYear = tahun ?? DateTime.Now.Year;
                var result = await _hargaPasarService.GetRingkasanKeuntunganTahunanAsync(targetYear);
                
                if (!result.Success)
                {
                    return Error(result.Message);
                }

                return Success(result.Data, $"Ringkasan keuntungan tahun {targetYear} berhasil diambil");
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Helper method untuk mendapatkan minggu ke berapa dalam tahun
        /// </summary>
        private int GetWeekOfYear(DateTime date)
        {
            var cal = System.Globalization.CultureInfo.CurrentCulture.Calendar;
            return cal.GetWeekOfYear(date, System.Globalization.CalendarWeekRule.FirstDay, DayOfWeek.Monday);
        }

        /// <summary>
        /// ?? DEBUG: Cek harga pasar yang tersedia untuk tanggal tertentu
        /// </summary>
        /// <param name="tanggal">Tanggal yang ingin dicek</param>
        /// <returns>Debug info harga pasar</returns>
        [HttpGet("debug/cek-harga-pasar")]
        public async Task<IActionResult> DebugCekHargaPasar([FromQuery] DateTime tanggal)
        {
            try
            {
                // Cek semua harga pasar yang ada
                var semuaHarga = await _hargaPasarService.GetAllAsync();
                
                // Cek harga pasar yang aktif pada tanggal tertentu
                var hargaAktif = await _hargaPasarService.GetHargaAktifByTanggalAsync(tanggal);

                // Cek harga pasar terbaru
                var hargaTerbaru = await _hargaPasarService.GetHargaTerbaruAsync();

                var debugInfo = new
                {
                    TanggalYangDicek = tanggal.ToString("yyyy-MM-dd"),
                    TotalHargaPasarDiDatabase = semuaHarga.Count(),
                    HargaPasarAktifPadaTanggal = hargaAktif != null ? new
                    {
                        Id = hargaAktif.Id,
                        HargaPerKg = hargaAktif.HargaPerEkor,
                        TanggalMulai = hargaAktif.TanggalMulai,
                        TanggalBerakhir = hargaAktif.TanggalBerakhir,
                        IsAktif = hargaAktif.IsAktif,
                        Wilayah = hargaAktif.Wilayah,
                        Keterangan = hargaAktif.Keterangan
                    } : null,
                    HargaPasarTerbaru = hargaTerbaru != null ? new
                    {
                        Id = hargaTerbaru.Id,
                        HargaPerKg = hargaTerbaru.HargaPerEkor,
                        TanggalMulai = hargaTerbaru.TanggalMulai,
                        TanggalBerakhir = hargaTerbaru.TanggalBerakhir,
                        IsAktif = hargaTerbaru.IsAktif,
                        Wilayah = hargaTerbaru.Wilayah,
                        Keterangan = hargaTerbaru.Keterangan
                    } : null,
                    SemuaHargaPasar = semuaHarga.Select(h => new
                    {
                        Id = h.Id.ToString().Substring(0, 8) + "...",
                        HargaPerKg = h.HargaPerEkor,
                        TanggalMulai = h.TanggalMulai.ToString("yyyy-MM-dd"),
                        TanggalBerakhir = h.TanggalBerakhir?.ToString("yyyy-MM-dd") ?? "null",
                        IsAktif = h.IsAktif,
                        Wilayah = h.Wilayah ?? "null",
                        Keterangan = h.Keterangan ?? "null"
                    }).ToList(),
                    RekomendasiSolusi = hargaAktif == null ? 
                        "? MASALAH: Tidak ada harga pasar aktif pada tanggal ini. Buat harga pasar baru atau aktifkan yang sudah ada." :
                        "? OK: Ada harga pasar aktif pada tanggal ini."
                };

                return Success(debugInfo, "Debug info harga pasar berhasil diambil");
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        /// <summary>
        /// ?? DEBUG: Cek data panen untuk periode tertentu
        /// </summary>
        /// <param name="startDate">Tanggal mulai</param>
        /// <param name="endDate">Tanggal akhir</param>
        /// <returns>Debug info data panen</returns>
        [HttpGet("debug/cek-data-panen")]
        public async Task<IActionResult> DebugCekDataPanen([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var result = await _hargaPasarService.GetLaporanKeuntunganAsync(startDate, endDate);
                
                var debugInfo = new
                {
                    PeriodeCek = $"{startDate:yyyy-MM-dd} sampai {endDate:yyyy-MM-dd}",
                    HasilLaporan = result.Success,
                    Message = result.Message,
                    JumlahDataPanen = result.Data?.DetailPanen?.Count ?? 0,
                    DetailPanen = result.Data?.DetailPanen?.Select(p => new
                    {
                        PanenId = p.PanenId.ToString().Substring(0, 8) + "...",
                        TanggalPanen = p.TanggalPanen.ToString("yyyy-MM-dd"),
                        JumlahAyam = p.JumlahAyam,
                        TotalBerat = p.TotalBerat,
                        HargaPerKg = p.HargaPerKg,
                        TotalPendapatan = p.TotalPendapatan,
                        NamaKandang = p.NamaKandang,
                        AdaHargaPasar = p.HargaPasarInfo != null
                    }).ToList()
                };

                return Success(debugInfo, "Debug info data panen berhasil diambil");
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }
    }
}
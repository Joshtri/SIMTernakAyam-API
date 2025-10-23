using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIMTernakAyam.DTOs.Laporan;
using SIMTernakAyam.Enums;
using SIMTernakAyam.Services.Interfaces;
using System.Security.Claims;

namespace SIMTernakAyam.Controllers
{
    [ApiController]
    [Route("api/laporan")]
    [Authorize]
    public class LaporanController : BaseController
    {
        private readonly ILaporanService _laporanService;

        public LaporanController(ILaporanService laporanService)
        {
            _laporanService = laporanService;
        }

        #region Helper Methods

        /// <summary>
        /// Cek apakah user memiliki role Operator atau Pemilik
        /// </summary>
        private bool IsOperatorOrPemilik()
        {
            var roleClaim = User.FindFirst(ClaimTypes.Role);
            if (roleClaim == null || !Enum.TryParse<RoleEnum>(roleClaim.Value, out var role))
            {
                return false;
            }

            return role == RoleEnum.Operator || role == RoleEnum.Pemilik;
        }

        #endregion

        #region Kesehatan Ayam Endpoints

        /// <summary>
        /// Mendapatkan data kesehatan ayam per kandang
        /// Hanya untuk Operator dan Pemilik
        /// </summary>
        [HttpGet("kesehatan-ayam")]
        [ProducesResponseType(typeof(Common.ApiResponse<List<KesehatanKandangDto>>), 200)]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 403)]
        public async Task<IActionResult> GetKesehatanAyam()
        {
            try
            {
                if (!IsOperatorOrPemilik())
                {
                    return Forbidden("Akses ditolak. Fitur ini hanya untuk Operator dan Pemilik.");
                }

                var result = await _laporanService.GetKesehatanKandangAsync();
                return Success(result, "Berhasil mengambil data kesehatan ayam.");
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Mendapatkan data kesehatan ayam untuk kandang tertentu
        /// Hanya untuk Operator dan Pemilik
        /// </summary>
        [HttpGet("kesehatan-ayam/{kandangId}")]
        [ProducesResponseType(typeof(Common.ApiResponse<KesehatanKandangDto>), 200)]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 403)]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 404)]
        public async Task<IActionResult> GetKesehatanAyamByKandang(Guid kandangId)
        {
            try
            {
                if (!IsOperatorOrPemilik())
                {
                    return Forbidden("Akses ditolak. Fitur ini hanya untuk Operator dan Pemilik.");
                }

                var result = await _laporanService.GetKesehatanKandangByIdAsync(kandangId);
                if (result == null)
                {
                    return NotFound("Kandang tidak ditemukan.");
                }

                return Success(result, "Berhasil mengambil data kesehatan ayam untuk kandang.");
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        #endregion

        #region Laporan Operasional Endpoints

        /// <summary>
        /// Mendapatkan laporan operasional berdasarkan periode
        /// Hanya untuk Operator dan Pemilik
        /// </summary>
        /// <param name="startDate">Tanggal mulai (format: yyyy-MM-dd)</param>
        /// <param name="endDate">Tanggal selesai (format: yyyy-MM-dd)</param>
        /// <param name="preset">Preset periode: thisWeek, lastWeek, thisMonth, lastMonth</param>
        [HttpGet("operasional")]
        [ProducesResponseType(typeof(Common.ApiResponse<LaporanOperasionalDto>), 200)]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 403)]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 400)]
        public async Task<IActionResult> GetLaporanOperasional(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string? preset = null)
        {
            try
            {
                if (!IsOperatorOrPemilik())
                {
                    return Forbidden("Akses ditolak. Fitur ini hanya untuk Operator dan Pemilik.");
                }

                DateTime start, end;

                // Jika ada preset, gunakan preset
                if (!string.IsNullOrWhiteSpace(preset))
                {
                    var now = DateTime.Now;
                    switch (preset.ToLower())
                    {
                        case "thisweek":
                            start = now.AddDays(-(int)now.DayOfWeek).Date;
                            end = start.AddDays(6).Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                            break;
                        case "lastweek":
                            start = now.AddDays(-(int)now.DayOfWeek - 7).Date;
                            end = start.AddDays(6).Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                            break;
                        case "thismonth":
                            start = new DateTime(now.Year, now.Month, 1);
                            end = start.AddMonths(1).AddDays(-1).Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                            break;
                        case "lastmonth":
                            start = new DateTime(now.Year, now.Month, 1).AddMonths(-1);
                            end = start.AddMonths(1).AddDays(-1).Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                            break;
                        default:
                            return Error("Preset tidak valid. Gunakan: thisWeek, lastWeek, thisMonth, lastMonth");
                    }
                }
                // Jika tidak ada preset, gunakan startDate dan endDate
                else if (startDate.HasValue && endDate.HasValue)
                {
                    start = startDate.Value;
                    end = endDate.Value;

                    if (start > end)
                    {
                        return Error("Tanggal mulai tidak boleh lebih besar dari tanggal selesai.");
                    }
                }
                // Default: bulan ini
                else
                {
                    var now = DateTime.Now;
                    start = new DateTime(now.Year, now.Month, 1);
                    end = start.AddMonths(1).AddDays(-1).Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                }

                var result = await _laporanService.GetLaporanOperasionalAsync(start, end);
                return Success(result, "Berhasil mengambil laporan operasional.");
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        #endregion

        #region Analisis Produktivitas Endpoints

        /// <summary>
        /// Mendapatkan analisis produktivitas semua petugas
        /// Hanya untuk Operator dan Pemilik
        /// </summary>
        [HttpGet("produktivitas")]
        [ProducesResponseType(typeof(Common.ApiResponse<List<AnalisisProduktivitasDto>>), 200)]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 403)]
        public async Task<IActionResult> GetAnalisisProduktivitas()
        {
            try
            {
                if (!IsOperatorOrPemilik())
                {
                    return Forbidden("Akses ditolak. Fitur ini hanya untuk Operator dan Pemilik.");
                }

                var result = await _laporanService.GetAnalisisProduktivitasAsync();
                return Success(result, "Berhasil mengambil analisis produktivitas petugas.");
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Mendapatkan analisis produktivitas petugas tertentu
        /// Hanya untuk Operator dan Pemilik
        /// </summary>
        [HttpGet("produktivitas/{petugasId}")]
        [ProducesResponseType(typeof(Common.ApiResponse<AnalisisProduktivitasDto>), 200)]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 403)]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 404)]
        public async Task<IActionResult> GetAnalisisProduktivitasByPetugas(Guid petugasId)
        {
            try
            {
                if (!IsOperatorOrPemilik())
                {
                    return Forbidden("Akses ditolak. Fitur ini hanya untuk Operator dan Pemilik.");
                }

                var result = await _laporanService.GetAnalisisProduktivitasByPetugasAsync(petugasId);
                if (result == null)
                {
                    return NotFound("Petugas tidak ditemukan.");
                }

                return Success(result, "Berhasil mengambil analisis produktivitas petugas.");
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        #endregion

        #region PDF Export Endpoints

        /// <summary>
        /// Generate PDF untuk laporan operasional
        /// </summary>
        [HttpGet("operasional/pdf/{kandangId}")]
        [ProducesResponseType(typeof(FileContentResult), 200)]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 403)]
        public async Task<IActionResult> GetLaporanOperasionalPdf(
            Guid kandangId,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                if (!IsOperatorOrPemilik())
                {
                    return Forbidden("Akses ditolak. Fitur ini hanya untuk Operator dan Pemilik.");
                }

                // Default ke bulan ini jika tidak ada parameter
                var start = startDate ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                var end = endDate ?? start.AddMonths(1).AddDays(-1);

                var pdfBytes = await _laporanService.GenerateLaporanOperasionalPdfAsync(kandangId, start, end);

                var fileName = $"Laporan_Operasional_{start:yyyyMMdd}_{end:yyyyMMdd}.pdf";
                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Generate PDF untuk laporan kesehatan ayam
        /// </summary>
        [HttpGet("kesehatan/pdf/{kandangId}")]
        [ProducesResponseType(typeof(FileContentResult), 200)]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 403)]
        public async Task<IActionResult> GetLaporanKesehatanPdf(
            Guid kandangId,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                if (!IsOperatorOrPemilik())
                {
                    return Forbidden("Akses ditolak. Fitur ini hanya untuk Operator dan Pemilik.");
                }

                // Default ke bulan ini jika tidak ada parameter
                var start = startDate ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                var end = endDate ?? start.AddMonths(1).AddDays(-1);

                var pdfBytes = await _laporanService.GenerateLaporanKesehatanPdfAsync(kandangId, start, end);

                var fileName = $"Laporan_Kesehatan_Ayam_{start:yyyyMMdd}_{end:yyyyMMdd}.pdf";
                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        #endregion
    }
}

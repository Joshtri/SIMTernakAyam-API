using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIMTernakAyam.DTOs.Chart;
using SIMTernakAyam.Enums;
using SIMTernakAyam.Services.Interfaces;
using System.Security.Claims;

namespace SIMTernakAyam.Controllers
{
    [ApiController]
    [Route("api/charts")]
    [Authorize]
    public class ChartController : BaseController
    {
        private readonly IChartService _chartService;

        public ChartController(IChartService chartService)
        {
            _chartService = chartService;
        }

        #region Helper Methods

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

        /// <summary>
        /// Mendapatkan grafik produktivitas kandang
        /// </summary>
        /// <param name="period">monthly atau weekly</param>
        /// <param name="startDate">Tanggal mulai (opsional)</param>
        /// <param name="endDate">Tanggal selesai (opsional)</param>
        [HttpGet("produktivitas-trend")]
        [ProducesResponseType(typeof(Common.ApiResponse<ProduktivitasTrendDto>), 200)]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 403)]
        public async Task<IActionResult> GetProduktivitasTrend(
            [FromQuery] string period = "monthly",
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                if (!IsOperatorOrPemilik())
                {
                    return Forbidden("Akses ditolak. Fitur ini hanya untuk Operator dan Pemilik.");
                }

                if (period != "monthly" && period != "weekly")
                {
                    return Error("Parameter period harus 'monthly' atau 'weekly'");
                }

                var result = await _chartService.GetProduktivitasTrendAsync(period, startDate, endDate);
                return Success(result, "Berhasil mengambil data produktivitas trend.");
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Mendapatkan statistik mortalitas ayam per kandang
        /// </summary>
        /// <param name="kandangId">ID Kandang (opsional, jika kosong akan mengambil semua kandang)</param>
        /// <param name="period">monthly atau weekly</param>
        [HttpGet("mortalitas-statistik")]
        [ProducesResponseType(typeof(Common.ApiResponse<MortalitasStatistikDto>), 200)]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 403)]
        public async Task<IActionResult> GetMortalitasStatistik(
            [FromQuery] Guid? kandangId = null,
            [FromQuery] string period = "monthly")
        {
            try
            {
                if (!IsOperatorOrPemilik())
                {
                    return Forbidden("Akses ditolak. Fitur ini hanya untuk Operator dan Pemilik.");
                }

                if (period != "monthly" && period != "weekly")
                {
                    return Error("Parameter period harus 'monthly' atau 'weekly'");
                }

                var result = await _chartService.GetMortalitasStatistikAsync(kandangId, period);
                return Success(result, "Berhasil mengambil data statistik mortalitas.");
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Mendapatkan breakdown aktivitas operasional
        /// </summary>
        /// <param name="petugasId">ID Petugas (opsional, jika kosong akan mengambil semua petugas)</param>
        /// <param name="period">monthly</param>
        [HttpGet("operasional-breakdown")]
        [ProducesResponseType(typeof(Common.ApiResponse<OperasionalBreakdownDto>), 200)]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 403)]
        public async Task<IActionResult> GetOperasionalBreakdown(
            [FromQuery] Guid? petugasId = null,
            [FromQuery] string period = "monthly")
        {
            try
            {
                if (!IsOperatorOrPemilik())
                {
                    return Forbidden("Akses ditolak. Fitur ini hanya untuk Operator dan Pemilik.");
                }

                var result = await _chartService.GetOperasionalBreakdownAsync(petugasId, period);
                return Success(result, "Berhasil mengambil data operasional breakdown.");
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Mendapatkan analisis finansial (revenue vs cost)
        /// </summary>
        /// <param name="startDate">Tanggal mulai</param>
        /// <param name="endDate">Tanggal selesai</param>
        [HttpGet("financial-analysis")]
        [ProducesResponseType(typeof(Common.ApiResponse<FinancialAnalysisDto>), 200)]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 403)]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 400)]
        public async Task<IActionResult> GetFinancialAnalysis(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                if (!IsOperatorOrPemilik())
                {
                    return Forbidden("Akses ditolak. Fitur ini hanya untuk Operator dan Pemilik.");
                }

                var start = startDate ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                var end = endDate ?? DateTime.Now;

                if (start > end)
                {
                    return Error("Tanggal mulai tidak boleh lebih besar dari tanggal selesai");
                }

                var result = await _chartService.GetFinancialAnalysisAsync(start, end);
                return Success(result, "Berhasil mengambil data analisis finansial.");
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }
    }
}

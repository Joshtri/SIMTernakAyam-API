using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIMTernakAyam.DTOs.Dashboard;
using SIMTernakAyam.DTOs.Dashboard.Charts;
using SIMTernakAyam.Enums;
using SIMTernakAyam.Services.Interfaces;
using System.Security.Claims;

namespace SIMTernakAyam.Controllers
{
    [ApiController]
    [Route("api/dashboard")]
    [Authorize]
    public class DashboardController : BaseController
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        /// <summary>
        /// Get dashboard data based on current user's role
        /// </summary>
        /// <returns>Role-specific dashboard data</returns>
        [HttpGet]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 200)]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 401)]
        public async Task<IActionResult> GetDashboard()
        {
            try
            {
                // Get user info from JWT token
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                var roleClaim = User.FindFirst(ClaimTypes.Role);

                if (userIdClaim == null || roleClaim == null || 
                    !Guid.TryParse(userIdClaim.Value, out var userId) ||
                    !Enum.TryParse<RoleEnum>(roleClaim.Value, out var role))
                {
                    return Unauthorized("Token tidak valid.");
                }

                var dashboardData = await _dashboardService.GetDashboardDataAsync(userId, role);
                return Success(dashboardData, $"Berhasil mengambil data dashboard untuk role {role}.");
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Get dashboard data for specific role (admin use)
        /// </summary>
        /// <param name="role">Target role</param>
        /// <param name="userId">Optional user ID for role-specific data</param>
        /// <returns>Role-specific dashboard data</returns>
        [HttpGet("role/{role}")]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 200)]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 400)]
        public async Task<IActionResult> GetDashboardByRole(RoleEnum role, [FromQuery] Guid? userId = null)
        {
            try
            {
                var targetUserId = userId ?? Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                var dashboardData = await _dashboardService.GetDashboardDataAsync(targetUserId, role);
                return Success(dashboardData, $"Berhasil mengambil data dashboard untuk role {role}.");
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Get operator dashboard data
        /// </summary>
        /// <returns>Operator dashboard</returns>
        [HttpGet("operator")]
        [ProducesResponseType(typeof(Common.ApiResponse<OperatorDashboardDto>), 200)]
        public async Task<IActionResult> GetOperatorDashboard()
        {
            try
            {
                var dashboard = await _dashboardService.GetOperatorDashboardAsync();
                return Success(dashboard, "Berhasil mengambil dashboard operator.");
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Get petugas dashboard data
        /// </summary>
        /// <param name="petugasId">Optional petugas ID, uses current user if not provided</param>
        /// <returns>Petugas dashboard</returns>
        [HttpGet("petugas")]
        [ProducesResponseType(typeof(Common.ApiResponse<PetugasDashboardDto>), 200)]
        public async Task<IActionResult> GetPetugasDashboard([FromQuery] Guid? petugasId = null)
        {
            try
            {
                var targetPetugasId = petugasId ?? Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                var dashboard = await _dashboardService.GetPetugasDashboardAsync(targetPetugasId);
                return Success(dashboard, "Berhasil mengambil dashboard petugas.");
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Get pemilik dashboard data
        /// </summary>
        /// <returns>Pemilik dashboard</returns>
        [HttpGet("pemilik")]
        [ProducesResponseType(typeof(Common.ApiResponse<PemilikDashboardDto>), 200)]
        public async Task<IActionResult> GetPemilikDashboard()
        {
            try
            {
                var dashboard = await _dashboardService.GetPemilikDashboardAsync();
                return Success(dashboard, "Berhasil mengambil dashboard pemilik.");
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        #region Chart Endpoints

        /// <summary>
        /// Get revenue vs expenses chart data
        /// </summary>
        /// <param name="period">Period: daily, weekly, monthly, yearly</param>
        /// <returns>Revenue vs expenses chart</returns>
        [HttpGet("charts/revenue-expense")]
        [ProducesResponseType(typeof(Common.ApiResponse<RevenueExpenseChartDto>), 200)]
        public async Task<IActionResult> GetRevenueExpenseChart([FromQuery] string period = "monthly")
        {
            try
            {
                var chartData = await _dashboardService.GetRevenueExpenseChartAsync(period);
                return Success(chartData, "Berhasil mengambil data chart revenue vs expense.");
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Get mortality trend chart data
        /// </summary>
        /// <param name="kandangId">Optional kandang ID for specific kandang</param>
        /// <param name="period">Period: daily, weekly, monthly, yearly</param>
        /// <returns>Mortality trend chart</returns>
        [HttpGet("charts/mortality-trend")]
        [ProducesResponseType(typeof(Common.ApiResponse<MortalityTrendChartDto>), 200)]
        public async Task<IActionResult> GetMortalityTrendChart([FromQuery] Guid? kandangId = null, [FromQuery] string period = "monthly")
        {
            try
            {
                var chartData = await _dashboardService.GetMortalityTrendChartAsync(kandangId, period);
                return Success(chartData, "Berhasil mengambil data chart trend mortalitas.");
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Get production performance chart data
        /// </summary>
        /// <param name="period">Period: daily, weekly, monthly, yearly</param>
        /// <returns>Production chart</returns>
        [HttpGet("charts/production")]
        [ProducesResponseType(typeof(Common.ApiResponse<ProductionChartDto>), 200)]
        public async Task<IActionResult> GetProductionChart([FromQuery] string period = "monthly")
        {
            try
            {
                var chartData = await _dashboardService.GetProductionChartAsync(period);
                return Success(chartData, "Berhasil mengambil data chart produksi.");
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Get kandang utilization chart data
        /// </summary>
        /// <returns>Kandang utilization chart</returns>
        [HttpGet("charts/kandang-utilization")]
        [ProducesResponseType(typeof(Common.ApiResponse<KandangUtilizationChartDto>), 200)]
        public async Task<IActionResult> GetKandangUtilizationChart()
        {
            try
            {
                var chartData = await _dashboardService.GetKandangUtilizationChartAsync();
                return Success(chartData, "Berhasil mengambil data chart utilisasi kandang.");
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Get feed consumption chart data
        /// </summary>
        /// <param name="period">Period: daily, weekly, monthly, yearly</param>
        /// <returns>Feed consumption chart</returns>
        [HttpGet("charts/feed-consumption")]
        [ProducesResponseType(typeof(Common.ApiResponse<FeedConsumptionChartDto>), 200)]
        public async Task<IActionResult> GetFeedConsumptionChart([FromQuery] string period = "monthly")
        {
            try
            {
                var chartData = await _dashboardService.GetFeedConsumptionChartAsync(period);
                return Success(chartData, "Berhasil mengambil data chart konsumsi pakan.");
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Get financial performance chart data
        /// </summary>
        /// <param name="period">Period: daily, weekly, monthly, yearly</param>
        /// <returns>Financial performance chart</returns>
        [HttpGet("charts/financial-performance")]
        [ProducesResponseType(typeof(Common.ApiResponse<FinancialPerformanceChartDto>), 200)]
        public async Task<IActionResult> GetFinancialPerformanceChart([FromQuery] string period = "monthly")
        {
            try
            {
                var chartData = await _dashboardService.GetFinancialPerformanceChartAsync(period);
                return Success(chartData, "Berhasil mengambil data chart performa finansial.");
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Get operational activities chart data
        /// </summary>
        /// <param name="petugasId">Optional petugas ID for specific petugas</param>
        /// <param name="period">Period: daily, weekly, monthly</param>
        /// <returns>Operational activities chart</returns>
        [HttpGet("charts/operational-activities")]
        [ProducesResponseType(typeof(Common.ApiResponse<OperationalActivitiesChartDto>), 200)]
        public async Task<IActionResult> GetOperationalActivitiesChart([FromQuery] Guid? petugasId = null, [FromQuery] string period = "weekly")
        {
            try
            {
                var chartData = await _dashboardService.GetOperationalActivitiesChartAsync(petugasId, period);
                return Success(chartData, "Berhasil mengambil data chart aktivitas operasional.");
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Get stock levels chart data
        /// </summary>
        /// <returns>Stock levels chart</returns>
        [HttpGet("charts/stock-levels")]
        [ProducesResponseType(typeof(Common.ApiResponse<StockLevelsChartDto>), 200)]
        public async Task<IActionResult> GetStockLevelsChart()
        {
            try
            {
                var chartData = await _dashboardService.GetStockLevelsChartAsync();
                return Success(chartData, "Berhasil mengambil data chart level stok.");
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Get performance comparison chart data
        /// </summary>
        /// <param name="kandangId">Optional kandang ID for specific kandang comparison</param>
        /// <returns>Performance comparison chart</returns>
        [HttpGet("charts/performance-comparison")]
        [ProducesResponseType(typeof(Common.ApiResponse<PerformanceComparisonChartDto>), 200)]
        public async Task<IActionResult> GetPerformanceComparisonChart([FromQuery] Guid? kandangId = null)
        {
            try
            {
                var chartData = await _dashboardService.GetPerformanceComparisonChartAsync(kandangId);
                return Success(chartData, "Berhasil mengambil data chart perbandingan performa.");
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Get seasonal trends chart data
        /// </summary>
        /// <returns>Seasonal trends chart</returns>
        [HttpGet("charts/seasonal-trends")]
        [ProducesResponseType(typeof(Common.ApiResponse<SeasonalTrendsChartDto>), 200)]
        public async Task<IActionResult> GetSeasonalTrendsChart()
        {
            try
            {
                var chartData = await _dashboardService.GetSeasonalTrendsChartAsync();
                return Success(chartData, "Berhasil mengambil data chart trend musiman.");
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Get cost breakdown chart data
        /// </summary>
        /// <param name="period">Period: monthly, quarterly, yearly</param>
        /// <returns>Cost breakdown chart</returns>
        [HttpGet("charts/cost-breakdown")]
        [ProducesResponseType(typeof(Common.ApiResponse<CostBreakdownChartDto>), 200)]
        public async Task<IActionResult> GetCostBreakdownChart([FromQuery] string period = "monthly")
        {
            try
            {
                var chartData = await _dashboardService.GetCostBreakdownChartAsync(period);
                return Success(chartData, "Berhasil mengambil data chart breakdown biaya.");
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        #endregion
    }
}
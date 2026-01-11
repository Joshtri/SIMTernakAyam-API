using SIMTernakAyam.DTOs.Dashboard;
using SIMTernakAyam.DTOs.Dashboard.Charts;
using SIMTernakAyam.Enums;

namespace SIMTernakAyam.Services.Interfaces
{
    public interface IDashboardService
    {
        /// <summary>
        /// Get dashboard data based on user role
        /// </summary>
        Task<object> GetDashboardDataAsync(Guid userId, RoleEnum role);

        /// <summary>
        /// Get dashboard data for Operator role
        /// </summary>
        Task<OperatorDashboardDto> GetOperatorDashboardAsync();

        /// <summary>
        /// Get dashboard data for Petugas role
        /// </summary>
        Task<PetugasDashboardDto> GetPetugasDashboardAsync(Guid petugasId);

        /// <summary>
        /// Get dashboard data for Pemilik role
        /// </summary>
        /// <param name="year">Optional year filter. Defaults to current year.</param>
        /// <param name="month">Optional month filter. Defaults to current month.</param>
        Task<PemilikDashboardDto> GetPemilikDashboardAsync(int? year = null, int? month = null);

        /// <summary>
        /// Get chart data for revenue vs expenses
        /// </summary>
        Task<RevenueExpenseChartDto> GetRevenueExpenseChartAsync(string period = "monthly");

        /// <summary>
        /// Get chart data for mortality trends
        /// </summary>
        Task<MortalityTrendChartDto> GetMortalityTrendChartAsync(Guid? kandangId = null, string period = "monthly");

        /// <summary>
        /// Get chart data for production performance
        /// </summary>
        Task<ProductionChartDto> GetProductionChartAsync(string period = "monthly");

        /// <summary>
        /// Get chart data for kandang utilization
        /// </summary>
        Task<KandangUtilizationChartDto> GetKandangUtilizationChartAsync();

        /// <summary>
        /// Get chart data for feed consumption
        /// </summary>
        Task<FeedConsumptionChartDto> GetFeedConsumptionChartAsync(string period = "monthly");

        /// <summary>
        /// Get chart data for financial performance
        /// </summary>
        Task<FinancialPerformanceChartDto> GetFinancialPerformanceChartAsync(string period = "monthly");

        /// <summary>
        /// Get chart data for operational activities
        /// </summary>
        Task<OperationalActivitiesChartDto> GetOperationalActivitiesChartAsync(Guid? petugasId = null, string period = "weekly");

        /// <summary>
        /// Get chart data for stock levels
        /// </summary>
        Task<StockLevelsChartDto> GetStockLevelsChartAsync();

        /// <summary>
        /// Get chart data for performance comparison
        /// </summary>
        Task<PerformanceComparisonChartDto> GetPerformanceComparisonChartAsync(Guid? kandangId = null);

        /// <summary>
        /// Get chart data for seasonal trends
        /// </summary>
        Task<SeasonalTrendsChartDto> GetSeasonalTrendsChartAsync();

        /// <summary>
        /// Get chart data for cost breakdown
        /// </summary>
        Task<CostBreakdownChartDto> GetCostBreakdownChartAsync(string period = "monthly");
    }
}
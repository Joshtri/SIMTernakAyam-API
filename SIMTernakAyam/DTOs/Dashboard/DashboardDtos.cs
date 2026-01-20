namespace SIMTernakAyam.DTOs.Dashboard
{
    /// <summary>
    /// Dashboard data untuk role OPERATOR - System-wide overview
    /// </summary>
    public class OperatorDashboardDto
    {
        public SystemOverviewDto SystemOverview { get; set; } = new();
        public List<KandangPerformanceDto> KandangPerformances { get; set; } = new();
        public FinancialSummaryDto FinancialSummary { get; set; } = new();
        public List<AlertDto> SystemAlerts { get; set; } = new();
        public ProductivityStatsDto ProductivityStats { get; set; } = new();
    }

    /// <summary>
    /// Dashboard data untuk role PETUGAS - Kandang-specific view
    /// </summary>
    public class PetugasDashboardDto
    {
        public List<MyKandangDto> MyKandangs { get; set; } = new();
        public DailyTasksDto DailyTasks { get; set; } = new();
        public StockAlertsDto StockAlerts { get; set; } = new();
        public MyPerformanceDto MyPerformance { get; set; } = new();
        public UpcomingActivitiesDto UpcomingActivities { get; set; } = new();
    }

    /// <summary>
    /// Dashboard data untuk role PEMILIK - Business overview
    /// </summary>
    public class PemilikDashboardDto
    {
        public BusinessKpiDto BusinessKpi { get; set; } = new();
        public ProfitabilityDto Profitability { get; set; } = new();
        public ComparisonAnalysisDto ComparisonAnalysis { get; set; } = new();
        public StrategicInsightsDto StrategicInsights { get; set; } = new();
        public MonthlyTrendsDto MonthlyTrends { get; set; } = new();
    }

    // Common DTOs
    public class SystemOverviewDto
    {
        public int TotalKandangs { get; set; }
        public int TotalAyams { get; set; }
        public int TotalUsers { get; set; }
        public int ActiveOperations { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    public class KandangPerformanceDto
    {
        public Guid KandangId { get; set; }
        public string KandangName { get; set; } = string.Empty;
        public string PetugasName { get; set; } = string.Empty;
        public int CurrentAyams { get; set; }
        public int Capacity { get; set; }
        public double UtilizationPercentage { get; set; }
        public int MortalityThisMonth { get; set; }
        public double MortalityRate { get; set; }
        public string Status { get; set; } = string.Empty; // Good, Warning, Critical
    }

    public class FinancialSummaryDto
    {
        public decimal TotalRevenue { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal NetProfit { get; set; }
        public decimal ProfitMargin { get; set; }
        public decimal MonthlyChange { get; set; }
    }

    public class AlertDto
    {
        public string Type { get; set; } = string.Empty; // Stock, Mortality, Maintenance
        public string Message { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty; // Low, Medium, High, Critical
        public DateTime CreatedAt { get; set; }
        public Guid? RelatedEntityId { get; set; }
        public string? RelatedEntityName { get; set; }
    }

    public class ProductivityStatsDto
    {
        public double AveragePanenWeight { get; set; }
        public int TotalPanenThisMonth { get; set; }
        public double AverageMortalityRate { get; set; }
        public double FeedConversionRatio { get; set; }
        public int ActiveKandangs { get; set; }
    }

    public class MyKandangDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int CurrentAyams { get; set; }
        public int Capacity { get; set; }
        public int MortalityToday { get; set; }
        public int MortalityThisWeek { get; set; }
        public DateTime LastFeedTime { get; set; }
        public DateTime LastVaccinationTime { get; set; }
        public string HealthStatus { get; set; } = string.Empty;
    }

    public class DailyTasksDto
    {
        public int PendingFeedings { get; set; }
        public int PendingVaccinations { get; set; }
        public int PendingCleanings { get; set; }
        public int CompletedTasks { get; set; }
        public int TotalTasks { get; set; }
        public double CompletionRate { get; set; }
    }

    public class StockAlertsDto
    {
        public List<StockItemDto> LowStockPakan { get; set; } = new();
        public List<StockItemDto> LowStockVaksin { get; set; } = new();
        public int CriticalStockCount { get; set; }
        public int WarningStockCount { get; set; }
    }

    public class StockItemDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int CurrentStock { get; set; }
        public int MinimumStock { get; set; }
        public string Status { get; set; } = string.Empty; // Critical, Warning, Good
    }

    public class MyPerformanceDto
    {
        public double EfficiencyScore { get; set; }
        public int TasksCompletedThisWeek { get; set; }
        public int TasksCompletedThisMonth { get; set; }
        public double AverageMortalityRate { get; set; }
        public int KandangsManaged { get; set; }
        public string PerformanceLevel { get; set; } = string.Empty; // Excellent, Good, Average, Needs Improvement
    }

    public class UpcomingActivitiesDto
    {
        public List<ScheduledActivityDto> TodayActivities { get; set; } = new();
        public List<ScheduledActivityDto> TomorrowActivities { get; set; } = new();
        public List<ScheduledActivityDto> ThisWeekActivities { get; set; } = new();
    }

    public class ScheduledActivityDto
    {
        public string ActivityType { get; set; } = string.Empty;
        public string KandangName { get; set; } = string.Empty;
        public DateTime ScheduledTime { get; set; }
        public string Priority { get; set; } = string.Empty;
        public bool IsOverdue { get; set; }
    }

    public class BusinessKpiDto
    {
        public decimal MonthlyRevenue { get; set; }
        public decimal MonthlyProfit { get; set; }
        public double ReturnOnInvestment { get; set; }
        public int TotalAyamStock { get; set; }
        public double AverageProductivity { get; set; }
        /// <summary>
        /// Feed Conversion Ratio (FCR) - Rasio konversi pakan
        /// Semakin rendah semakin baik (ideal: 1.5-1.8)
        /// FCR = Total Pakan (kg) / Total Berat Ayam (kg)
        /// </summary>
        public double FeedConversionRatio { get; set; }
        public double CustomerSatisfaction { get; set; }
        public int MarketShare { get; set; }
    }

    public class ProfitabilityDto
    {
        public decimal GrossProfit { get; set; }
        public decimal NetProfit { get; set; }
        public decimal OperatingExpenses { get; set; }
        public decimal CostPerKg { get; set; }
        public decimal PricePerKg { get; set; }
        public decimal ProfitPerKg { get; set; }
        public double ProfitMargin { get; set; }
    }

    public class ComparisonAnalysisDto
    {
        public MonthComparisonDto CurrentVsPreviousMonth { get; set; } = new();
        public YearComparisonDto CurrentVsPreviousYear { get; set; } = new();
        public BenchmarkDto IndustryBenchmark { get; set; } = new();
    }

    public class MonthComparisonDto
    {
        public decimal RevenueChange { get; set; }
        public decimal ProfitChange { get; set; }
        public double ProductivityChange { get; set; }
        public double MortalityRateChange { get; set; }
        public double EfficiencyChange { get; set; }
    }

    public class YearComparisonDto
    {
        public decimal RevenueGrowth { get; set; }
        public decimal ProfitGrowth { get; set; }
        public double ProductivityGrowth { get; set; }
        public int CapacityGrowth { get; set; }
        public double EfficiencyImprovement { get; set; }
    }

    public class BenchmarkDto
    {
        public double IndustryAvgMortalityRate { get; set; }
        public double YourMortalityRate { get; set; }
        public double IndustryAvgProductivity { get; set; }
        public double YourProductivity { get; set; }
        /// <summary>
        /// ? BARU: Industry average FCR (Feed Conversion Ratio)
        /// Standard industri: 1.6 - 1.8
        /// </summary>
        public double IndustryAvgFcr { get; set; }
        /// <summary>
        /// ? BARU: Your actual FCR
        /// </summary>
        public double YourFcr { get; set; }
        public string PerformanceRating { get; set; } = string.Empty;
    }

    public class StrategicInsightsDto
    {
        public List<string> Recommendations { get; set; } = new();
        public List<string> Opportunities { get; set; } = new();
        public List<string> Risks { get; set; } = new();
        public List<string> KeySuccessFactors { get; set; } = new();
    }

    public class MonthlyTrendsDto
    {
        public List<MonthlyDataDto> RevenueData { get; set; } = new();
        public List<MonthlyDataDto> ProfitData { get; set; } = new();
        /// <summary>
        /// ? BARU: Trend FCR 6 bulan terakhir
        /// </summary>
        public List<MonthlyFcrDataDto> FcrData { get; set; } = new();
        public List<MonthlyDataDto> ProductivityData { get; set; } = new();
        public List<MonthlyDataDto> MortalityData { get; set; } = new();
    }

    public class MonthlyDataDto
    {
        public string Month { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public double Percentage { get; set; }
    }

    /// <summary>
    /// ? BARU: DTO khusus untuk FCR trend
    /// </summary>
    public class MonthlyFcrDataDto
    {
        public string Month { get; set; } = string.Empty;
        public double FcrValue { get; set; }
        public string Status { get; set; } = string.Empty; // Excellent (<1.6), Good (1.6-1.8), Fair (1.8-2.0), Poor (>2.0)
    }
}
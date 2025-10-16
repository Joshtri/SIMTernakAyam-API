namespace SIMTernakAyam.DTOs.Dashboard.Charts
{
    /// <summary>
    /// Base chart data structure
    /// </summary>
    public class ChartDataDto
    {
        public string ChartType { get; set; } = string.Empty; // line, bar, pie, doughnut, area
        public string Title { get; set; } = string.Empty;
        public List<string> Labels { get; set; } = new();
        public List<ChartDatasetDto> Datasets { get; set; } = new();
    }

    public class ChartDatasetDto
    {
        public string Label { get; set; } = string.Empty;
        public List<decimal> Data { get; set; } = new();
        public string BackgroundColor { get; set; } = string.Empty;
        public string BorderColor { get; set; } = string.Empty;
        public int BorderWidth { get; set; } = 2;
        public bool Fill { get; set; } = false;
    }

    /// <summary>
    /// Revenue vs Expenses Chart (Bar Chart)
    /// </summary>
    public class RevenueExpenseChartDto : ChartDataDto
    {
        public decimal TotalRevenue { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal NetProfit { get; set; }
    }

    /// <summary>
    /// Mortality Rate Trend Chart (Line Chart)
    /// </summary>
    public class MortalityTrendChartDto : ChartDataDto
    {
        public double AverageMortalityRate { get; set; }
        public double HighestRate { get; set; }
        public double LowestRate { get; set; }
        public string TrendDirection { get; set; } = string.Empty; // Increasing, Decreasing, Stable
    }

    /// <summary>
    /// Production Performance Chart (Area Chart)
    /// </summary>
    public class ProductionChartDto : ChartDataDto
    {
        public int TotalProduced { get; set; }
        public double AverageWeight { get; set; }
        public double ProductionEfficiency { get; set; }
    }

    /// <summary>
    /// Kandang Capacity Utilization (Doughnut Chart)
    /// </summary>
    public class KandangUtilizationChartDto : ChartDataDto
    {
        public double AverageUtilization { get; set; }
        public int TotalCapacity { get; set; }
        public int CurrentOccupancy { get; set; }
        public List<KandangUtilizationDto> KandangDetails { get; set; } = new();
    }

    public class KandangUtilizationDto
    {
        public string KandangName { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public int Current { get; set; }
        public double UtilizationPercentage { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    /// <summary>
    /// Feed Consumption Chart (Bar Chart)
    /// </summary>
    public class FeedConsumptionChartDto : ChartDataDto
    {
        public decimal TotalConsumption { get; set; }
        public decimal AverageDaily { get; set; }
        public decimal CostPerKg { get; set; }
        public List<FeedTypeConsumptionDto> FeedTypes { get; set; } = new();
    }

    public class FeedTypeConsumptionDto
    {
        public string FeedName { get; set; } = string.Empty;
        public decimal Consumption { get; set; }
        public decimal Cost { get; set; }
        public double Percentage { get; set; }
    }

    /// <summary>
    /// Financial Performance Chart (Multi-line Chart)
    /// </summary>
    public class FinancialPerformanceChartDto : ChartDataDto
    {
        public List<FinancialMetricDto> Metrics { get; set; } = new();
        public string Period { get; set; } = string.Empty; // Daily, Weekly, Monthly, Yearly
    }

    public class FinancialMetricDto
    {
        public string MetricName { get; set; } = string.Empty;
        public decimal CurrentValue { get; set; }
        public decimal PreviousValue { get; set; }
        public double ChangePercentage { get; set; }
        public string Trend { get; set; } = string.Empty;
    }

    /// <summary>
    /// Operational Activities Chart (Stacked Bar Chart)
    /// </summary>
    public class OperationalActivitiesChartDto : ChartDataDto
    {
        public int TotalActivities { get; set; }
        public int CompletedActivities { get; set; }
        public int PendingActivities { get; set; }
        public double CompletionRate { get; set; }
    }

    /// <summary>
    /// Stock Levels Chart (Horizontal Bar Chart)
    /// </summary>
    public class StockLevelsChartDto : ChartDataDto
    {
        public List<StockItemChartDto> StockItems { get; set; } = new();
        public int CriticalItems { get; set; }
        public int WarningItems { get; set; }
        public int GoodItems { get; set; }
    }

    public class StockItemChartDto
    {
        public string ItemName { get; set; } = string.Empty;
        public int CurrentStock { get; set; }
        public int MinimumStock { get; set; }
        public int MaximumStock { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty; // Pakan, Vaksin
    }

    /// <summary>
    /// Performance Comparison Chart (Radar Chart)
    /// </summary>
    public class PerformanceComparisonChartDto : ChartDataDto
    {
        public List<PerformanceMetricDto> Metrics { get; set; } = new();
        public string ComparisonPeriod { get; set; } = string.Empty;
    }

    public class PerformanceMetricDto
    {
        public string MetricName { get; set; } = string.Empty;
        public double CurrentScore { get; set; }
        public double BenchmarkScore { get; set; }
        public double MaxScore { get; set; } = 100;
        public string Unit { get; set; } = string.Empty;
    }

    /// <summary>
    /// Seasonal Trends Chart (Multi-area Chart)
    /// </summary>
    public class SeasonalTrendsChartDto : ChartDataDto
    {
        public List<SeasonalDataDto> SeasonalData { get; set; } = new();
        public string BestPerformingSeason { get; set; } = string.Empty;
        public string WorstPerformingSeason { get; set; } = string.Empty;
    }

    public class SeasonalDataDto
    {
        public string Season { get; set; } = string.Empty; // Q1, Q2, Q3, Q4 or Spring, Summer, etc.
        public decimal Revenue { get; set; }
        public double Productivity { get; set; }
        public double MortalityRate { get; set; }
        public int AyamCount { get; set; }
    }

    /// <summary>
    /// Cost Breakdown Chart (Pie Chart)
    /// </summary>
    public class CostBreakdownChartDto : ChartDataDto
    {
        public decimal TotalCosts { get; set; }
        public List<CostCategoryDto> CostCategories { get; set; } = new();
        public string LargestCostCategory { get; set; } = string.Empty;
    }

    public class CostCategoryDto
    {
        public string CategoryName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public double Percentage { get; set; }
        public string Color { get; set; } = string.Empty;
    }
}
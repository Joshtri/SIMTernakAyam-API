namespace SIMTernakAyam.DTOs.Chart
{
    /// <summary>
    /// DTO untuk analisis finansial (Bar/Line Combo Chart)
    /// </summary>
    public class FinancialAnalysisDto
    {
        public DateTime TanggalMulai { get; set; }
        public DateTime TanggalSelesai { get; set; }

        // Data untuk chart
        public ChartDataDto ChartData { get; set; } = new();

        // Summary
        public decimal TotalRevenue { get; set; }
        public decimal TotalCost { get; set; }
        public decimal NetProfit { get; set; }
        public decimal ProfitMargin { get; set; }

        // Breakdown per kategori
        public List<BiayaBreakdownDto> BiayaBreakdown { get; set; } = new();
    }

    public class BiayaBreakdownDto
    {
        public string JenisBiaya { get; set; } = string.Empty;
        public decimal TotalBiaya { get; set; }
        public decimal Persentase { get; set; }
    }
}

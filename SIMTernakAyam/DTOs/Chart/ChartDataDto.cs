namespace SIMTernakAyam.DTOs.Chart
{
    /// <summary>
    /// DTO generik untuk data chart
    /// </summary>
    public class ChartDataDto
    {
        public List<string> Labels { get; set; } = new();
        public List<ChartSeriesDto> Series { get; set; } = new();
    }

    public class ChartSeriesDto
    {
        public string Name { get; set; } = string.Empty;
        public List<decimal> Data { get; set; } = new();
        public string? Color { get; set; }
    }
}

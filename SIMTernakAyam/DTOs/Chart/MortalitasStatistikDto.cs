namespace SIMTernakAyam.DTOs.Chart
{
    /// <summary>
    /// DTO untuk statistik mortalitas (Bar Chart)
    /// </summary>
    public class MortalitasStatistikDto
    {
        public Guid? KandangId { get; set; }
        public string? NamaKandang { get; set; }
        public string Periode { get; set; } = string.Empty;

        // Data untuk chart
        public ChartDataDto ChartData { get; set; } = new();

        // Summary
        public int TotalMortalitas { get; set; }
        public int TotalAyam { get; set; }
        public decimal PersentaseMortalitas { get; set; }
        public List<PenyebabMortalitasDto> TopPenyebab { get; set; } = new();
    }

    public class PenyebabMortalitasDto
    {
        public string Penyebab { get; set; } = string.Empty;
        public int Jumlah { get; set; }
        public decimal Persentase { get; set; }
    }
}

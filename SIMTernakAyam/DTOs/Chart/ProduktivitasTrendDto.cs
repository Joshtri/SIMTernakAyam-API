namespace SIMTernakAyam.DTOs.Chart
{
    /// <summary>
    /// DTO untuk grafik produktivitas kandang (Line Chart)
    /// </summary>
    public class ProduktivitasTrendDto
    {
        public string Periode { get; set; } = string.Empty;
        public DateTime TanggalMulai { get; set; }
        public DateTime TanggalSelesai { get; set; }

        // Data untuk chart
        public ChartDataDto ChartData { get; set; } = new();

        // Summary
        public int TotalKandangAktif { get; set; }
        public int TotalOperasionalDilakukan { get; set; }
        public decimal RataProduktivitas { get; set; }
    }
}

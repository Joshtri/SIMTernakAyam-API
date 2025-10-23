namespace SIMTernakAyam.DTOs.Chart
{
    /// <summary>
    /// DTO untuk breakdown operasional (Pie Chart)
    /// </summary>
    public class OperasionalBreakdownDto
    {
        public Guid? PetugasId { get; set; }
        public string? NamaPetugas { get; set; }
        public string Periode { get; set; } = string.Empty;

        // Data untuk pie chart
        public List<OperasionalCategoryDto> Categories { get; set; } = new();

        // Summary
        public int TotalOperasional { get; set; }
        public int TotalKandangDikelola { get; set; }
        public string KategoriTerbanyak { get; set; } = string.Empty;
    }

    public class OperasionalCategoryDto
    {
        public string NamaKegiatan { get; set; } = string.Empty;
        public int JumlahOperasional { get; set; }
        public decimal Persentase { get; set; }
        public string? Color { get; set; }
    }
}

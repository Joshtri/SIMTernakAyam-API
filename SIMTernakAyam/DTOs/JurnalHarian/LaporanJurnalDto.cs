namespace SIMTernakAyam.DTOs.JurnalHarian
{
    /// <summary>
    /// DTO untuk laporan jurnal harian (aggregated)
    /// </summary>
    public class LaporanJurnalDto
    {
        public DateTime TanggalMulai { get; set; }
        public DateTime TanggalSelesai { get; set; }
        public Guid? PetugasId { get; set; }
        public string? NamaPetugas { get; set; }

        public int TotalJurnal { get; set; }
        public TimeSpan TotalDurasiKerja { get; set; }
        public decimal RataRataDurasiPerHari { get; set; } // dalam jam

        public List<JurnalPerKandangDto> JurnalPerKandang { get; set; } = new();
        public List<JurnalHarianResponseDto> DetailJurnal { get; set; } = new();
    }

    public class JurnalPerKandangDto
    {
        public Guid KandangId { get; set; }
        public string NamaKandang { get; set; } = string.Empty;
        public int JumlahKegiatan { get; set; }
        public TimeSpan TotalDurasi { get; set; }
    }
}

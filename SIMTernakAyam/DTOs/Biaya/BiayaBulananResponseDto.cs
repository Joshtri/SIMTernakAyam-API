namespace SIMTernakAyam.DTOs.Biaya
{
    /// <summary>
    /// DTO untuk rekap biaya bulanan per kandang
    /// </summary>
    public class BiayaBulananResponseDto
    {
        public int Bulan { get; set; }
        public int Tahun { get; set; }
        public Guid? KandangId { get; set; }
        public string? KandangNama { get; set; }
        public decimal TotalBiayaListrik { get; set; }
        public decimal TotalBiayaAir { get; set; }
        public decimal TotalBiayaLainnya { get; set; }
        public decimal TotalBiaya { get; set; }
        public List<BiayaResponseDto> DetailBiaya { get; set; } = new List<BiayaResponseDto>();
    }

    /// <summary>
    /// DTO untuk rekap biaya bulanan semua kandang
    /// </summary>
    public class RekapBiayaBulananDto
    {
        public int Bulan { get; set; }
        public int Tahun { get; set; }
        public List<BiayaBulananResponseDto> PerKandang { get; set; } = new List<BiayaBulananResponseDto>();
        public decimal GrandTotalBiayaListrik { get; set; }
        public decimal GrandTotalBiayaAir { get; set; }
        public decimal GrandTotalBiayaLainnya { get; set; }
        public decimal GrandTotal { get; set; }
    }
}

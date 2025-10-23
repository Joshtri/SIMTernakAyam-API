namespace SIMTernakAyam.DTOs.Laporan
{
    public class LaporanKesehatanResponseDto
    {
        public string KandangNama { get; set; } = string.Empty;
        public DateTime TanggalMulai { get; set; }
        public DateTime TanggalAkhir { get; set; }
        public int JumlahAyam { get; set; }
        public int TotalMortalitas { get; set; }
        public decimal PersentaseMortalitas { get; set; }
        public int TotalVaksinasi { get; set; }
        public string StatusKesehatan { get; set; } = "Sehat";
        public string? Rekomendasi { get; set; }
        public List<RiwayatMortalitasDto> RiwayatMortalitas { get; set; } = new List<RiwayatMortalitasDto>();
        public List<RiwayatVaksinasiDto> RiwayatVaksinasi { get; set; } = new List<RiwayatVaksinasiDto>();
    }

    public class RiwayatMortalitasDto
    {
        public DateTime Tanggal { get; set; }
        public int JumlahMati { get; set; }
        public string PenyebabKematian { get; set; } = string.Empty;
        public string? Keterangan { get; set; }
    }

    public class RiwayatVaksinasiDto
    {
        public DateTime Tanggal { get; set; }
        public string JenisVaksin { get; set; } = string.Empty;
        public int Jumlah { get; set; }
        public string PetugasNama { get; set; } = string.Empty;
    }
}

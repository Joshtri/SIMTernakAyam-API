namespace SIMTernakAyam.DTOs.Laporan
{
    public class LaporanOperasionalResponseDto
    {
        public string KandangNama { get; set; } = string.Empty;
        public string PetugasNama { get; set; } = string.Empty;
        public DateTime TanggalMulai { get; set; }
        public DateTime TanggalAkhir { get; set; }
        public int TotalKegiatan { get; set; }
        public decimal TotalPakanDigunakan { get; set; }
        public int TotalVaksinDigunakan { get; set; }
        public decimal TotalBiaya { get; set; }
        public List<DetailKegiatanDto> DetailKegiatan { get; set; } = new List<DetailKegiatanDto>();
        public List<string> CatatanPengeluaran { get; set; } = new List<string>();
    }

    public class DetailKegiatanDto
    {
        public DateTime Tanggal { get; set; }
        public string JenisKegiatanNama { get; set; } = string.Empty;
        public int Jumlah { get; set; }
        public string Satuan { get; set; } = string.Empty;
        public string? ItemNama { get; set; }
        public decimal? Biaya { get; set; }
    }
}

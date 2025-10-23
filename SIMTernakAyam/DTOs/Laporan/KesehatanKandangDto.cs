namespace SIMTernakAyam.DTOs.Laporan
{
    /// <summary>
    /// DTO untuk menampilkan kesehatan ayam per kandang
    /// </summary>
    public class KesehatanKandangDto
    {
        public Guid KandangId { get; set; }
        public string NamaKandang { get; set; } = string.Empty;
        public string Lokasi { get; set; } = string.Empty;
        public int Kapasitas { get; set; }

        // Petugas yang bertanggung jawab
        public Guid PetugasId { get; set; }
        public string NamaPetugas { get; set; } = string.Empty;
        public string UsernamePetugas { get; set; } = string.Empty;
        public string EmailPetugas { get; set; } = string.Empty;
        public string NoWAPetugas { get; set; } = string.Empty;

        // Data Kesehatan
        public int TotalAyamMasuk { get; set; }
        public int TotalMortalitas { get; set; }
        public decimal PersentaseMortalitas { get; set; }
        public int AyamHidup { get; set; }
        public decimal TingkatKesehatanPersen { get; set; }

        // Status Kesehatan (Baik, Sedang, Buruk)
        public string StatusKesehatan { get; set; } = string.Empty;

        // Detail Mortalitas Terbaru
        public List<MortalitasDetailDto>? MortalitasTerbaru { get; set; }
    }

    public class MortalitasDetailDto
    {
        public DateTime TanggalKematian { get; set; }
        public int JumlahKematian { get; set; }
        public string PenyebabKematian { get; set; } = string.Empty;
    }
}

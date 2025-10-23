namespace SIMTernakAyam.DTOs.Laporan
{
    /// <summary>
    /// DTO untuk analisis produktivitas petugas per kandang
    /// </summary>
    public class AnalisisProduktivitasDto
    {
        public Guid PetugasId { get; set; }
        public string NamaPetugas { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        // Kandang yang dikelola
        public List<ProduktivitasKandangDto>? KandangDikelola { get; set; }

        // Summary Performa
        public int TotalKandang { get; set; }
        public int TotalOperasional { get; set; }
        public int TotalAyamDikelola { get; set; }
        public int TotalMortalitas { get; set; }
        public decimal RataMortalitasPersen { get; set; }

        // Rating Performa (Sangat Baik, Baik, Cukup, Kurang)
        public string RatingPerforma { get; set; } = string.Empty;
        public decimal SkorProduktivitas { get; set; } // 0-100
    }

    public class ProduktivitasKandangDto
    {
        public Guid KandangId { get; set; }
        public string NamaKandang { get; set; } = string.Empty;
        public string Lokasi { get; set; } = string.Empty;
        public int Kapasitas { get; set; }

        // Statistik
        public int TotalAyam { get; set; }
        public int TotalOperasional { get; set; }
        public int TotalMortalitas { get; set; }
        public decimal PersentaseMortalitas { get; set; }
        public decimal TingkatPengisianPersen { get; set; } // TotalAyam / Kapasitas * 100

        // Kegiatan Terakhir
        public DateTime? TanggalOperasionalTerakhir { get; set; }
        public string? JenisKegiatanTerakhir { get; set; }
    }
}

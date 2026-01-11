namespace SIMTernakAyam.DTOs.Kandang
{
    /// <summary>
    /// DTO untuk detail kandang dengan history aktivitas lengkap
    /// </summary>
    public class KandangDetailDto
    {
        // Informasi Dasar Kandang
        public Guid Id { get; set; }
        public string NamaKandang { get; set; } = string.Empty;
        public int Kapasitas { get; set; }
        public string Lokasi { get; set; } = string.Empty;
        public Guid PetugasId { get; set; }
        public string? PetugasNama { get; set; }
        
        // Informasi Stok Ayam
        public int JumlahAyamTerisi { get; set; }
        public int KapasitasTersedia { get; set; }
        public decimal PersentaseTerisi { get; set; }
        public bool IsKandangPenuh { get; set; }
        public string StatusKapasitas { get; set; } = string.Empty;
        
        // Summary Aktivitas
        public int TotalAyamMasuk { get; set; }
        public int TotalPanen { get; set; }
        public int TotalMortalitas { get; set; }
        public int TotalOperasional { get; set; }
        
        // ? NEW: Informasi Ayam Sisa
        public List<AyamSisaDetailDto> AyamSisaList { get; set; } = new();
        public int TotalAyamSisa { get; set; }
        
        // History Aktivitas (sorted by date descending)
        public List<AyamHistoryDto> HistoryAyamMasuk { get; set; } = new();
        public List<PanenHistoryDto> HistoryPanen { get; set; } = new();
        public List<MortalitasHistoryDto> HistoryMortalitas { get; set; } = new();
        public List<OperasionalHistoryDto> HistoryOperasional { get; set; } = new();
        
        public DateTime CreatedAt { get; set; }
        public DateTime UpdateAt { get; set; }
    }

    /// <summary>
    /// History ayam yang masuk ke kandang
    /// </summary>
    public class AyamHistoryDto
    {
        public Guid Id { get; set; }
        public DateTime TanggalMasuk { get; set; }
        public int JumlahMasuk { get; set; }
        public int SisaHidup { get; set; } // Sisa ayam dari batch ini yang masih hidup
    }

    /// <summary>
    /// ? NEW: Detail ayam sisa (ayam dari periode sebelumnya yang masih ada sisanya)
    /// </summary>
    public class AyamSisaDetailDto
    {
        public Guid Id { get; set; }
        public DateTime TanggalMasuk { get; set; }
        public int JumlahMasukAwal { get; set; }
        public int SisaHidup { get; set; }
        public string? AlasanSisa { get; set; }
        public DateTime? TanggalDitandaiSisa { get; set; }
        public int UmurAyam { get; set; } // Umur dalam hari
        public bool PerluPerhatian { get; set; } // True jika umur > 60 hari
    }

    /// <summary>
    /// History panen di kandang
    /// </summary>
    public class PanenHistoryDto
    {
        public Guid Id { get; set; }
        public DateTime TanggalPanen { get; set; }
        public int JumlahEkorPanen { get; set; }
        public decimal BeratRataRata { get; set; }
        public decimal TotalBerat { get; set; }
        public string? Keterangan { get; set; }
        public string? NamaAyamBatch { get; set; } // Dari batch ayam mana
    }

    /// <summary>
    /// History mortalitas di kandang
    /// </summary>
    public class MortalitasHistoryDto
    {
        public Guid Id { get; set; }
        public DateTime TanggalKematian { get; set; }
        public int JumlahKematian { get; set; }
        public string PenyebabKematian { get; set; } = string.Empty;
        public string? NamaAyamBatch { get; set; } // Dari batch ayam mana
    }

    /// <summary>
    /// History operasional di kandang
    /// </summary>
    public class OperasionalHistoryDto
    {
        public Guid Id { get; set; }
        public DateTime Tanggal { get; set; }
        public string JenisKegiatan { get; set; } = string.Empty;
        public int Jumlah { get; set; }
        public string? Satuan { get; set; }
        public string? ItemNama { get; set; } // Nama pakan atau vaksin
        public string? PetugasNama { get; set; }
        public string? Keterangan { get; set; }
    }
}

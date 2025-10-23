namespace SIMTernakAyam.Models
{
    /// <summary>
    /// Model untuk jurnal harian petugas
    /// </summary>
    public class JurnalHarian : BaseModel
    {
        public Guid PetugasId { get; set; }
        public User Petugas { get; set; } = null!;

        public DateTime Tanggal { get; set; }
        public string JudulKegiatan { get; set; } = string.Empty;
        public string DeskripsiKegiatan { get; set; } = string.Empty;

        public TimeSpan WaktuMulai { get; set; }
        public TimeSpan WaktuSelesai { get; set; }

        // Relasi opsional ke Kandang
        public Guid? KandangId { get; set; }
        public Kandang? Kandang { get; set; }

        public string? Catatan { get; set; }

        // Path foto kegiatan (untuk file upload)
        public string? FotoKegiatan { get; set; }
    }
}

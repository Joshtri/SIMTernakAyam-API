using SIMTernakAyam.Enums;

namespace SIMTernakAyam.Models
{
    /// <summary>
    /// Model untuk tracking pemindahan/relokasi ayam antar kandang
    /// Digunakan untuk memindahkan ayam sakit ke kandang isolasi atau sebaliknya
    /// </summary>
    public class RelokasiAyam : BaseModel
    {
        // ========== KANDANG ASAL & TUJUAN ==========

        /// <summary>
        /// Kandang asal tempat ayam dipindahkan
        /// </summary>
        public Guid KandangAsalId { get; set; }
        public Kandang? KandangAsal { get; set; }

        /// <summary>
        /// Kandang tujuan tempat ayam akan dipindahkan
        /// </summary>
        public Guid KandangTujuanId { get; set; }
        public Kandang? KandangTujuan { get; set; }

        // ========== BATCH AYAM ==========

        /// <summary>
        /// Batch ayam asal yang dikurangi jumlahnya
        /// </summary>
        public Guid AyamAsalId { get; set; }
        public Ayam? AyamAsal { get; set; }

        /// <summary>
        /// Batch ayam tujuan yang dibuat baru di kandang tujuan
        /// Nullable karena dibuat otomatis saat relokasi
        /// </summary>
        public Guid? AyamTujuanId { get; set; }
        public Ayam? AyamTujuan { get; set; }

        // ========== DETAIL RELOKASI ==========

        /// <summary>
        /// Jumlah ekor ayam yang dipindahkan
        /// </summary>
        public int JumlahEkor { get; set; }

        /// <summary>
        /// Tanggal relokasi dilakukan
        /// </summary>
        public DateTime TanggalRelokasi { get; set; }

        /// <summary>
        /// Alasan/tujuan relokasi (Sakit, Karantina, Pulih, Lainnya)
        /// </summary>
        public AlasanRelokasiEnum AlasanRelokasi { get; set; }

        /// <summary>
        /// Status relokasi (Pending, Selesai, Dibatalkan)
        /// </summary>
        public StatusRelokasiEnum StatusRelokasi { get; set; } = StatusRelokasiEnum.Selesai;

        /// <summary>
        /// Catatan tambahan (diagnosa, treatment, dll)
        /// </summary>
        public string? Catatan { get; set; }

        // ========== PETUGAS ==========

        /// <summary>
        /// Petugas yang melakukan relokasi
        /// </summary>
        public Guid PetugasId { get; set; }
        public User? Petugas { get; set; }
    }
}

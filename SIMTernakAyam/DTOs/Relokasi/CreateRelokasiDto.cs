using System.ComponentModel.DataAnnotations;
using SIMTernakAyam.Enums;

namespace SIMTernakAyam.DTOs.Relokasi
{
    /// <summary>
    /// DTO untuk membuat relokasi ayam baru
    /// </summary>
    public class CreateRelokasiDto
    {
        /// <summary>
        /// ID kandang asal (tempat ayam dipindahkan)
        /// </summary>
        [Required(ErrorMessage = "Kandang asal wajib diisi.")]
        public Guid KandangAsalId { get; set; }

        /// <summary>
        /// ID kandang tujuan (tempat ayam akan dipindahkan)
        /// </summary>
        [Required(ErrorMessage = "Kandang tujuan wajib diisi.")]
        public Guid KandangTujuanId { get; set; }

        /// <summary>
        /// ID batch ayam yang akan dipindahkan
        /// </summary>
        [Required(ErrorMessage = "Batch ayam asal wajib diisi.")]
        public Guid AyamAsalId { get; set; }

        /// <summary>
        /// Jumlah ekor ayam yang akan dipindahkan
        /// </summary>
        [Required(ErrorMessage = "Jumlah ekor wajib diisi.")]
        [Range(1, 100000, ErrorMessage = "Jumlah ekor harus antara 1 sampai 100000.")]
        public int JumlahEkor { get; set; }

        /// <summary>
        /// Tanggal relokasi dilakukan
        /// </summary>
        [Required(ErrorMessage = "Tanggal relokasi wajib diisi.")]
        public DateTime TanggalRelokasi { get; set; }

        /// <summary>
        /// Alasan relokasi (Sakit, Karantina, Pulih, Lainnya)
        /// </summary>
        [Required(ErrorMessage = "Alasan relokasi wajib diisi.")]
        public AlasanRelokasiEnum AlasanRelokasi { get; set; }

        /// <summary>
        /// Catatan tambahan (diagnosa, treatment, dll)
        /// </summary>
        [StringLength(1000, ErrorMessage = "Catatan maksimal 1000 karakter.")]
        public string? Catatan { get; set; }
    }
}

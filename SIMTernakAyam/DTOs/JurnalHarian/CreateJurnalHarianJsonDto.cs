using System.ComponentModel.DataAnnotations;

namespace SIMTernakAyam.DTOs.JurnalHarian
{
    /// <summary>
    /// DTO untuk create jurnal harian via JSON (tanpa atau dengan foto base64)
    /// </summary>
    public class CreateJurnalHarianJsonDto
    {
        [Required(ErrorMessage = "Tanggal harus diisi")]
        public DateTime Tanggal { get; set; }

        [Required(ErrorMessage = "Judul kegiatan harus diisi")]
        [MaxLength(200, ErrorMessage = "Judul kegiatan maksimal 200 karakter")]
        public string JudulKegiatan { get; set; } = string.Empty;

        [Required(ErrorMessage = "Deskripsi kegiatan harus diisi")]
        [MaxLength(1000, ErrorMessage = "Deskripsi kegiatan maksimal 1000 karakter")]
        public string DeskripsiKegiatan { get; set; } = string.Empty;

        [Required(ErrorMessage = "Waktu mulai harus diisi")]
        [RegularExpression(@"^([0-1]?[0-9]|2[0-3]):[0-5][0-9](:[0-5][0-9])?$", ErrorMessage = "Format waktu mulai harus HH:mm atau HH:mm:ss")]
        public string WaktuMulai { get; set; } = string.Empty;

        [Required(ErrorMessage = "Waktu selesai harus diisi")]
        [RegularExpression(@"^([0-1]?[0-9]|2[0-3]):[0-5][0-9](:[0-5][0-9])?$", ErrorMessage = "Format waktu selesai harus HH:mm atau HH:mm:ss")]
        public string WaktuSelesai { get; set; } = string.Empty;

        public Guid? KandangId { get; set; }

        [MaxLength(500, ErrorMessage = "Catatan maksimal 500 karakter")]
        public string? Catatan { get; set; }

        /// <summary>
        /// Foto dalam format base64 (opsional)
        /// Format: "data:image/jpeg;base64,/9j/4AAQSkZJRg..." atau langsung base64 string
        /// </summary>
        public string? FotoKegiatanBase64 { get; set; }

        /// <summary>
        /// Nama file foto (opsional, jika tidak diisi akan auto-generate)
        /// </summary>
        public string? FotoKegiatanFileName { get; set; }
    }
}

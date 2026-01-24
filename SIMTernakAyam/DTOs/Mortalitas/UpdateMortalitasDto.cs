using System.ComponentModel.DataAnnotations;

namespace SIMTernakAyam.DTOs.Mortalitas
{
    public class UpdateMortalitasDto
    {
        [Required(ErrorMessage = "ID wajib diisi.")]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Ayam ID wajib diisi.")]
        public Guid AyamId { get; set; }

        [Required(ErrorMessage = "Tanggal kematian wajib diisi.")]
        public DateTime TanggalKematian { get; set; }

        /// <summary>
        /// Waktu kematian (jam:menit)
        /// Format: HH:mm (contoh: 14:30)
        /// </summary>
        [Required(ErrorMessage = "Waktu kematian wajib diisi.")]
        public TimeOnly WaktuKematian { get; set; }

        [Required(ErrorMessage = "Jumlah kematian wajib diisi.")]
        [Range(1, 1000000, ErrorMessage = "Jumlah kematian harus antara 1 sampai 1000000.")]
        public int JumlahKematian { get; set; }

        [Required(ErrorMessage = "Penyebab kematian wajib diisi.")]
        [StringLength(200, ErrorMessage = "Penyebab kematian maksimal 200 karakter.")]
        public string PenyebabKematian { get; set; } = string.Empty;

        /// <summary>
        /// Foto bukti mortalitas dalam format base64 (opsional)
        /// Format: "data:image/jpeg;base64,/9j/4AAQSkZJRg..." atau langsung base64 string
        /// </summary>
        public string? FotoMortalitasBase64 { get; set; }

        /// <summary>
        /// Nama file foto (opsional, jika tidak diisi akan auto-generate)
        /// </summary>
        public string? FotoMortalitasFileName { get; set; }
    }
}

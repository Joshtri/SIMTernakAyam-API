using System.ComponentModel.DataAnnotations;

namespace SIMTernakAyam.DTOs.Mortalitas
{
    /// <summary>
    /// DTO untuk create mortalitas - supports 2 modes
    /// 
    /// RECOMMENDED: Gunakan mode "manual-split" karena mortalitas tidak selalu FIFO.
    /// Mode "auto-fifo" sudah deprecated dan akan return error.
    /// </summary>
    public class CreateMortalitasDto
    {
        [Required(ErrorMessage = "Kandang ID wajib diisi.")]
        public Guid KandangId { get; set; }

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
        /// Mode pencatatan mortalitas
        /// - "auto-fifo": ?? DEPRECATED - Akan return error. Jangan gunakan!
        /// - "manual-split": ? RECOMMENDED - User tentukan sendiri berapa dari ayam lama dan baru
        /// </summary>
        [Required(ErrorMessage = "Mode input wajib diisi.")]
        public string Mode { get; set; } = "manual-split"; // Default: manual-split (changed from auto-fifo)

        /// <summary>
        /// Jumlah kematian dari ayam periode lama/sisa
        /// Hanya digunakan jika Mode = "manual-split"
        /// REQUIRED untuk mode manual-split
        /// </summary>
        [Range(0, 1000000, ErrorMessage = "Jumlah dari ayam lama harus antara 0 sampai 1000000.")]
        public int? JumlahDariAyamLama { get; set; }

        /// <summary>
        /// Jumlah kematian dari ayam periode baru
        /// Hanya digunakan jika Mode = "manual-split"
        /// REQUIRED untuk mode manual-split
        /// </summary>
        [Range(0, 1000000, ErrorMessage = "Jumlah dari ayam baru harus antara 0 sampai 1000000.")]
        public int? JumlahDariAyamBaru { get; set; }

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

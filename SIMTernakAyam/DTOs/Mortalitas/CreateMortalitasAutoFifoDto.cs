using System.ComponentModel.DataAnnotations;

namespace SIMTernakAyam.DTOs.Mortalitas
{
    /// <summary>
    /// ?? DEPRECATED - DTO ini sudah tidak menggunakan Auto FIFO lagi.
    /// Mortalitas tidak selalu First In First Out (yang mati bisa ayam baru atau lama).
    /// 
    /// Silakan gunakan mode "manual-split" di CreateMortalitasDto untuk menentukan distribusi manual.
    /// 
    /// Endpoint ini tetap tersedia untuk backward compatibility, tapi akan return error message.
    /// </summary>
    [Obsolete("Auto FIFO tidak digunakan lagi untuk mortalitas. Gunakan CreateMortalitasDto dengan mode 'manual-split'.")]
    public class CreateMortalitasAutoFifoDto
    {
        [Required(ErrorMessage = "Kandang ID wajib diisi.")]
        public Guid KandangId { get; set; }

        [Required(ErrorMessage = "Tanggal kematian wajib diisi.")]
        public DateTime TanggalKematian { get; set; }

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

using System.ComponentModel.DataAnnotations;
using SIMTernakAyam.Enums;

namespace SIMTernakAyam.DTOs.Relokasi
{
    /// <summary>
    /// DTO untuk update data relokasi
    /// </summary>
    public class UpdateRelokasiDto
    {
        [Required(ErrorMessage = "ID wajib diisi.")]
        public Guid Id { get; set; }

        /// <summary>
        /// Status relokasi (Pending, Selesai, Dibatalkan)
        /// </summary>
        public StatusRelokasiEnum? StatusRelokasi { get; set; }

        /// <summary>
        /// Catatan tambahan (diagnosa, treatment, dll)
        /// </summary>
        [StringLength(1000, ErrorMessage = "Catatan maksimal 1000 karakter.")]
        public string? Catatan { get; set; }
    }
}

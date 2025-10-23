using System.ComponentModel.DataAnnotations;

namespace SIMTernakAyam.DTOs.Biaya
{
    public class CreateBiayaDto
    {
        [Required(ErrorMessage = "Jenis biaya wajib diisi.")]
        [StringLength(100, ErrorMessage = "Jenis biaya maksimal 100 karakter.")]
        public string JenisBiaya { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tanggal wajib diisi.")]
        public DateTime Tanggal { get; set; }

        [Required(ErrorMessage = "Jumlah wajib diisi.")]
        [Range(0.01, 999999999.99, ErrorMessage = "Jumlah harus lebih dari 0.")]
        public decimal Jumlah { get; set; }

        [Required(ErrorMessage = "Petugas ID wajib diisi.")]
        public Guid PetugasId { get; set; }

        public Guid? OperasionalId { get; set; }

        /// <summary>
        /// ID Kandang untuk biaya listrik dan air (opsional)
        /// </summary>
        public Guid? KandangId { get; set; }

        [StringLength(500, ErrorMessage = "URL bukti maksimal 500 karakter.")]
        public string? BuktiUrl { get; set; }

        [StringLength(1000, ErrorMessage = "Catatan maksimal 1000 karakter.")]
        public string? Catatan { get; set; }

        /// <summary>
        /// Bulan untuk biaya recurring (1-12), wajib untuk Listrik dan Air
        /// </summary>
        [Range(1, 12, ErrorMessage = "Bulan harus antara 1-12.")]
        public int? Bulan { get; set; }

        /// <summary>
        /// Tahun untuk biaya recurring, wajib untuk Listrik dan Air
        /// </summary>
        [Range(2000, 2100, ErrorMessage = "Tahun harus antara 2000-2100.")]
        public int? Tahun { get; set; }
    }
}

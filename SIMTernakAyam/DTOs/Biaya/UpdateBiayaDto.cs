using System.ComponentModel.DataAnnotations;
using SIMTernakAyam.Enums;

namespace SIMTernakAyam.DTOs.Biaya
{
    public class UpdateBiayaDto
    {
        [Required(ErrorMessage = "ID wajib diisi.")]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Jenis biaya wajib diisi.")]
        [StringLength(100, ErrorMessage = "Jenis biaya maksimal 100 karakter.")]
        public string JenisBiaya { get; set; } = string.Empty;

        /// <summary>
        /// Kategori biaya: Pengeluaran Operasional atau Pembelian
        /// </summary>
        public KategoriBiayaEnum KategoriBiaya { get; set; } = KategoriBiayaEnum.PengeluaranOperasional;

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

        // Base64 bukti (replaces URL)
        public string? BuktiBase64 { get; set; }

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

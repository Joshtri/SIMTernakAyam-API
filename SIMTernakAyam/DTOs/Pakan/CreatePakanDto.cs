using System.ComponentModel.DataAnnotations;

namespace SIMTernakAyam.DTOs.Pakan
{
    public class CreatePakanDto
    {
        [Required(ErrorMessage = "Nama pakan wajib diisi.")]
        [StringLength(100, ErrorMessage = "Nama pakan maksimal 100 karakter.")]
        public string NamaPakan { get; set; } = string.Empty;

        [Required(ErrorMessage = "Stok wajib diisi.")]
        [Range(0, double.MaxValue, ErrorMessage = "Stok harus 0 atau lebih.")]
        public decimal StokKg { get; set; }

        [Required(ErrorMessage = "Bulan wajib diisi.")]
        [Range(1, 12, ErrorMessage = "Bulan harus antara 1-12.")]
        public int Bulan { get; set; }

        [Required(ErrorMessage = "Tahun wajib diisi.")]
        [Range(2000, 2100, ErrorMessage = "Tahun harus antara 2000-2100.")]
        public int Tahun { get; set; }
    }
}
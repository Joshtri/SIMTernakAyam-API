using System.ComponentModel.DataAnnotations;

namespace SIMTernakAyam.DTOs.Pakan
{
    public class CreatePakanDto
    {
        [Required(ErrorMessage = "Nama pakan wajib diisi.")]
        [StringLength(100, ErrorMessage = "Nama pakan maksimal 100 karakter.")]
        public string NamaPakan { get; set; } = string.Empty;

        [Required(ErrorMessage = "Stok wajib diisi.")]
        [Range(0, int.MaxValue, ErrorMessage = "Stok harus 0 atau lebih.")]
        public int Stok { get; set; }
    }
}
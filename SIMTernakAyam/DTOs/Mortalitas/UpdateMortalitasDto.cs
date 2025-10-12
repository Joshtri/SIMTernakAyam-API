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

        [Required(ErrorMessage = "Jumlah kematian wajib diisi.")]
        [Range(1, 1000000, ErrorMessage = "Jumlah kematian harus antara 1 sampai 1000000.")]
        public int JumlahKematian { get; set; }

        [Required(ErrorMessage = "Penyebab kematian wajib diisi.")]
        [StringLength(200, ErrorMessage = "Penyebab kematian maksimal 200 karakter.")]
        public string PenyebabKematian { get; set; } = string.Empty;
    }
}

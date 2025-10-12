using System.ComponentModel.DataAnnotations;

namespace SIMTernakAyam.DTOs.Biaya
{
    public class UpdateBiayaDto
    {
        [Required(ErrorMessage = "ID wajib diisi.")]
        public Guid Id { get; set; }

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

        [StringLength(500, ErrorMessage = "URL bukti maksimal 500 karakter.")]
        public string? BuktiUrl { get; set; }
    }
}

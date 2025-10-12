using System.ComponentModel.DataAnnotations;

namespace SIMTernakAyam.DTOs.Kandang
{
    public class CreateKandangDto
    {
        [Required(ErrorMessage = "Nama kandang wajib diisi.")]
        [StringLength(100, ErrorMessage = "Nama kandang maksimal 100 karakter.")]
        public string NamaKandang { get; set; } = string.Empty;

        [Required(ErrorMessage = "Kapasitas wajib diisi.")]
        [Range(1, 100000, ErrorMessage = "Kapasitas harus antara 1 sampai 100000.")]
        public int Kapasitas { get; set; }

        [Required(ErrorMessage = "Lokasi wajib diisi.")]
        [StringLength(200, ErrorMessage = "Lokasi maksimal 200 karakter.")]
        public string Lokasi { get; set; } = string.Empty;

        [Required(ErrorMessage = "Petugas ID wajib diisi.")]
        public Guid PetugasId { get; set; }
    }
}

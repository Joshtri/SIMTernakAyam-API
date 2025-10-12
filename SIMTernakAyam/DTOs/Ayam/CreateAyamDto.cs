using System.ComponentModel.DataAnnotations;

namespace SIMTernakAyam.DTOs.Ayam
{
    public class CreateAyamDto
    {
        [Required(ErrorMessage = "Kandang ID wajib diisi.")]
        public Guid KandangId { get; set; }

        [Required(ErrorMessage = "Tanggal masuk wajib diisi.")]
        public DateTime TanggalMasuk { get; set; }

        [Required(ErrorMessage = "Jumlah masuk wajib diisi.")]
        [Range(1, 1000000, ErrorMessage = "Jumlah masuk harus antara 1 sampai 1000000.")]
        public int JumlahMasuk { get; set; }
    }
}

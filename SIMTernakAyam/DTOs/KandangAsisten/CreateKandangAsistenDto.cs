using System.ComponentModel.DataAnnotations;

namespace SIMTernakAyam.DTOs.KandangAsisten
{
    public class CreateKandangAsistenDto
    {
        [Required(ErrorMessage = "Kandang ID wajib diisi")]
        public Guid KandangId { get; set; }

        [Required(ErrorMessage = "Asisten ID wajib diisi")]
        public Guid AsistenId { get; set; }

        [MaxLength(500, ErrorMessage = "Catatan maksimal 500 karakter")]
        public string? Catatan { get; set; }

        public bool IsAktif { get; set; } = true;
    }
}

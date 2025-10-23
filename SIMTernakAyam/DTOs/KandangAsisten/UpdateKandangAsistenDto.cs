using System.ComponentModel.DataAnnotations;

namespace SIMTernakAyam.DTOs.KandangAsisten
{
    public class UpdateKandangAsistenDto
    {
        [MaxLength(500, ErrorMessage = "Catatan maksimal 500 karakter")]
        public string? Catatan { get; set; }

        public bool IsAktif { get; set; }
    }
}

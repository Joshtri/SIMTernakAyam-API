using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;

namespace SIMTernakAyam.DTOs.JurnalHarian
{
    public class CreateJurnalHarianDto
    {
        [Required(ErrorMessage = "Tanggal harus diisi")]
        public DateTime Tanggal { get; set; }

        [Required(ErrorMessage = "Judul kegiatan harus diisi")]
        [MaxLength(200, ErrorMessage = "Judul kegiatan maksimal 200 karakter")]
        public string JudulKegiatan { get; set; } = string.Empty;

        [Required(ErrorMessage = "Deskripsi kegiatan harus diisi")]
        [MaxLength(1000, ErrorMessage = "Deskripsi kegiatan maksimal 1000 karakter")]
        public string DeskripsiKegiatan { get; set; } = string.Empty;

        // Untuk JSON input (format: "08:00" atau "08:00:00")
        [Required(ErrorMessage = "Waktu mulai harus diisi")]
        [RegularExpression(@"^([0-1]?[0-9]|2[0-3]):[0-5][0-9](:[0-5][0-9])?$", ErrorMessage = "Format waktu mulai harus HH:mm atau HH:mm:ss")]
        public string WaktuMulai { get; set; } = string.Empty;

        [Required(ErrorMessage = "Waktu selesai harus diisi")]
        [RegularExpression(@"^([0-1]?[0-9]|2[0-3]):[0-5][0-9](:[0-5][0-9])?$", ErrorMessage = "Format waktu selesai harus HH:mm atau HH:mm:ss")]
        public string WaktuSelesai { get; set; } = string.Empty;

        // Properties untuk backward compatibility (digunakan internal)
        [JsonIgnore]
        public TimeSpan WaktuMulaiTimeSpan
        {
            get
            {
                if (TimeSpan.TryParse(WaktuMulai, out var result))
                    return result;
                return TimeSpan.Zero;
            }
        }

        [JsonIgnore]
        public TimeSpan WaktuSelesaiTimeSpan
        {
            get
            {
                if (TimeSpan.TryParse(WaktuSelesai, out var result))
                    return result;
                return TimeSpan.Zero;
            }
        }

        public Guid? KandangId { get; set; }

        [MaxLength(500, ErrorMessage = "Catatan maksimal 500 karakter")]
        public string? Catatan { get; set; }

        public IFormFile? FotoKegiatan { get; set; }
    }
}

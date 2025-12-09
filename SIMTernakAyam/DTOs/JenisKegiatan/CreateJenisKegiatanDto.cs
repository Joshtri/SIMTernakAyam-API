using System.ComponentModel.DataAnnotations;

namespace SIMTernakAyam.DTOs.JenisKegiatan
{
    public class CreateJenisKegiatanDto
    {
        [Required(ErrorMessage = "Nama kegiatan wajib diisi.")]
        [StringLength(100, ErrorMessage = "Nama kegiatan maksimal 100 karakter.")]
        public string NamaKegiatan { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Deskripsi maksimal 500 karakter.")]
        public string? Deskripsi { get; set; }
    }
}
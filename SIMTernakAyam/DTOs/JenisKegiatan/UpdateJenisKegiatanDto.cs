using System.ComponentModel.DataAnnotations;

namespace SIMTernakAyam.DTOs.JenisKegiatan
{
    public class UpdateJenisKegiatanDto
    {
        [Required(ErrorMessage = "ID wajib diisi.")]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Nama kegiatan wajib diisi.")]
        [StringLength(100, ErrorMessage = "Nama kegiatan maksimal 100 karakter.")]
        public string NamaKegiatan { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Deskripsi maksimal 500 karakter.")]
        public string? Deskripsi { get; set; }

        //[StringLength(20, ErrorMessage = "Satuan maksimal 20 karakter.")]
        //public string? Satuan { get; set; }

        //[Range(0, 999999999.99, ErrorMessage = "Biaya default harus 0 atau lebih.")]
        //public decimal? BiayaDefault { get; set; }
    }
}
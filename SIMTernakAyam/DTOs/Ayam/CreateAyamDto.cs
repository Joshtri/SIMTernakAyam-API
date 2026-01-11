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

        /// <summary>
        /// Flag untuk force input meskipun ada sisa ayam dari periode sebelumnya
        /// Jika true, ayam lama akan di-flag sebagai IsAyamSisa
        /// </summary>
        public bool ForceInput { get; set; } = false;

        /// <summary>
        /// Alasan kenapa ada sisa ayam dan tetap input ayam baru
        /// Wajib diisi jika ForceInput = true
        /// </summary>
        [MaxLength(500, ErrorMessage = "Alasan maksimal 500 karakter.")]
        public string? AlasanInput { get; set; }
    }
}

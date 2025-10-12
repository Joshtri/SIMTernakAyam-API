using System.ComponentModel.DataAnnotations;

namespace SIMTernakAyam.DTOs.Panen
{
    public class UpdatePanenDto
    {
        [Required(ErrorMessage = "ID wajib diisi.")]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Ayam ID wajib diisi.")]
        public Guid AyamId { get; set; }

        [Required(ErrorMessage = "Tanggal panen wajib diisi.")]
        public DateTime TanggalPanen { get; set; }

        [Required(ErrorMessage = "Jumlah ekor panen wajib diisi.")]
        [Range(1, 100000, ErrorMessage = "Jumlah ekor panen harus antara 1 sampai 100000.")]
        public int JumlahEkorPanen { get; set; }

        [Required(ErrorMessage = "Berat rata-rata wajib diisi.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Berat rata-rata harus lebih dari 0.")]
        public decimal BeratRataRata { get; set; }
    }
}

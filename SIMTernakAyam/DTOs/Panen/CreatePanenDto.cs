using System.ComponentModel.DataAnnotations;

namespace SIMTernakAyam.DTOs.Panen
{
    public class CreatePanenDto
    {
        [Required(ErrorMessage = "Ayam ID wajib diisi.")]
        public Guid AyamId { get; set; }

        [Required(ErrorMessage = "Tanggal panen wajib diisi.")]
        public DateTime TanggalPanen { get; set; }

        [Required(ErrorMessage = "Jumlah ekor panen wajib diisi.")]
        [Range(1, 1000000, ErrorMessage = "Jumlah ekor panen harus antara 1 sampai 1000000. Pastikan tidak melebihi stok ayam yang tersedia.")]
        public int JumlahEkorPanen { get; set; }

        [Required(ErrorMessage = "Berat rata-rata wajib diisi.")]
        [Range(0.01, 100.00, ErrorMessage = "Berat rata-rata harus antara 0.01 sampai 100.00 kg.")]
        public decimal BeratRataRata { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace SIMTernakAyam.DTOs.Panen
{
    /// <summary>
    /// DTO untuk create panen dengan multiple modes
    /// Mode:
    /// - "auto-fifo": Sistem otomatis pilih ayam berdasarkan FIFO
    /// - "manual-split": User tentukan sendiri berapa dari ayam lama dan baru
    /// </summary>
    public class CreatePanenWithModeDto
    {
        [Required(ErrorMessage = "Kandang ID wajib diisi.")]
        public Guid KandangId { get; set; }

        [Required(ErrorMessage = "Tanggal panen wajib diisi.")]
        public DateTime TanggalPanen { get; set; }

        [Required(ErrorMessage = "Jumlah ekor panen wajib diisi.")]
        [Range(1, 1000000, ErrorMessage = "Jumlah ekor panen harus antara 1 sampai 1000000.")]
        public int JumlahEkorPanen { get; set; }

        [Required(ErrorMessage = "Berat rata-rata wajib diisi.")]
        [Range(0.01, 100.00, ErrorMessage = "Berat rata-rata harus antara 0.01 sampai 100.00 kg.")]
        public decimal BeratRataRata { get; set; }

        /// <summary>
        /// Mode pencatatan panen
        /// - "auto-fifo": Sistem otomatis pilih ayam terbaru (LIFO in practice)
        /// - "manual-split": ? RECOMMENDED - User tentukan sendiri berapa dari ayam lama dan baru
        /// </summary>
        [Required(ErrorMessage = "Mode input wajib diisi.")]
        public string Mode { get; set; } = "manual-split"; // Default: manual-split

        /// <summary>
        /// Jumlah panen dari ayam periode lama/sisa
        /// REQUIRED jika Mode = "manual-split"
        /// </summary>
        [Range(0, 1000000, ErrorMessage = "Jumlah dari ayam lama harus antara 0 sampai 1000000.")]
        public int? JumlahDariAyamLama { get; set; }

        /// <summary>
        /// Jumlah panen dari ayam periode baru
        /// REQUIRED jika Mode = "manual-split"
        /// </summary>
        [Range(0, 1000000, ErrorMessage = "Jumlah dari ayam baru harus antara 0 sampai 1000000.")]
        public int? JumlahDariAyamBaru { get; set; }
    }
}

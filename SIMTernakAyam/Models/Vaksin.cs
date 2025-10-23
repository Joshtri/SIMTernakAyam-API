using SIMTernakAyam.Enums;

namespace SIMTernakAyam.Models
{
    /// <summary>
    /// Model untuk data Vaksin dan Vitamin (digabung karena satuan sama: Dosis)
    /// </summary>
    public class Vaksin : BaseModel
    {
        public string NamaVaksin { get; set; } = string.Empty;
        public int Stok { get; set; } // Stok dalam dosis
        public int Bulan { get; set; } // 1-12 (Januari-Desember)
        public int Tahun { get; set; } // Contoh: 2024, 2025

        /// <summary>
        /// Tipe: Vaksin atau Vitamin
        /// Default: Vaksin untuk backward compatibility
        /// </summary>
        public VaksinVitaminTypeEnum Tipe { get; set; } = VaksinVitaminTypeEnum.Vaksin;
    }

}

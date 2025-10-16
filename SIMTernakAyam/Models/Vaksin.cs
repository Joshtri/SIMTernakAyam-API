namespace SIMTernakAyam.Models
{
    public class Vaksin : BaseModel
    {
        public string NamaVaksin { get; set; } = string.Empty;
        public int Stok { get; set; } // Stok dalam dosis
        public int Bulan { get; set; } // 1-12 (Januari-Desember)
        public int Tahun { get; set; } // Contoh: 2024, 2025
    }

}

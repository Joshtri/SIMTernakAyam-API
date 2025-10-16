namespace SIMTernakAyam.Models
{
    public class Pakan : BaseModel
    {
        public string NamaPakan { get; set; } = string.Empty;
        public decimal StokKg { get; set; } // Stok dalam satuan kilogram
        public int Bulan { get; set; } // 1-12 (Januari-Desember)
        public int Tahun { get; set; } // Contoh: 2024, 2025
    }

}
    
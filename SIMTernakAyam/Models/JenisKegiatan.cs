namespace SIMTernakAyam.Models
{
    public class JenisKegiatan : BaseModel
    {
        public string NamaKegiatan { get; set; } = string.Empty; // contoh: "Pakan", "Vaksin", "Listrik"
        public string? Deskripsi { get; set; }   // opsional
        public string? Satuan { get; set; }      // contoh: "Kg", "Liter", "Dosis"
        public decimal? BiayaDefault { get; set; } // opsional
    }
}

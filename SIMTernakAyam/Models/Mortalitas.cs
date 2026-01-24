namespace SIMTernakAyam.Models
{
    public class Mortalitas : BaseModel
    {
        public Guid AyamId { get; set; }       // Relasi ke entri ayam masuk
        public Ayam Ayam { get; set; }

        public DateTime TanggalKematian { get; set; }
        
        /// <summary>
        /// ✅ BARU: Waktu kematian (jam:menit)
        /// Format: HH:mm (contoh: 14:30)
        /// </summary>
        public TimeOnly WaktuKematian { get; set; }
        
        public int JumlahKematian { get; set; }
        public string PenyebabKematian { get; set; }
        public string? FotoMortalitas { get; set; }  // Path foto bukti mortalitas
    }
}

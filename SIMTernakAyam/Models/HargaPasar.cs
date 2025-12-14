namespace SIMTernakAyam.Models
{
    /// <summary>
    /// Model untuk menyimpan data harga pasar ayam yang akan digunakan
    /// sebagai referensi dalam perhitungan keuntungan
    /// </summary>
    public class HargaPasar : BaseModel
    {
        /// <summary>
        /// Harga per kilogram ayam hidup (Rp/kg)
        /// </summary>
        public decimal HargaPerKg { get; set; }
        
        /// <summary>
        /// Tanggal mulai berlaku harga ini
        /// </summary>
        public DateTime TanggalMulai { get; set; }
        
        /// <summary>
        /// Tanggal berakhir harga ini (nullable, jika null berarti masih berlaku)
        /// </summary>
        public DateTime? TanggalBerakhir { get; set; }
        
        /// <summary>
        /// Keterangan tambahan tentang harga
        /// </summary>
        public string? Keterangan { get; set; }
        
        /// <summary>
        /// Status aktif harga (true = aktif, false = tidak aktif)
        /// </summary>
        public bool IsAktif { get; set; } = true;
        
        /// <summary>
        /// Wilayah/area harga (opsional untuk diferensiasi regional)
        /// </summary>
        public string? Wilayah { get; set; }
    }
}
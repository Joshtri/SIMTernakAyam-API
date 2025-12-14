namespace SIMTernakAyam.DTOs.HargaPasar
{
    /// <summary>
    /// DTO untuk membuat harga pasar baru
    /// </summary>
    public class CreateHargaPasarDto
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
        /// Tanggal berakhir harga ini (opsional)
        /// </summary>
        public DateTime? TanggalBerakhir { get; set; }
        
        /// <summary>
        /// Keterangan tambahan tentang harga
        /// </summary>
        public string? Keterangan { get; set; }
        
        /// <summary>
        /// Wilayah/area harga (opsional)
        /// </summary>
        public string? Wilayah { get; set; }
        
        /// <summary>
        /// Apakah otomatis menonaktifkan harga sebelumnya
        /// </summary>
        public bool AutoDeactivatePrevious { get; set; } = true;
    }
}
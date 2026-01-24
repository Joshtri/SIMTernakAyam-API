namespace SIMTernakAyam.DTOs.HargaPasar
{
    /// <summary>
    /// DTO untuk mengupdate harga pasar
    /// </summary>
    public class UpdateHargaPasarDto
    {
        /// <summary>
        /// Harga per ekor ayam hidup (Rp/ekor)
        /// </summary>
        public decimal HargaPerEkor { get; set; }
        
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
        /// Status aktif harga
        /// </summary>
        public bool IsAktif { get; set; }
    }
}
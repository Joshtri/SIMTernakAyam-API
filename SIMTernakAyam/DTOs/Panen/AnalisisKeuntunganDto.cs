namespace SIMTernakAyam.DTOs.Panen
{
    /// <summary>
    /// DTO untuk analisis keuntungan panen
    /// </summary>
    public class AnalisisKeuntunganDto
    {
        /// <summary>
        /// Data panen
        /// </summary>
        public Guid PanenId { get; set; }
        public DateTime TanggalPanen { get; set; }
        public int JumlahAyam { get; set; }
        public decimal BeratRataRata { get; set; }
        public decimal TotalBeratKg { get; set; }
        
        /// <summary>
        /// Data harga pasar
        /// </summary>
        public decimal HargaPasarPerKg { get; set; }
        public DateTime TanggalHargaPasar { get; set; }
        public string? WilayahHarga { get; set; }
        
        /// <summary>
        /// Perhitungan keuntungan
        /// </summary>
        public decimal PendapatanKotor { get; set; }
        public decimal TotalBiayaOperasional { get; set; }
        public decimal KeuntunganBersih { get; set; }
        public decimal MarginKeuntungan { get; set; } // dalam persen
        
        /// <summary>
        /// Detail biaya
        /// </summary>
        public decimal BiayaPakan { get; set; }
        public decimal BiayaVaksin { get; set; }
        public decimal BiayaLainnya { get; set; }
        
        /// <summary>
        /// Analisis tambahan
        /// </summary>
        public string StatusKeuntungan { get; set; } = string.Empty; // Untung, Rugi, Impas
        public decimal ROI { get; set; } // Return on Investment
        public decimal HargaPokokProduksi { get; set; } // per kg
        
        /// <summary>
        /// Format untuk display
        /// </summary>
        public string PendapatanKotorFormatted => $"Rp {PendapatanKotor:N0}";
        public string TotalBiayaFormatted => $"Rp {TotalBiayaOperasional:N0}";
        public string KeuntunganBersihFormatted => $"Rp {KeuntunganBersih:N0}";
        public string MarginKeuntunganFormatted => $"{MarginKeuntungan:F2}%";
        public string ROIFormatted => $"{ROI:F2}%";
        public string HargaPokokProduksiFormatted => $"Rp {HargaPokokProduksi:N0}";
    }
}
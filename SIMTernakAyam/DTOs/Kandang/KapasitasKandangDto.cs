namespace SIMTernakAyam.DTOs.Kandang
{
    /// <summary>
    /// DTO untuk informasi kapasitas dan sisa ayam di kandang
    /// </summary>
    public class KapasitasKandangDto
    {
        public Guid KandangId { get; set; }
        public string NamaKandang { get; set; } = string.Empty;
        public int KapasitasKandang { get; set; }

        /// <summary>
        /// Total ayam hidup saat ini (belum dipanen, belum mati)
        /// </summary>
        public int TotalAyamHidup { get; set; }

        /// <summary>
        /// Jumlah ayam sisa dari periode sebelumnya
        /// </summary>
        public int SisaAyamDariPeriodeSebelumnya { get; set; }

        /// <summary>
        /// Kapasitas yang masih tersedia untuk input ayam baru
        /// </summary>
        public int KapasitasTersedia { get; set; }

        /// <summary>
        /// Periode ayam sisa (jika ada)
        /// </summary>
        public string? PeriodeAyamSisa { get; set; }

        /// <summary>
        /// Flag apakah ada sisa ayam dari periode sebelumnya
        /// </summary>
        public bool AdaSisaAyam { get; set; }

        /// <summary>
        /// Persentase pengisian kandang
        /// </summary>
        public decimal PersentasePengisian { get; set; }
    }
}

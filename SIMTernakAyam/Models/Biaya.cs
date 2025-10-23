namespace SIMTernakAyam.Models
{
    public class Biaya : BaseModel
    {
        public string JenisBiaya { get; set; } // contoh: "Pakan", "Vaksin", "Listrik", "Air"
        public DateTime Tanggal { get; set; }
        public decimal Jumlah { get; set; }

        public Guid PetugasId { get; set; }
        public User Petugas { get; set; }

        // Opsional: relasi ke Operasional
        public Guid? OperasionalId { get; set; }
        public Operasional? Operasional { get; set; }

        // Opsional: relasi ke Kandang untuk biaya listrik dan air per kandang
        public Guid? KandangId { get; set; }
        public Kandang? Kandang { get; set; }

        public string? BuktiUrl { get; set; }

        /// <summary>
        /// Catatan tambahan untuk biaya
        /// </summary>
        public string? Catatan { get; set; }

        /// <summary>
        /// Bulan untuk biaya recurring (Listrik, Air)
        /// </summary>
        public int? Bulan { get; set; }

        /// <summary>
        /// Tahun untuk biaya recurring (Listrik, Air)
        /// </summary>
        public int? Tahun { get; set; }
    }

}

namespace SIMTernakAyam.Models
{
    /// <summary>
    /// Junction table untuk relasi Many-to-Many antara Kandang dan Asisten (User)
    /// Menyimpan daftar asisten yang dapat menggantikan petugas utama jika tidak hadir
    /// </summary>
    public class KandangAsisten : BaseModel
    {
        public Guid KandangId { get; set; }
        public Kandang Kandang { get; set; }

        public Guid AsistenId { get; set; }
        public User Asisten { get; set; }

        /// <summary>
        /// Catatan atau keterangan tambahan untuk asisten ini
        /// </summary>
        public string? Catatan { get; set; }

        /// <summary>
        /// Status apakah asisten ini masih aktif
        /// </summary>
        public bool IsAktif { get; set; } = true;
    }
}

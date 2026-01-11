namespace SIMTernakAyam.Models
{
    public class Ayam : BaseModel
    {
        public Guid KandangId { get; set; }
        public Kandang? Kandang { get; set; } // Make nullable to prevent null reference
        public DateTime TanggalMasuk { get; set; }
        public int JumlahMasuk { get; set; }

        // Field untuk tracking ayam sisa dari periode sebelumnya
        public bool IsAyamSisa { get; set; } = false;
        public string? AlasanSisa { get; set; }
        public DateTime? TanggalDitandaiSisa { get; set; }

        // Petugas bisa diakses lewat Kandang.User
    }
}

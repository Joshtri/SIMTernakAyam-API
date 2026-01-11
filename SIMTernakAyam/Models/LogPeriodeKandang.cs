namespace SIMTernakAyam.Models
{
    /// <summary>
    /// Log history untuk tracking periode input ayam dan sisa ayam di kandang
    /// Digunakan untuk audit trail dan evaluasi manajemen kandang
    /// </summary>
    public class LogPeriodeKandang : BaseModel
    {
        public Guid KandangId { get; set; }
        public Kandang? Kandang { get; set; }

        public string Periode { get; set; } = string.Empty; // Format: "Februari 2026"
        public DateTime TanggalInput { get; set; }

        public int JumlahInputBaru { get; set; } // Ayam baru yang diinput periode ini
        public int SisaDariPeriodeSebelumnya { get; set; } // Ayam sisa dari periode sebelumnya
        public string? AlasanAdaSisa { get; set; } // Alasan kenapa ada sisa ayam

        public Guid PetugasId { get; set; }
        public User? Petugas { get; set; }

        // Untuk tracking kapasitas
        public int KapasitasKandang { get; set; }
        public int TotalAyamSetelahInput { get; set; } // Sisa + Baru
    }
}

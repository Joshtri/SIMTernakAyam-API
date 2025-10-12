namespace SIMTernakAyam.Models
{
    public class Biaya : BaseModel
    {
        public string JenisBiaya { get; set; } // contoh: "Pakan", "Vaksin", "Listrik"
        public DateTime Tanggal { get; set; }
        public decimal Jumlah { get; set; }

        public Guid PetugasId { get; set; }
        public User Petugas { get; set; }

        // Opsional: relasi ke Operasional
        public Guid? OperasionalId { get; set; }
        public Operasional? Operasional { get; set; }

        public string? BuktiUrl { get; set; }

    }

}

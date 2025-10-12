namespace SIMTernakAyam.Models
{
    public class Operasional : BaseModel
    {
        public Guid JenisKegiatanId { get; set; }
        public JenisKegiatan JenisKegiatan { get; set; }

        public DateTime Tanggal { get; set; }
        public int Jumlah { get; set; }

        public Guid PetugasId { get; set; }
        public User Petugas { get; set; }

        public Guid KandangId { get; set; }
        public Kandang Kandang { get; set; }

        // Opsional: relasi ke Pakan/Vaksin
        public Guid? PakanId { get; set; }
        public Pakan? Pakan { get; set; }

        public Guid? VaksinId { get; set; }
        public Vaksin? Vaksin { get; set; }
    }

}

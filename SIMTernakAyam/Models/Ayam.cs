namespace SIMTernakAyam.Models
{
    public class Ayam : BaseModel
    {

        public Guid KandangId { get; set; }
        public Kandang Kandang { get; set; }
        public DateTime TanggalMasuk { get; set; }
        public int JumlahMasuk { get; set; }

        // Petugas bisa diakses lewat Kandang.User

    }
}

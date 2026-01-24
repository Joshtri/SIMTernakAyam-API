using SIMTernakAyam.Enums;

namespace SIMTernakAyam.Models
{
    public class Kandang : BaseModel
    {
        public string NamaKandang { get; set; }
        public int Kapasitas { get; set; }
        public string Lokasi { get; set; }

        /// <summary>
        /// Tipe kandang: Normal (produksi) atau Isolasi (ayam sakit)
        /// </summary>
        public TipeKandangEnum TipeKandang { get; set; } = TipeKandangEnum.Normal;
        //public DateTime TanggalDibuat { get; set; }

        // jadi ini relasi banyak ke satu (many-to-one) ke User
        public Guid petugasId { get; set; }
        public User? User { get; set; } = null;

        public ICollection<Ayam>? Ayams { get; set; }

        // Relasi Many-to-Many dengan Asisten kandang
        public ICollection<KandangAsisten>? KandangAsistens { get; set; }

    }
}

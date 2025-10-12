namespace SIMTernakAyam.Models
{
    public class Panen : BaseModel
    {
        public Guid AyamId { get; set; }
        public Ayam Ayam { get; set; }

        public DateTime TanggalPanen { get; set; }
        public int JumlahEkorPanen { get; set; }
        public decimal BeratRataRata { get; set; }
    }

}

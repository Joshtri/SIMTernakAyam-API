namespace SIMTernakAyam.DTOs.Panen
{
    public class PanenResponseDto
    {
        public Guid Id { get; set; }
        public Guid AyamId { get; set; }
        public string? NamaKandang { get; set; }
        public DateTime TanggalPanen { get; set; }
        public int JumlahEkorPanen { get; set; }
        public decimal BeratRataRata { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdateAt { get; set; }

        public static PanenResponseDto FromEntity(Models.Panen panen)
        {
            return new PanenResponseDto
            {
                Id = panen.Id,
                AyamId = panen.AyamId,
                NamaKandang = panen.Ayam?.Kandang?.NamaKandang,
                TanggalPanen = panen.TanggalPanen,
                JumlahEkorPanen = panen.JumlahEkorPanen,
                BeratRataRata = panen.BeratRataRata,
                CreatedAt = panen.CreatedAt,
                UpdateAt = panen.UpdateAt
            };
        }

        public static List<PanenResponseDto> FromEntities(IEnumerable<Models.Panen> panens)
        {
            return panens.Select(FromEntity).ToList();
        }
    }
}

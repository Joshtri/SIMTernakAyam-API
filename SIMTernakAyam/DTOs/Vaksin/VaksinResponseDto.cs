namespace SIMTernakAyam.DTOs.Vaksin
{
    public class VaksinResponseDto
    {
        public Guid Id { get; set; }
        public string NamaVaksin { get; set; } = string.Empty;
        public int Stok { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdateAt { get; set; }

        public static VaksinResponseDto FromEntity(Models.Vaksin vaksin)
        {
            return new VaksinResponseDto
            {
                Id = vaksin.Id,
                NamaVaksin = vaksin.NamaVaksin,
                Stok = vaksin.Stok,
                CreatedAt = vaksin.CreatedAt,
                UpdateAt = vaksin.UpdateAt
            };
        }

        public static List<VaksinResponseDto> FromEntities(IEnumerable<Models.Vaksin> vaksins)
        {
            return vaksins.Select(FromEntity).ToList();
        }
    }
}
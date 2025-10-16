namespace SIMTernakAyam.DTOs.Vaksin
{
    public class VaksinResponseDto
    {
        public Guid Id { get; set; }
        public string NamaVaksin { get; set; } = string.Empty;
        public int Stok { get; set; }

        public int Bulan { get; set; } // 1-12 (Januari-Desember)
        public int Tahun { get; set; } // Contoh: 2024, 2025
        public DateTime CreatedAt { get; set; }
        public DateTime UpdateAt { get; set; }

        public static VaksinResponseDto FromEntity(Models.Vaksin vaksin)
        {
            return new VaksinResponseDto
            {
                Id = vaksin.Id,
                NamaVaksin = vaksin.NamaVaksin,
                Stok = vaksin.Stok,
                Bulan = vaksin.Bulan,
                Tahun = vaksin.Tahun,
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
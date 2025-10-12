namespace SIMTernakAyam.DTOs.Mortalitas
{
    public class MortalitasResponseDto
    {
        public Guid Id { get; set; }
        public Guid AyamId { get; set; }
        public DateTime TanggalKematian { get; set; }
        public int JumlahKematian { get; set; }
        public string PenyebabKematian { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdateAt { get; set; }

        public static MortalitasResponseDto FromEntity(Models.Mortalitas mortalitas)
        {
            return new MortalitasResponseDto
            {
                Id = mortalitas.Id,
                AyamId = mortalitas.AyamId,
                TanggalKematian = mortalitas.TanggalKematian,
                JumlahKematian = mortalitas.JumlahKematian,
                PenyebabKematian = mortalitas.PenyebabKematian,
                CreatedAt = mortalitas.CreatedAt,
                UpdateAt = mortalitas.UpdateAt
            };
        }

        public static List<MortalitasResponseDto> FromEntities(IEnumerable<Models.Mortalitas> mortalitasList)
        {
            return mortalitasList.Select(FromEntity).ToList();
        }
    }
}

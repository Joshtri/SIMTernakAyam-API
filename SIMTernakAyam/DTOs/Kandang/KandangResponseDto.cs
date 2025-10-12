namespace SIMTernakAyam.DTOs.Kandang
{
    public class KandangResponseDto
    {
        public Guid Id { get; set; }
        public string NamaKandang { get; set; } = string.Empty;
        public int Kapasitas { get; set; }
        public string Lokasi { get; set; } = string.Empty;
        public Guid PetugasId { get; set; }
        public string? PetugasNama { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdateAt { get; set; }

        public static KandangResponseDto FromEntity(Models.Kandang kandang)
        {
            return new KandangResponseDto
            {
                Id = kandang.Id,
                NamaKandang = kandang.NamaKandang,
                Kapasitas = kandang.Kapasitas,
                Lokasi = kandang.Lokasi,
                PetugasId = kandang.petugasId,
                PetugasNama = kandang.User?.FullName ?? kandang.User?.Username,
                CreatedAt = kandang.CreatedAt,
                UpdateAt = kandang.UpdateAt
            };
        }

        public static List<KandangResponseDto> FromEntities(IEnumerable<Models.Kandang> kandangs)
        {
            return kandangs.Select(FromEntity).ToList();
        }
    }
}

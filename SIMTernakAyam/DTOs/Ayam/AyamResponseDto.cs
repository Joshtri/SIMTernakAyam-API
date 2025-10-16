namespace SIMTernakAyam.DTOs.Ayam
{
    public class AyamResponseDto
    {
        public Guid Id { get; set; }
        public Guid KandangId { get; set; }
        public string? KandangNama { get; set; }
        public string? PetugasKandangNama { get; set; }
        public DateTime TanggalMasuk { get; set; }
        public int JumlahMasuk { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdateAt { get; set; }

        public static AyamResponseDto FromEntity(Models.Ayam ayam)
        {
            return new AyamResponseDto
            {
                Id = ayam.Id,
                KandangId = ayam.KandangId,
                KandangNama = ayam.Kandang?.NamaKandang,
                PetugasKandangNama = ayam.Kandang?.User?.FullName ?? ayam.Kandang?.User?.Username, // ✅ Mapping
                TanggalMasuk = ayam.TanggalMasuk,
                JumlahMasuk = ayam.JumlahMasuk,
                CreatedAt = ayam.CreatedAt,
                UpdateAt = ayam.UpdateAt
            };
        }

        public static List<AyamResponseDto> FromEntities(IEnumerable<Models.Ayam> ayams)
        {
            return ayams.Select(FromEntity).ToList();
        }
    }
}

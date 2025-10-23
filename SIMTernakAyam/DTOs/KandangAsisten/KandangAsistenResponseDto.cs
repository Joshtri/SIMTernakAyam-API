namespace SIMTernakAyam.DTOs.KandangAsisten
{
    public class KandangAsistenResponseDto
    {
        public Guid Id { get; set; }
        public Guid KandangId { get; set; }
        public string? KandangNama { get; set; }
        public Guid AsistenId { get; set; }
        public string? AsistenNama { get; set; }
        public string? AsistenEmail { get; set; }
        public string? AsistenNoWA { get; set; }
        public string? Catatan { get; set; }
        public bool IsAktif { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdateAt { get; set; }

        public static KandangAsistenResponseDto FromEntity(Models.KandangAsisten kandangAsisten)
        {
            return new KandangAsistenResponseDto
            {
                Id = kandangAsisten.Id,
                KandangId = kandangAsisten.KandangId,
                KandangNama = kandangAsisten.Kandang?.NamaKandang,
                AsistenId = kandangAsisten.AsistenId,
                AsistenNama = kandangAsisten.Asisten?.FullName,
                AsistenEmail = kandangAsisten.Asisten?.Email,
                AsistenNoWA = kandangAsisten.Asisten?.NoWA,
                Catatan = kandangAsisten.Catatan,
                IsAktif = kandangAsisten.IsAktif,
                CreatedAt = kandangAsisten.CreatedAt,
                UpdateAt = kandangAsisten.UpdateAt
            };
        }

        public static List<KandangAsistenResponseDto> FromEntities(IEnumerable<Models.KandangAsisten> kandangAsistens)
        {
            return kandangAsistens.Select(FromEntity).ToList();
        }
    }
}

namespace SIMTernakAyam.DTOs.JenisKegiatan
{
    public class JenisKegiatanResponseDto
    {
        public Guid Id { get; set; }
        public string NamaKegiatan { get; set; } = string.Empty;
        public string? Deskripsi { get; set; }
        //public string? Satuan { get; set; }
        //public decimal? BiayaDefault { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdateAt { get; set; }

        public static JenisKegiatanResponseDto FromEntity(Models.JenisKegiatan jenisKegiatan)
        {
            return new JenisKegiatanResponseDto
            {
                Id = jenisKegiatan.Id,
                NamaKegiatan = jenisKegiatan.NamaKegiatan,
                Deskripsi = jenisKegiatan.Deskripsi,
                //Satuan = jenisKegiatan.Satuan,
                //BiayaDefault = jenisKegiatan.BiayaDefault,
                CreatedAt = jenisKegiatan.CreatedAt,
                UpdateAt = jenisKegiatan.UpdateAt
            };
        }

        public static List<JenisKegiatanResponseDto> FromEntities(IEnumerable<Models.JenisKegiatan> jenisKegiatans)
        {
            return jenisKegiatans.Select(FromEntity).ToList();
        }
    }
}
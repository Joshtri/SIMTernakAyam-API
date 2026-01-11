namespace SIMTernakAyam.DTOs.KandangAsisten
{
    /// <summary>
    /// Response DTO untuk KandangAsisten dengan informasi ayam sisa
    /// </summary>
    public class KandangAsistenWithAyamSisaDto
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

        // Informasi Ayam Sisa
        public List<AyamSisaInfo>? AyamSisaList { get; set; }
        public int TotalAyamSisa { get; set; }

        public static KandangAsistenWithAyamSisaDto FromEntity(
            Models.KandangAsisten kandangAsisten, 
            IEnumerable<Models.Ayam> ayamSisaList)
        {
            var ayamSisaInfoList = ayamSisaList.Select(ayam => new AyamSisaInfo
            {
                Id = ayam.Id,
                TanggalMasuk = ayam.TanggalMasuk,
                JumlahMasukAwal = ayam.JumlahMasuk,
                AlasanSisa = ayam.AlasanSisa,
                TanggalDitandaiSisa = ayam.TanggalDitandaiSisa,
                IsAyamSisa = ayam.IsAyamSisa,
                UmurAyam = (DateTime.UtcNow - ayam.TanggalMasuk).Days
            }).ToList();

            return new KandangAsistenWithAyamSisaDto
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
                UpdateAt = kandangAsisten.UpdateAt,
                AyamSisaList = ayamSisaInfoList,
                TotalAyamSisa = ayamSisaInfoList.Sum(a => a.JumlahMasukAwal)
            };
        }

        public static List<KandangAsistenWithAyamSisaDto> FromEntities(
            IEnumerable<Models.KandangAsisten> kandangAsistens,
            Dictionary<Guid, IEnumerable<Models.Ayam>> ayamSisaByKandang)
        {
            return kandangAsistens.Select(ka => FromEntity(
                ka, 
                ayamSisaByKandang.GetValueOrDefault(ka.KandangId, Enumerable.Empty<Models.Ayam>())
            )).ToList();
        }
    }

    /// <summary>
    /// Informasi detail ayam sisa
    /// </summary>
    public class AyamSisaInfo
    {
        public Guid Id { get; set; }
        public DateTime TanggalMasuk { get; set; }
        public int JumlahMasukAwal { get; set; }
        public string? AlasanSisa { get; set; }
        public DateTime? TanggalDitandaiSisa { get; set; }
        public bool IsAyamSisa { get; set; }
        public int UmurAyam { get; set; } // Dalam hari
    }
}

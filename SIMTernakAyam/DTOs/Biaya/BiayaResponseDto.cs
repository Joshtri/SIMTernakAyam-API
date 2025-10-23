namespace SIMTernakAyam.DTOs.Biaya
{
    public class BiayaResponseDto
    {
        public Guid Id { get; set; }
        public string JenisBiaya { get; set; } = string.Empty;
        public DateTime Tanggal { get; set; }
        public decimal Jumlah { get; set; }
        public Guid PetugasId { get; set; }
        public string? PetugasNama { get; set; }
        public Guid? OperasionalId { get; set; }
        public Guid? KandangId { get; set; }
        public string? KandangNama { get; set; }
        public string? BuktiUrl { get; set; }
        public string? Catatan { get; set; }
        public int? Bulan { get; set; }
        public int? Tahun { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdateAt { get; set; }

        public static BiayaResponseDto FromEntity(Models.Biaya biaya)
        {
            return new BiayaResponseDto
            {
                Id = biaya.Id,
                JenisBiaya = biaya.JenisBiaya,
                Tanggal = biaya.Tanggal,
                Jumlah = biaya.Jumlah,
                PetugasId = biaya.PetugasId,
                PetugasNama = biaya.Petugas?.FullName ?? biaya.Petugas?.Username,
                OperasionalId = biaya.OperasionalId,
                KandangId = biaya.KandangId,
                KandangNama = biaya.Kandang?.NamaKandang,
                BuktiUrl = biaya.BuktiUrl,
                Catatan = biaya.Catatan,
                Bulan = biaya.Bulan,
                Tahun = biaya.Tahun,
                CreatedAt = biaya.CreatedAt,
                UpdateAt = biaya.UpdateAt
            };
        }

        public static List<BiayaResponseDto> FromEntities(IEnumerable<Models.Biaya> biayas)
        {
            return biayas.Select(FromEntity).ToList();
        }
    }
}

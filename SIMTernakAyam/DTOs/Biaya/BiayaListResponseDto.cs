using SIMTernakAyam.Enums;

namespace SIMTernakAyam.DTOs.Biaya
{
    public class BiayaListResponseDto
    {
        public Guid Id { get; set; }
        public string JenisBiaya { get; set; } = string.Empty; // This is the actual name/label
        public KategoriBiayaEnum KategoriBiaya { get; set; } // This is just for categorization
        public DateTime Tanggal { get; set; }
        public decimal Jumlah { get; set; }
        public Guid PetugasId { get; set; }
        public string? PetugasNama { get; set; }
        public Guid? OperasionalId { get; set; }
        public Guid? KandangId { get; set; }
        public string? KandangNama { get; set; }
        public string? Catatan { get; set; }
        public int? Bulan { get; set; }
        public int? Tahun { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdateAt { get; set; }

        public static BiayaListResponseDto FromEntity(Models.Biaya biaya)
        {
            return new BiayaListResponseDto
            {
                Id = biaya.Id,
                JenisBiaya = biaya.JenisBiaya, // e.g., "Pakan BR-1", "Vaksin ND", "Listrik", etc.
                KategoriBiaya = biaya.KategoriBiaya, // 0 = Pengeluaran Operasional, 1 = Pembelian
                Tanggal = biaya.Tanggal,
                Jumlah = biaya.Jumlah,
                PetugasId = biaya.PetugasId,
                PetugasNama = biaya.Petugas?.FullName ?? biaya.Petugas?.Username,
                OperasionalId = biaya.OperasionalId,
                KandangId = biaya.KandangId,
                KandangNama = biaya.Kandang?.NamaKandang,
                Catatan = biaya.Catatan,
                Bulan = biaya.Bulan,
                Tahun = biaya.Tahun,
                CreatedAt = biaya.CreatedAt,
                UpdateAt = biaya.UpdateAt
            };
        }

        public static List<BiayaListResponseDto> FromEntities(IEnumerable<Models.Biaya> biayas)
        {
            return biayas.Select(FromEntity).ToList();
        }
    }
}
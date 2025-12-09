namespace SIMTernakAyam.DTOs.Operasional
{
    public class OperasionalResponseDto
    {
        public Guid Id { get; set; }
        public Guid JenisKegiatanId { get; set; }
        public string? JenisKegiatanNama { get; set; }
        public DateTime Tanggal { get; set; }
        public int Jumlah { get; set; }
        public Guid PetugasId { get; set; }
        public string? PetugasNama { get; set; }
        public Guid KandangId { get; set; }
        public string? KandangNama { get; set; }
        public Guid? PakanId { get; set; }
        public string? PakanNama { get; set; }
        public Guid? VaksinId { get; set; }
        public string? VaksinNama { get; set; }
        public decimal? TotalBiaya { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdateAt { get; set; }

        public static OperasionalResponseDto FromEntity(Models.Operasional operasional)
        {
            var biayaPerUnit = operasional.JenisKegiatan?.BiayaDefault ?? 0m;
            return new OperasionalResponseDto
            {
                Id = operasional.Id,
                JenisKegiatanId = operasional.JenisKegiatanId,
                JenisKegiatanNama = operasional.JenisKegiatan?.NamaKegiatan,
                Tanggal = operasional.Tanggal,
                Jumlah = operasional.Jumlah,
                TotalBiaya = biayaPerUnit * operasional.Jumlah,
                PetugasId = operasional.PetugasId,
                PetugasNama = operasional.Petugas?.FullName ?? operasional.Petugas?.Username,
                KandangId = operasional.KandangId,
                KandangNama = operasional.Kandang?.NamaKandang,
                PakanId = operasional.PakanId,
                PakanNama = operasional.Pakan?.NamaPakan,
                VaksinId = operasional.VaksinId,
                VaksinNama = operasional.Vaksin?.NamaVaksin,
                CreatedAt = operasional.CreatedAt,
                UpdateAt = operasional.UpdateAt
            };
        }

        public static List<OperasionalResponseDto> FromEntities(IEnumerable<Models.Operasional> operasionals)
        {
            return operasionals.Select(FromEntity).ToList();
        }
    }
}

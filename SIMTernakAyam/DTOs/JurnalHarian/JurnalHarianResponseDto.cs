namespace SIMTernakAyam.DTOs.JurnalHarian
{
    public class JurnalHarianResponseDto
    {
        public Guid Id { get; set; }
        public Guid PetugasId { get; set; }
        public string NamaPetugas { get; set; } = string.Empty;
        public string UsernamePetugas { get; set; } = string.Empty;

        public DateTime Tanggal { get; set; }
        public string JudulKegiatan { get; set; } = string.Empty;
        public string DeskripsiKegiatan { get; set; } = string.Empty;

        public TimeSpan WaktuMulai { get; set; }
        public TimeSpan WaktuSelesai { get; set; }
        public TimeSpan DurasiKegiatan { get; set; }

        public Guid? KandangId { get; set; }
        public string? NamaKandang { get; set; }
        public string? LokasiKandang { get; set; }

        public string? Catatan { get; set; }
        public string? FotoKegiatan { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}

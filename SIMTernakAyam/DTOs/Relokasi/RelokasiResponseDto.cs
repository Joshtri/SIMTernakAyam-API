using SIMTernakAyam.Enums;
using SIMTernakAyam.Models;

namespace SIMTernakAyam.DTOs.Relokasi
{
    /// <summary>
    /// DTO untuk response data relokasi
    /// </summary>
    public class RelokasiResponseDto
    {
        public Guid Id { get; set; }

        // Kandang Asal
        public Guid KandangAsalId { get; set; }
        public string KandangAsalNama { get; set; } = string.Empty;
        public string KandangAsalTipe { get; set; } = string.Empty;

        // Kandang Tujuan
        public Guid KandangTujuanId { get; set; }
        public string KandangTujuanNama { get; set; } = string.Empty;
        public string KandangTujuanTipe { get; set; } = string.Empty;

        // Batch Ayam Asal
        public Guid AyamAsalId { get; set; }
        public int AyamAsalJumlahMasuk { get; set; }
        public DateTime AyamAsalTanggalMasuk { get; set; }

        // Batch Ayam Tujuan (dibuat otomatis)
        public Guid? AyamTujuanId { get; set; }

        // Detail Relokasi
        public int JumlahEkor { get; set; }
        public DateTime TanggalRelokasi { get; set; }

        // Alasan & Status
        public AlasanRelokasiEnum AlasanRelokasi { get; set; }
        public string AlasanRelokasiNama { get; set; } = string.Empty;
        public StatusRelokasiEnum StatusRelokasi { get; set; }
        public string StatusRelokasiNama { get; set; } = string.Empty;

        // Catatan
        public string? Catatan { get; set; }

        // Petugas
        public Guid PetugasId { get; set; }
        public string PetugasNama { get; set; } = string.Empty;

        // Timestamps
        public DateTime CreatedAt { get; set; }
        public DateTime UpdateAt { get; set; }

        /// <summary>
        /// Factory method untuk convert entity ke DTO
        /// </summary>
        public static RelokasiResponseDto FromEntity(RelokasiAyam relokasi)
        {
            return new RelokasiResponseDto
            {
                Id = relokasi.Id,

                // Kandang Asal
                KandangAsalId = relokasi.KandangAsalId,
                KandangAsalNama = relokasi.KandangAsal?.NamaKandang ?? string.Empty,
                KandangAsalTipe = relokasi.KandangAsal?.TipeKandang.ToString() ?? string.Empty,

                // Kandang Tujuan
                KandangTujuanId = relokasi.KandangTujuanId,
                KandangTujuanNama = relokasi.KandangTujuan?.NamaKandang ?? string.Empty,
                KandangTujuanTipe = relokasi.KandangTujuan?.TipeKandang.ToString() ?? string.Empty,

                // Batch Ayam Asal
                AyamAsalId = relokasi.AyamAsalId,
                AyamAsalJumlahMasuk = relokasi.AyamAsal?.JumlahMasuk ?? 0,
                AyamAsalTanggalMasuk = relokasi.AyamAsal?.TanggalMasuk ?? DateTime.MinValue,

                // Batch Ayam Tujuan
                AyamTujuanId = relokasi.AyamTujuanId,

                // Detail Relokasi
                JumlahEkor = relokasi.JumlahEkor,
                TanggalRelokasi = relokasi.TanggalRelokasi,

                // Alasan & Status
                AlasanRelokasi = relokasi.AlasanRelokasi,
                AlasanRelokasiNama = relokasi.AlasanRelokasi.ToString(),
                StatusRelokasi = relokasi.StatusRelokasi,
                StatusRelokasiNama = relokasi.StatusRelokasi.ToString(),

                // Catatan
                Catatan = relokasi.Catatan,

                // Petugas
                PetugasId = relokasi.PetugasId,
                PetugasNama = relokasi.Petugas?.FullName ?? relokasi.Petugas?.Username ?? string.Empty,

                // Timestamps
                CreatedAt = relokasi.CreatedAt,
                UpdateAt = relokasi.UpdateAt
            };
        }

        /// <summary>
        /// Factory method untuk convert list entities ke list DTOs
        /// </summary>
        public static List<RelokasiResponseDto> FromEntities(IEnumerable<RelokasiAyam> relokasis)
        {
            return relokasis.Select(r => FromEntity(r)).ToList();
        }
    }
}

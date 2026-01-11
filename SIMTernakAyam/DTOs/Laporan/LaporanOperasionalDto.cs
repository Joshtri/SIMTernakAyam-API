namespace SIMTernakAyam.DTOs.Laporan
{
    /// <summary>
    /// DTO untuk laporan operasional (mingguan/bulanan)
    /// </summary>
    public class LaporanOperasionalDto
    {
        public string Periode { get; set; } = string.Empty; // "Minggu 1 Oktober 2025" atau "Oktober 2025"
        public DateTime TanggalMulai { get; set; }
        public DateTime TanggalSelesai { get; set; }

        // Summary
        public int TotalOperasional { get; set; }
        public int TotalKandang { get; set; }
        public int TotalPetugas { get; set; }

        // Detail per Jenis Kegiatan
        public List<OperasionalPerJenisDto>? DetailPerJenis { get; set; }

        // Detail per Kandang
        public List<OperasionalPerKandangDto>? DetailPerKandang { get; set; }

        // Detail per Petugas
        public List<OperasionalPerPetugasDto>? DetailPerPetugas { get; set; }
    }

    public class OperasionalPerJenisDto
    {
        public string NamaJenisKegiatan { get; set; } = string.Empty;
        public int JumlahKegiatan { get; set; }
        public int TotalJumlah { get; set; }
    }

    public class OperasionalPerKandangDto
    {
        public Guid KandangId { get; set; }
        public string NamaKandang { get; set; } = string.Empty;
        public string Lokasi { get; set; } = string.Empty;
        public int JumlahOperasional { get; set; }
        public string NamaPetugas { get; set; } = string.Empty;
    }

    public class OperasionalPerPetugasDto
    {
        public string NamaPetugas { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public int JumlahOperasional { get; set; }
        public List<string>? KandangDikelola { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace SIMTernakAyam.DTOs.HargaPasar
{
    /// <summary>
    /// DTO untuk response estimasi keuntungan
    /// </summary>
    public class EstimasiKeuntunganDto
    {
        /// <summary>
        /// Total jumlah ayam
        /// </summary>
        public int TotalAyam { get; set; }

        /// <summary>
        /// Harga pasar per ekor (Rp/ekor)
        /// </summary>
        public decimal HargaPerEkor { get; set; }

        /// <summary>
        /// Total pendapatan estimasi (Total Ayam � Harga Per Ekor)
        /// </summary>
        public decimal TotalPendapatan { get; set; }

        /// <summary>
        /// Tanggal referensi untuk pengambilan harga pasar
        /// </summary>
        public DateTime TanggalReferensi { get; set; }

        /// <summary>
        /// Informasi detail harga pasar yang digunakan
        /// </summary>
        public HargaPasarInfoDto? HargaPasarInfo { get; set; }
    }

    /// <summary>
    /// DTO untuk informasi harga pasar
    /// </summary>
    public class HargaPasarInfoDto
    {
        /// <summary>
        /// ID harga pasar
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Harga per ekor
        /// </summary>
        public decimal HargaPerEkor { get; set; }

        /// <summary>
        /// Harga per kilogram
        /// </summary>
        public decimal HargaPerKg { get; set; }

        /// <summary>
        /// Tanggal mulai berlaku
        /// </summary>
        public DateTime TanggalMulai { get; set; }

        /// <summary>
        /// Tanggal berakhir (nullable)
        /// </summary>
        public DateTime? TanggalBerakhir { get; set; }

        /// <summary>
        /// Wilayah
        /// </summary>
        public string? Wilayah { get; set; }

        /// <summary>
        /// Keterangan
        /// </summary>
        public string? Keterangan { get; set; }

        public string HargaPerEkorFormatted => $"Rp {HargaPerEkor:N0}/ekor";
        public string HargaPerKgFormatted => $"Rp {HargaPerKg:N0}/kg";
        public string PeriodeBerlaku => $"{TanggalMulai:dd/MM/yyyy} - {(TanggalBerakhir?.ToString("dd/MM/yyyy") ?? "Sekarang")}";
    }

    /// <summary>
    /// DTO untuk keuntungan berdasarkan data panen
    /// </summary>
    public class KeuntunganPanenDto
    {
        /// <summary>
        /// ID data panen
        /// </summary>
        public Guid PanenId { get; set; }

        /// <summary>
        /// Tanggal panen
        /// </summary>
        public DateTime TanggalPanen { get; set; }

        /// <summary>
        /// Jumlah ayam yang dipanen
        /// </summary>
        public int JumlahAyam { get; set; }

        /// <summary>
        /// Total berat yang dipanen (kg)
        /// </summary>
        public decimal TotalBerat { get; set; }

        /// <summary>
        /// Berat rata-rata per ekor
        /// </summary>
        public decimal BeratRataRata { get; set; }

        /// <summary>
        /// Harga pasar yang berlaku saat panen
        /// </summary>
        public decimal HargaPerKg { get; set; }

        /// <summary>
        /// Total pendapatan dari panen ini
        /// </summary>
        public decimal TotalPendapatan { get; set; }

        /// <summary>
        /// Informasi kandang
        /// </summary>
        public string NamaKandang { get; set; } = string.Empty;

        /// <summary>
        /// Informasi harga pasar yang digunakan
        /// </summary>
        public HargaPasarInfoDto HargaPasarInfo { get; set; } = null!;
    }

    /// <summary>
    /// DTO untuk laporan keuntungan periode
    /// </summary>
    public class LaporanKeuntunganDto
    {
        /// <summary>
        /// Periode laporan
        /// </summary>
        public PeriodeLaporanDto Periode { get; set; } = null!;

        /// <summary>
        /// Total semua panen dalam periode
        /// </summary>
        public TotalKeuntunganDto Total { get; set; } = null!;

        /// <summary>
        /// Detail keuntungan per panen
        /// </summary>
        public List<KeuntunganPanenDto> DetailPanen { get; set; } = new();

        /// <summary>
        /// Rata-rata harga pasar dalam periode
        /// </summary>
        public decimal RataRataHargaPerKg { get; set; }

        /// <summary>
        /// Fluktuasi harga dalam periode
        /// </summary>
        public FluktusiHargaDto FluktusiHarga { get; set; } = null!;
    }

    /// <summary>
    /// DTO untuk periode laporan
    /// </summary>
    public class PeriodeLaporanDto
    {
        /// <summary>
        /// Tanggal mulai
        /// </summary>
        public DateTime TanggalMulai { get; set; }

        /// <summary>
        /// Tanggal akhir
        /// </summary>
        public DateTime TanggalAkhir { get; set; }

        /// <summary>
        /// Jumlah hari dalam periode
        /// </summary>
        public int JumlahHari { get; set; }
    }

    /// <summary>
    /// DTO untuk total keuntungan
    /// </summary>
    public class TotalKeuntunganDto
    {
        /// <summary>
        /// Total jumlah panen
        /// </summary>
        public int TotalPanen { get; set; }

        /// <summary>
        /// Total ayam yang dipanen
        /// </summary>
        public int TotalAyam { get; set; }

        /// <summary>
        /// Total berat yang dipanen (kg)
        /// </summary>
        public decimal TotalBerat { get; set; }

        /// <summary>
        /// Total pendapatan kotor
        /// </summary>
        public decimal TotalPendapatan { get; set; }

        /// <summary>
        /// Rata-rata berat per ekor
        /// </summary>
        public decimal RataRataBeratPerEkor { get; set; }

        /// <summary>
        /// Rata-rata harga per ekor
        /// </summary>
        public decimal RataRataHargaPerEkor { get; set; }
    }

    /// <summary>
    /// DTO untuk fluktuasi harga
    /// </summary>
    public class FluktusiHargaDto
    {
        /// <summary>
        /// Harga terendah dalam periode
        /// </summary>
        public decimal HargaTerendah { get; set; }

        /// <summary>
        /// Harga tertinggi dalam periode
        /// </summary>
        public decimal HargaTertinggi { get; set; }

        /// <summary>
        /// Selisih harga (tertinggi - terendah)
        /// </summary>
        public decimal SelisihHarga { get; set; }

        /// <summary>
        /// Persentase fluktuasi
        /// </summary>
        public decimal PersentaseFluktuasi { get; set; }
    }

    /// <summary>
    /// DTO untuk laporan keuntungan bulanan
    /// </summary>
    public class LaporanKeuntunganBulananDto
    {
        /// <summary>
        /// Tahun dan bulan laporan
        /// </summary>
        public int Tahun { get; set; }
        public int Bulan { get; set; }
        public string NamaBulan { get; set; } = string.Empty;

        /// <summary>
        /// Periode laporan
        /// </summary>
        public PeriodeLaporanDto Periode { get; set; } = null!;

        /// <summary>
        /// Total keuntungan bulan ini
        /// </summary>
        public TotalKeuntunganDto Total { get; set; } = null!;

        /// <summary>
        /// Detail panen per hari dalam bulan
        /// </summary>
        public List<KeuntunganHarianDto> DetailHarian { get; set; } = new();

        /// <summary>
        /// Perbandingan dengan bulan sebelumnya
        /// </summary>
        public PerbandinganBulananDto? PerbandinganBulanSebelumnya { get; set; }

        /// <summary>
        /// Harga pasar yang digunakan dalam bulan ini
        /// </summary>
        public List<HargaPasarInfoDto> HargaPasarBulanIni { get; set; } = new();

        /// <summary>
        /// Rata-rata dan fluktuasi harga
        /// </summary>
        public decimal RataRataHargaPerKg { get; set; }
        public FluktusiHargaDto FluktusiHarga { get; set; } = null!;
    }

    /// <summary>
    /// DTO untuk keuntungan harian
    /// </summary>
    public class KeuntunganHarianDto
    {
        /// <summary>
        /// Tanggal
        /// </summary>
        public DateTime Tanggal { get; set; }

        /// <summary>
        /// Jumlah panen di hari ini
        /// </summary>
        public int JumlahPanen { get; set; }

        /// <summary>
        /// Total ayam dipanen
        /// </summary>
        public int TotalAyam { get; set; }

        /// <summary>
        /// Total berat dipanen (kg)
        /// </summary>
        public decimal TotalBerat { get; set; }

        /// <summary>
        /// Total keuntungan hari ini
        /// </summary>
        public decimal TotalKeuntungan { get; set; }

        /// <summary>
        /// Harga pasar yang berlaku
        /// </summary>
        public decimal HargaPerKg { get; set; }

        /// <summary>
        /// Detail panen dalam hari ini
        /// </summary>
        public List<KeuntunganPanenDto> DetailPanen { get; set; } = new();
    }

    /// <summary>
    /// DTO untuk perbandingan bulanan
    /// </summary>
    public class PerbandinganBulananDto
    {
        /// <summary>
        /// Bulan sebelumnya
        /// </summary>
        public string BulanSebelumnya { get; set; } = string.Empty;

        /// <summary>
        /// Total keuntungan bulan sebelumnya
        /// </summary>
        public decimal TotalKeuntunganSebelumnya { get; set; }

        /// <summary>
        /// Selisih keuntungan
        /// </summary>
        public decimal SelisihKeuntungan { get; set; }

        /// <summary>
        /// Persentase perubahan
        /// </summary>
        public decimal PersentasePerubahan { get; set; }

        /// <summary>
        /// Status perubahan (Naik/Turun/Stabil)
        /// </summary>
        public string StatusPerubahan { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO untuk laporan keuntungan mingguan
    /// </summary>
    public class LaporanKeuntunganMingguanDto
    {
        /// <summary>
        /// Tahun dan minggu
        /// </summary>
        public int Tahun { get; set; }
        public int MingguKe { get; set; }

        /// <summary>
        /// Periode minggu (tanggal mulai - akhir)
        /// </summary>
        public PeriodeLaporanDto Periode { get; set; } = null!;

        /// <summary>
        /// Total keuntungan minggu ini
        /// </summary>
        public TotalKeuntunganDto Total { get; set; } = null!;

        /// <summary>
        /// Detail panen per hari dalam minggu
        /// </summary>
        public List<KeuntunganHarianDto> DetailHarian { get; set; } = new();

        /// <summary>
        /// Perbandingan dengan minggu sebelumnya
        /// </summary>
        public PerbandinganMingguanDto? PerbandinganMingguSebelumnya { get; set; }

        /// <summary>
        /// Statistik mingguan
        /// </summary>
        public decimal RataRataHargaPerKg { get; set; }
        public FluktusiHargaDto FluktusiHarga { get; set; } = null!;
    }

    /// <summary>
    /// DTO untuk perbandingan mingguan
    /// </summary>
    public class PerbandinganMingguanDto
    {
        /// <summary>
        /// Minggu sebelumnya
        /// </summary>
        public string MingguSebelumnya { get; set; } = string.Empty;

        /// <summary>
        /// Total keuntungan minggu sebelumnya
        /// </summary>
        public decimal TotalKeuntunganSebelumnya { get; set; }

        /// <summary>
        /// Selisih keuntungan
        /// </summary>
        public decimal SelisihKeuntungan { get; set; }

        /// <summary>
        /// Persentase perubahan
        /// </summary>
        public decimal PersentasePerubahan { get; set; }

        /// <summary>
        /// Status perubahan (Naik/Turun/Stabil)
        /// </summary>
        public string StatusPerubahan { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO untuk ringkasan keuntungan tahunan
    /// </summary>
    public class RingkasanKeuntunganTahunanDto
    {
        /// <summary>
        /// Tahun laporan
        /// </summary>
        public int Tahun { get; set; }

        /// <summary>
        /// Total keuntungan sepanjang tahun
        /// </summary>
        public TotalKeuntunganDto TotalTahunan { get; set; } = null!;

        /// <summary>
        /// Breakdown per bulan
        /// </summary>
        public List<KeuntunganBulananSummaryDto> BreakdownBulanan { get; set; } = new();

        /// <summary>
        /// Bulan terbaik (keuntungan tertinggi)
        /// </summary>
        public KeuntunganBulananSummaryDto? BulanTerbaik { get; set; }

        /// <summary>
        /// Bulan terburuk (keuntungan terendah)
        /// </summary>
        public KeuntunganBulananSummaryDto? BulanTerburuk { get; set; }

        /// <summary>
        /// Rata-rata keuntungan per bulan
        /// </summary>
        public decimal RataRataKeuntunganPerBulan { get; set; }

        /// <summary>
        /// Trend keuntungan tahunan
        /// </summary>
        public string TrendTahunan { get; set; } = string.Empty; // "Naik", "Turun", "Stabil"

        /// <summary>
        /// Statistik harga pasar tahunan
        /// </summary>
        public FluktusiHargaDto FluktusiHargaTahunan { get; set; } = null!;
    }

    /// <summary>
    /// DTO untuk summary keuntungan bulanan
    /// </summary>
    public class KeuntunganBulananSummaryDto
    {
        /// <summary>
        /// Bulan (1-12)
        /// </summary>
        public int Bulan { get; set; }

        /// <summary>
        /// Nama bulan
        /// </summary>
        public string NamaBulan { get; set; } = string.Empty;

        /// <summary>
        /// Total keuntungan bulan ini
        /// </summary>
        public decimal TotalKeuntungan { get; set; }

        /// <summary>
        /// Total ayam dipanen
        /// </summary>
        public int TotalAyam { get; set; }

        /// <summary>
        /// Total berat dipanen
        /// </summary>
        public decimal TotalBerat { get; set; }

        /// <summary>
        /// Jumlah hari panen
        /// </summary>
        public int JumlahHariPanen { get; set; }

        /// <summary>
        /// Rata-rata harga pasar dalam bulan
        /// </summary>
        public decimal RataRataHargaPerKg { get; set; }
    }
}
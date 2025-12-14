using SIMTernakAyam.Models;
using SIMTernakAyam.DTOs.HargaPasar;

namespace SIMTernakAyam.Services.Interfaces
{
    /// <summary>
    /// Interface service untuk operasi harga pasar ayam
    /// </summary>
    public interface IHargaPasarService : IBaseService<HargaPasar>
    {
        /// <summary>
        /// Mendapatkan harga pasar yang aktif berdasarkan tanggal
        /// </summary>
        /// <param name="tanggal">Tanggal referensi</param>
        /// <returns>Harga pasar yang berlaku</returns>
        Task<HargaPasar?> GetHargaAktifByTanggalAsync(DateTime tanggal);
        
        /// <summary>
        /// Mendapatkan harga pasar terbaru yang masih aktif
        /// </summary>
        /// <returns>Harga pasar terbaru</returns>
        Task<HargaPasar?> GetHargaTerbaruAsync();
        
        /// <summary>
        /// Mendapatkan riwayat harga dalam rentang waktu tertentu
        /// </summary>
        /// <param name="startDate">Tanggal mulai</param>
        /// <param name="endDate">Tanggal akhir</param>
        /// <returns>List harga pasar</returns>
        Task<IEnumerable<HargaPasar>> GetRiwayatHargaAsync(DateTime startDate, DateTime endDate);
        
        /// <summary>
        /// Menonaktifkan semua harga yang aktif
        /// </summary>
        /// <returns>Hasil operasi dengan pesan</returns>
        Task<(bool Success, string Message)> DeactivateAllHargaAsync();
        
        /// <summary>
        /// Memperbarui status aktif harga pasar
        /// </summary>
        /// <param name="id">ID harga pasar</param>
        /// <param name="isAktif">Status aktif baru</param>
        /// <returns>Hasil operasi dengan pesan</returns>
        Task<(bool Success, string Message)> UpdateStatusHargaAsync(Guid id, bool isAktif);
        
        /// <summary>
        /// Validasi overlap tanggal harga pasar
        /// </summary>
        /// <param name="tanggalMulai">Tanggal mulai baru</param>
        /// <param name="tanggalBerakhir">Tanggal berakhir baru</param>
        /// <param name="excludeId">ID yang dikecualikan dari pengecekan (untuk update)</param>
        /// <returns>Hasil validasi</returns>
        Task<(bool IsValid, string Message)> ValidateTanggalOverlapAsync(DateTime tanggalMulai, DateTime? tanggalBerakhir, Guid? excludeId = null);
        
        /// <summary>
        /// Mendapatkan harga berdasarkan wilayah dan tanggal
        /// </summary>
        /// <param name="wilayah">Nama wilayah</param>
        /// <param name="tanggal">Tanggal referensi</param>
        /// <returns>Harga pasar untuk wilayah tertentu</returns>
        Task<HargaPasar?> GetHargaByWilayahAsync(string wilayah, DateTime tanggal);

        /// <summary>
        /// Menghitung estimasi keuntungan berdasarkan harga pasar aktif
        /// </summary>
        /// <param name="totalAyam">Total jumlah ayam</param>
        /// <param name="beratRataRata">Berat rata-rata per ekor (kg)</param>
        /// <param name="tanggalReferensi">Tanggal referensi untuk harga pasar</param>
        /// <returns>Estimasi keuntungan</returns>
        Task<(bool Success, string Message, EstimasiKeuntunganDto? Data)> HitungKeuntunganAsync(int totalAyam, decimal beratRataRata, DateTime tanggalReferensi);

        /// <summary>
        /// Menghitung keuntungan berdasarkan data panen yang sudah ada
        /// </summary>
        /// <param name="panenId">ID data panen</param>
        /// <returns>Keuntungan berdasarkan data panen</returns>
        Task<(bool Success, string Message, KeuntunganPanenDto? Data)> HitungKeuntunganDariPanenAsync(Guid panenId);

        /// <summary>
        /// Mendapatkan laporan keuntungan dalam periode tertentu
        /// </summary>
        /// <param name="startDate">Tanggal mulai</param>
        /// <param name="endDate">Tanggal akhir</param>
        /// <returns>Laporan keuntungan periode</returns>
        Task<(bool Success, string Message, LaporanKeuntunganDto? Data)> GetLaporanKeuntunganAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Mendapatkan laporan keuntungan bulanan
        /// </summary>
        /// <param name="tahun">Tahun</param>
        /// <param name="bulan">Bulan (1-12)</param>
        /// <returns>Laporan keuntungan bulanan</returns>
        Task<(bool Success, string Message, LaporanKeuntunganBulananDto? Data)> GetLaporanKeuntunganBulananAsync(int tahun, int bulan);

        /// <summary>
        /// Mendapatkan laporan keuntungan mingguan
        /// </summary>
        /// <param name="tahun">Tahun</param>
        /// <param name="mingguKe">Minggu ke- dalam tahun (1-53)</param>
        /// <returns>Laporan keuntungan mingguan</returns>
        Task<(bool Success, string Message, LaporanKeuntunganMingguanDto? Data)> GetLaporanKeuntunganMingguanAsync(int tahun, int mingguKe);

        /// <summary>
        /// Mendapatkan ringkasan keuntungan tahunan dengan breakdown per bulan
        /// </summary>
        /// <param name="tahun">Tahun</param>
        /// <returns>Ringkasan keuntungan tahunan</returns>
        Task<(bool Success, string Message, RingkasanKeuntunganTahunanDto? Data)> GetRingkasanKeuntunganTahunanAsync(int tahun);
    }
}
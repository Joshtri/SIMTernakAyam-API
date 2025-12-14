using SIMTernakAyam.Models;
using SIMTernakAyam.Repositories.Interfaces;

namespace SIMTernakAyam.Repository.Interfaces
{
    /// <summary>
    /// Interface repository untuk operasi data harga pasar ayam
    /// </summary>
    public interface IHargaPasarRepository : IBaseRepository<HargaPasar>
    {
        /// <summary>
        /// Mendapatkan harga pasar yang aktif berdasarkan tanggal
        /// </summary>
        /// <param name="tanggal">Tanggal referensi untuk mencari harga</param>
        /// <returns>Harga pasar yang berlaku pada tanggal tersebut</returns>
        Task<HargaPasar?> GetHargaAktifByTanggalAsync(DateTime tanggal);
        
        /// <summary>
        /// Mendapatkan harga pasar terbaru yang masih aktif
        /// </summary>
        /// <returns>Harga pasar terbaru yang aktif</returns>
        Task<HargaPasar?> GetHargaTerbaruAsync();
        
        /// <summary>
        /// Mendapatkan riwayat harga dalam rentang waktu tertentu
        /// </summary>
        /// <param name="startDate">Tanggal mulai</param>
        /// <param name="endDate">Tanggal akhir</param>
        /// <returns>List harga pasar dalam rentang waktu</returns>
        Task<IEnumerable<HargaPasar>> GetRiwayatHargaAsync(DateTime startDate, DateTime endDate);
        
        /// <summary>
        /// Menonaktifkan semua harga yang aktif
        /// </summary>
        /// <returns>Hasil operasi</returns>
        Task<bool> DeactivateAllAsync();
        
        /// <summary>
        /// Memperbarui status aktif harga pasar
        /// </summary>
        /// <param name="id">ID harga pasar</param>
        /// <param name="isAktif">Status aktif baru</param>
        /// <returns>Hasil operasi</returns>
        Task<bool> UpdateStatusAsync(Guid id, bool isAktif);
        
        /// <summary>
        /// Mendapatkan harga berdasarkan wilayah dan tanggal
        /// </summary>
        /// <param name="wilayah">Nama wilayah</param>
        /// <param name="tanggal">Tanggal referensi</param>
        /// <returns>Harga pasar untuk wilayah dan tanggal tertentu</returns>
        Task<HargaPasar?> GetHargaByWilayahAsync(string wilayah, DateTime tanggal);
    }
}
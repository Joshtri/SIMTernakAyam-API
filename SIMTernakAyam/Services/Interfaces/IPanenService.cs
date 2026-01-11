using SIMTernakAyam.Models;
using SIMTernakAyam.DTOs.Panen;

namespace SIMTernakAyam.Services.Interfaces
{
    public interface IPanenService : IBaseService<Panen>
    {
        Task<IEnumerable<Panen>> GetPanenByAyamAsync(Guid ayamId);
        Task<IEnumerable<Panen>> GetPanenByKandangAsync(Guid kandangId);
        Task<IEnumerable<Panen>> GetPanenByPeriodAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<Panen>> GetAllPanenWithDetailsAsync();
        Task<Panen?> GetPanenWithDetailsAsync(Guid id);
        Task<int> GetTotalEkorPanenByKandangAsync(Guid kandangId);
        Task<decimal> GetTotalBeratPanenByPeriodAsync(DateTime startDate, DateTime endDate);
        /// <summary>
        /// Get available chicken stock information for an Ayam
        /// </summary>
        Task<(int TotalMasuk, int SudahDipanen, int SisaTersedia)> GetStokAyamAsync(Guid ayamId);
        
        /// <summary>
        /// Menghitung analisis keuntungan untuk panen tertentu
        /// </summary>
        /// <param name="panenId">ID panen yang akan dianalisis</param>
        /// <returns>Detail analisis keuntungan</returns>
        Task<AnalisisKeuntunganDto?> GetAnalisisKeuntunganAsync(Guid panenId);
        
        /// <summary>
        /// Menghitung analisis keuntungan untuk beberapa panen dalam periode tertentu
        /// </summary>
        /// <param name="startDate">Tanggal mulai</param>
        /// <param name="endDate">Tanggal akhir</param>
        /// <param name="kandangId">ID kandang (opsional)</param>
        /// <returns>List analisis keuntungan</returns>
        Task<List<AnalisisKeuntunganDto>> GetAnalisisKeuntunganByPeriodAsync(DateTime startDate, DateTime endDate, Guid? kandangId = null);
        
        /// <summary>
        /// Mendapatkan ringkasan keuntungan dalam periode tertentu
        /// </summary>
        /// <param name="startDate">Tanggal mulai</param>
        /// <param name="endDate">Tanggal akhir</param>
        /// <returns>Ringkasan keuntungan</returns>
        Task<object> GetRingkasanKeuntunganAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Create panen dengan Auto FIFO - sistem otomatis distribute ke ayam-ayam dengan prioritas FIFO
        /// </summary>
        /// <param name="kandangId">ID kandang</param>
        /// <param name="tanggalPanen">Tanggal panen</param>
        /// <param name="jumlahEkorPanen">Total jumlah ekor yang dipanen</param>
        /// <param name="beratRataRata">Berat rata-rata per ekor</param>
        /// <returns>List panen yang berhasil dibuat</returns>
        Task<(bool Success, string Message, List<Models.Panen>? Data)> CreatePanenAutoFifoAsync(
            Guid kandangId,
            DateTime tanggalPanen,
            int jumlahEkorPanen,
            decimal beratRataRata);

        /// <summary>
        /// Create panen dengan Manual Split - user menentukan jumlah dari ayam lama dan baru
        /// </summary>
        /// <param name="kandangId">ID kandang</param>
        /// <param name="tanggalPanen">Tanggal panen</param>
        /// <param name="jumlahDariAyamLama">Jumlah panen dari ayam periode lama/sisa</param>
        /// <param name="jumlahDariAyamBaru">Jumlah panen dari ayam periode baru</param>
        /// <param name="beratRataRata">Berat rata-rata per ekor (kg)</param>
        /// <returns>List panen yang berhasil dibuat</returns>
        Task<(bool Success, string Message, List<Models.Panen>? Data)> CreatePanenManualSplitAsync(
            Guid kandangId,
            DateTime tanggalPanen,
            int jumlahDariAyamLama,
            int jumlahDariAyamBaru,
            decimal beratRataRata);
    }
}
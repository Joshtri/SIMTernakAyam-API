using SIMTernakAyam.Models;
using SIMTernakAyam.Repositories.Interfaces;
using SIMTernakAyam.Enums;

namespace SIMTernakAyam.Repository.Interfaces
{
    public interface IBiayaRepository : IBaseRepository<Biaya>
    {
        Task<IEnumerable<Biaya>> GetByPetugasIdAsync(Guid petugasId);
        Task<IEnumerable<Biaya>> GetByOperasionalIdAsync(Guid operasionalId);
        Task<IEnumerable<Biaya>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<Biaya>> GetByJenisBiayaAsync(string jenisBiaya);
        Task<decimal> GetTotalBiayaByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<Biaya>> GetWithDetailsAsync();

        /// <summary>
        /// Get biaya by month and year
        /// </summary>
        Task<IEnumerable<Biaya>> GetByBulanTahunAsync(int bulan, int tahun);

        /// <summary>
        /// Get biaya by kandang, bulan, and tahun
        /// </summary>
        Task<IEnumerable<Biaya>> GetByKandangBulanTahunAsync(Guid kandangId, int bulan, int tahun);

        /// <summary>
        /// Get biaya by kandang ID
        /// </summary>
        Task<IEnumerable<Biaya>> GetByKandangIdAsync(Guid kandangId);

        /// <summary>
        /// Get single biaya that is linked to an operasional record (if any), without tracking.
        /// </summary>
        Task<Biaya?> GetSingleByOperasionalIdAsync(Guid operasionalId);

        /// <summary>
        /// Get biaya by category
        /// </summary>
        Task<IEnumerable<Biaya>> GetByKategoriBiayaAsync(KategoriBiayaEnum kategori);

        /// <summary>
        /// Get biaya by category and date range
        /// </summary>
        Task<IEnumerable<Biaya>> GetByKategoriBiayaAndDateRangeAsync(KategoriBiayaEnum kategori, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Get total biaya by category and date range
        /// </summary>
        Task<decimal> GetTotalBiayaByKategoriBiayaAndDateRangeAsync(KategoriBiayaEnum kategori, DateTime startDate, DateTime endDate);
    }
}

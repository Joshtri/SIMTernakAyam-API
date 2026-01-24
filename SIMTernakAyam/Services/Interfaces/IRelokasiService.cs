using SIMTernakAyam.DTOs.Relokasi;
using SIMTernakAyam.Models;

namespace SIMTernakAyam.Services.Interfaces
{
    public interface IRelokasiService : IBaseService<RelokasiAyam>
    {
        /// <summary>
        /// Get all relokasi with full details
        /// </summary>
        Task<IEnumerable<RelokasiAyam>> GetAllRelokasiWithDetailsAsync();

        /// <summary>
        /// Get relokasi by ID with full details
        /// </summary>
        Task<RelokasiAyam?> GetRelokasiWithDetailsAsync(Guid id);

        /// <summary>
        /// Get all relokasi for a specific kandang (as source or destination)
        /// </summary>
        Task<IEnumerable<RelokasiAyam>> GetRelokasiByKandangAsync(Guid kandangId);

        /// <summary>
        /// Create relokasi with full business logic:
        /// 1. Validate kandang asal & tujuan exist
        /// 2. Validate ayam asal has enough stock
        /// 3. Validate kandang tujuan has capacity
        /// 4. Create new batch at kandang tujuan
        /// 5. Save relokasi record
        /// </summary>
        Task<(bool Success, string Message, RelokasiAyam? Data)> CreateRelokasiAsync(
            CreateRelokasiDto dto,
            Guid petugasId);

        /// <summary>
        /// Cancel relokasi (soft cancel - change status to Dibatalkan)
        /// Note: This does NOT automatically reverse the stock changes
        /// </summary>
        Task<(bool Success, string Message)> BatalkanRelokasiAsync(Guid id);

        /// <summary>
        /// Get total ekor yang telah direlokasi keluar dari batch ayam tertentu
        /// </summary>
        Task<int> GetTotalRelokasiKeluarByAyamAsync(Guid ayamId);

        /// <summary>
        /// Get total relokasi keluar untuk multiple ayam IDs (bulk query)
        /// </summary>
        Task<Dictionary<Guid, int>> GetTotalRelokasiKeluarByAyamIdsAsync(IEnumerable<Guid> ayamIds);

        /// <summary>
        /// Search relokasi with filters
        /// </summary>
        Task<IEnumerable<RelokasiAyam>> SearchRelokasiAsync(string? search = null, Guid? kandangId = null);
    }
}

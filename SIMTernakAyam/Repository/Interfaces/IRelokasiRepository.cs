using SIMTernakAyam.Models;
using SIMTernakAyam.Repositories.Interfaces;

namespace SIMTernakAyam.Repository.Interfaces
{
    public interface IRelokasiRepository : IBaseRepository<RelokasiAyam>
    {
        /// <summary>
        /// Get all relokasi with full details (kandang, ayam, petugas)
        /// </summary>
        Task<IEnumerable<RelokasiAyam>> GetAllWithDetailsAsync();

        /// <summary>
        /// Get relokasi by ID with full details
        /// </summary>
        Task<RelokasiAyam?> GetWithDetailsAsync(Guid id);

        /// <summary>
        /// Get all relokasi for a specific kandang (as source or destination)
        /// </summary>
        Task<IEnumerable<RelokasiAyam>> GetByKandangAsync(Guid kandangId);

        /// <summary>
        /// Get all relokasi from a specific kandang (as source)
        /// </summary>
        Task<IEnumerable<RelokasiAyam>> GetByKandangAsalAsync(Guid kandangAsalId);

        /// <summary>
        /// Get all relokasi to a specific kandang (as destination)
        /// </summary>
        Task<IEnumerable<RelokasiAyam>> GetByKandangTujuanAsync(Guid kandangTujuanId);

        /// <summary>
        /// Get all relokasi for a specific batch ayam (as source)
        /// </summary>
        Task<IEnumerable<RelokasiAyam>> GetByAyamAsalAsync(Guid ayamAsalId);

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

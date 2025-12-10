using SIMTernakAyam.Models;
using SIMTernakAyam.Repositories.Interfaces;

namespace SIMTernakAyam.Repository.Interfaces
{
    public interface IMortalitasRepository : IBaseRepository<Mortalitas>
    {
        Task<IEnumerable<Mortalitas>> GetByAyamIdAsync(Guid ayamId);
        Task<IEnumerable<Mortalitas>> GetByKandangIdAsync(Guid kandangId);
        Task<IEnumerable<Mortalitas>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<Mortalitas>> GetWithDetailsAsync();
        Task<Mortalitas?> GetWithDetailsAsync(Guid id);
        Task<int> GetTotalMortalitasByKandangAsync(Guid kandangId);
        Task<int> GetTotalMortalitasByDateRangeAsync(DateTime startDate, DateTime endDate);
        
        // New methods for calculations
        Task<IEnumerable<Mortalitas>> GetMortalitasWithCalculationsAsync();
        Task<int> GetTotalAyamBeforeMortalityAsync(Guid ayamId, DateTime tanggalKematian);
        Task<int> GetKandangCapacityAsync(Guid kandangId);
        Task<IEnumerable<Mortalitas>> SearchMortalitasAsync(string? search = null, Guid? kandangId = null);

        /// <summary>
        /// Get total mortality count for specific ayam
        /// </summary>
        Task<int> GetTotalMortalitasByAyamAsync(Guid ayamId);

        /// <summary>
        /// Get total mortality count for multiple ayam IDs at once (for efficient bulk queries)
        /// </summary>
        Task<Dictionary<Guid, int>> GetTotalMortalitasByAyamIdsAsync(IEnumerable<Guid> ayamIds);
    }
}
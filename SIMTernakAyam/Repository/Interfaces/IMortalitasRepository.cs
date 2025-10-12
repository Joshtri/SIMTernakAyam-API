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
    }
}
using SIMTernakAyam.Models;
using SIMTernakAyam.Repositories.Interfaces;

namespace SIMTernakAyam.Repository.Interfaces
{
    public interface IPanenRepository : IBaseRepository<Panen>
    {
        Task<IEnumerable<Panen>> GetByAyamIdAsync(Guid ayamId);
        Task<IEnumerable<Panen>> GetByKandangIdAsync(Guid kandangId);
        Task<IEnumerable<Panen>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<Panen>> GetWithDetailsAsync();
        Task<Panen?> GetWithDetailsAsync(Guid id);
        Task<int> GetTotalEkorPanenByKandangAsync(Guid kandangId);
        Task<decimal> GetTotalBeratPanenByDateRangeAsync(DateTime startDate, DateTime endDate);
    }
}
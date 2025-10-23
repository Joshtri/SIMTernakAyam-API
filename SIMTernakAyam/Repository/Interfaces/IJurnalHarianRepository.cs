using SIMTernakAyam.Models;
using SIMTernakAyam.Repositories.Interfaces;

namespace SIMTernakAyam.Repository.Interfaces
{
    public interface IJurnalHarianRepository : IBaseRepository<JurnalHarian>
    {
        Task<List<JurnalHarian>> GetByPetugasIdAsync(Guid petugasId, int page = 1, int pageSize = 10);
        Task<List<JurnalHarian>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, Guid? petugasId = null);
        Task<List<JurnalHarian>> GetByKandangIdAsync(Guid kandangId, int page = 1, int pageSize = 10);
        Task<int> CountByPetugasIdAsync(Guid petugasId);
        Task Delete(Guid id);
    }
}

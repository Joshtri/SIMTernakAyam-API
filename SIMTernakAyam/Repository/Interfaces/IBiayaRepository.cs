using SIMTernakAyam.Models;
using SIMTernakAyam.Repositories.Interfaces;
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
    }
}
using SIMTernakAyam.Models;
using SIMTernakAyam.Repositories.Interfaces;

namespace SIMTernakAyam.Repository.Interfaces
{
    public interface IKandangRepository : IBaseRepository<Kandang>
    {
        Task<bool> IsKandangAvailableAsync(Guid kandangId, int jumlahAyamBaru);
        Task<int> GetCurrentAyamCountAsync(Guid kandangId);

    }
}

using SIMTernakAyam.Models;
using SIMTernakAyam.Repositories.Interfaces;
namespace SIMTernakAyam.Repository.Interfaces
{
    public interface IAyamRepository : IBaseRepository<Ayam>
    {
        Task<IEnumerable<Ayam>> GetByKandangIdAsync(Guid kandangId);
        Task<int> GetTotalAyamInKandangAsync(Guid kandangId);
        Task<IEnumerable<Ayam>> GetAyamWithKandangAsync();
        Task<Ayam?> GetWithDetailsAsync(Guid id);
        
        /// <summary>
        /// Get ayam sisa (IsAyamSisa = true) by kandangId
        /// </summary>
        Task<IEnumerable<Ayam>> GetAyamSisaByKandangIdAsync(Guid kandangId);
    }
}
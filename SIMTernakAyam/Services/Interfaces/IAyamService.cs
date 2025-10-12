using SIMTernakAyam.Models;

namespace SIMTernakAyam.Services.Interfaces
{
    public interface IAyamService : IBaseService<Ayam>
    {
        Task<IEnumerable<Ayam>> GetAyamByKandangAsync(Guid kandangId);
        Task<(bool Success, string Message, Ayam? Data)> AddAyamToKandangAsync(Ayam ayam);
        Task<int> GetTotalAyamCountInKandangAsync(Guid kandangId);
        Task<IEnumerable<Ayam>> GetAllAyamWithDetailsAsync();
        Task<Ayam?> GetAyamWithDetailsAsync(Guid id);
    }
}
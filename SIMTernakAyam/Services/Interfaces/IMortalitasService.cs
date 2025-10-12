using SIMTernakAyam.Models;

namespace SIMTernakAyam.Services.Interfaces
{
    public interface IMortalitasService : IBaseService<Mortalitas>
    {
        Task<IEnumerable<Mortalitas>> GetMortalitasByAyamAsync(Guid ayamId);
        Task<IEnumerable<Mortalitas>> GetMortalitasByKandangAsync(Guid kandangId);
        Task<IEnumerable<Mortalitas>> GetMortalitasByPeriodAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<Mortalitas>> GetAllMortalitasWithDetailsAsync();
        Task<Mortalitas?> GetMortalitasWithDetailsAsync(Guid id);
        Task<int> GetTotalMortalitasByKandangAsync(Guid kandangId);
        Task<int> GetTotalMortalitasByPeriodAsync(DateTime startDate, DateTime endDate);
    }
}
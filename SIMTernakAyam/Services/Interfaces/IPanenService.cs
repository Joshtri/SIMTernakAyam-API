using SIMTernakAyam.Models;

namespace SIMTernakAyam.Services.Interfaces
{
    public interface IPanenService : IBaseService<Panen>
    {
        Task<IEnumerable<Panen>> GetPanenByAyamAsync(Guid ayamId);
        Task<IEnumerable<Panen>> GetPanenByKandangAsync(Guid kandangId);
        Task<IEnumerable<Panen>> GetPanenByPeriodAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<Panen>> GetAllPanenWithDetailsAsync();
        Task<Panen?> GetPanenWithDetailsAsync(Guid id);
        Task<int> GetTotalEkorPanenByKandangAsync(Guid kandangId);
        Task<decimal> GetTotalBeratPanenByPeriodAsync(DateTime startDate, DateTime endDate);
        /// <summary>
        /// Get available chicken stock information for an Ayam
        /// </summary>
        Task<(int TotalMasuk, int SudahDipanen, int SisaTersedia)> GetStokAyamAsync(Guid ayamId);
    }
}
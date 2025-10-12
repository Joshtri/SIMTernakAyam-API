using SIMTernakAyam.Models;

namespace SIMTernakAyam.Services.Interfaces
{
    public interface IBiayaService : IBaseService<Biaya>
    {
        Task<IEnumerable<Biaya>> GetBiayaByPetugasAsync(Guid petugasId);
        Task<IEnumerable<Biaya>> GetBiayaByOperasionalAsync(Guid operasionalId);
        Task<IEnumerable<Biaya>> GetBiayaByPeriodAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<Biaya>> GetBiayaByJenisAsync(string jenisBiaya);
        Task<decimal> GetTotalBiayaPeriodAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<Biaya>> GetAllBiayaWithDetailsAsync();
    }
}
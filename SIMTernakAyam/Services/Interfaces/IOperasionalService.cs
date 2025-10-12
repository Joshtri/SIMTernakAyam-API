using SIMTernakAyam.Models;

namespace SIMTernakAyam.Services.Interfaces
{
    public interface IOperasionalService : IBaseService<Operasional>
    {
        Task<IEnumerable<Operasional>> GetOperasionalByKandangAsync(Guid kandangId);
        Task<IEnumerable<Operasional>> GetOperasionalByPetugasAsync(Guid petugasId);
        Task<IEnumerable<Operasional>> GetOperasionalByJenisKegiatanAsync(Guid jenisKegiatanId);
        Task<IEnumerable<Operasional>> GetOperasionalByPeriodAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<Operasional>> GetAllOperasionalWithDetailsAsync();
        Task<Operasional?> GetOperasionalWithDetailsAsync(Guid id);
    }
}
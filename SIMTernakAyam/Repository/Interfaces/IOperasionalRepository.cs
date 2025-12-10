using SIMTernakAyam.Models;
using SIMTernakAyam.Repositories.Interfaces;
namespace SIMTernakAyam.Repository.Interfaces
{
    public interface IOperasionalRepository : IBaseRepository<Operasional>
    {
        Task<IEnumerable<Operasional>> GetByKandangIdAsync(Guid kandangId);
        Task<IEnumerable<Operasional>> GetByPetugasIdAsync(Guid petugasId);
        Task<IEnumerable<Operasional>> GetByJenisKegiatanIdAsync(Guid jenisKegiatanId);
        Task<IEnumerable<Operasional>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<Operasional>> GetWithDetailsAsync();
        Task<Operasional?> GetWithDetailsAsync(Guid id);
        
        // Method baru untuk tracking penggunaan stok
        Task<IEnumerable<Operasional>> GetByVaksinIdAndPeriodAsync(Guid vaksinId, DateTime startDate, DateTime endDate);
        Task<IEnumerable<Operasional>> GetByPakanIdAndPeriodAsync(Guid pakanId, DateTime startDate, DateTime endDate);
    }
}
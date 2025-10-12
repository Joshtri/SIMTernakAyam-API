using SIMTernakAyam.Models;
using SIMTernakAyam.Repositories.Interfaces;

namespace SIMTernakAyam.Repository.Interfaces
{
    public interface IJenisKegiatanRepository : IBaseRepository<JenisKegiatan>
    {
        Task<JenisKegiatan?> GetByNameAsync(string namaKegiatan);
        Task<IEnumerable<JenisKegiatan>> GetBySatuanAsync(string satuan);
        Task<bool> IsNameExistsAsync(string namaKegiatan, Guid? excludeId = null);
    }
}
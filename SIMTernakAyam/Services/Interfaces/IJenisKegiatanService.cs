using SIMTernakAyam.Models;

namespace SIMTernakAyam.Services.Interfaces
{
    public interface IJenisKegiatanService : IBaseService<JenisKegiatan>
    {
        Task<JenisKegiatan?> GetByNameAsync(string namaKegiatan);
        Task<IEnumerable<JenisKegiatan>> GetBySatuanAsync(string satuan);
        Task<(bool Success, string Message)> ValidateUniqueNameAsync(string namaKegiatan, Guid? excludeId = null);
    }
}
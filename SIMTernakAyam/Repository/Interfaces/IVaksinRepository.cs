using SIMTernakAyam.Enums;
using SIMTernakAyam.Models;
using SIMTernakAyam.Repositories.Interfaces;

namespace SIMTernakAyam.Repository.Interfaces
{
    public interface IVaksinRepository : IBaseRepository<Vaksin>
    {
        Task<Vaksin?> GetByNameAsync(string namaVaksin);
        Task<IEnumerable<Vaksin>> GetLowStockAsync(int threshold = 5);
        Task<bool> IsNameExistsAsync(string namaVaksin, Guid? excludeId = null);
        Task<bool> UpdateStokAsync(Guid id, int newStok);
        Task<(bool Success, string Message)> UpdateStokAsyncDirect(Guid id, int amountChange, DateTime tanggal);
        Task<(int Stok, int Bulan, int Tahun)?> GetStockInfoAsync(Guid id);

        /// <summary>
        /// Get all vaksin/vitamin filtered by type
        /// </summary>
        Task<IEnumerable<Vaksin>> GetByTypeAsync(VaksinVitaminTypeEnum tipe);
    }
}
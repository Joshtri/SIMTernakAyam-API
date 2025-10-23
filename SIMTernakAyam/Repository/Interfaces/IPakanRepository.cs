using SIMTernakAyam.Models;
using SIMTernakAyam.Repositories.Interfaces;

 namespace SIMTernakAyam.Repository.Interfaces
{
    public interface IPakanRepository : IBaseRepository<Pakan>
    {
        Task<Pakan?> GetByNameAsync(string namaPakan);
        Task<IEnumerable<Pakan>> GetLowStockAsync(int threshold = 10);
        Task<bool> IsNameExistsAsync(string namaPakan, Guid? excludeId = null);
        Task<bool> UpdateStokAsync(Guid id, int newStok);
        Task<bool> UpdateStokKgAsync(Guid id, decimal newStok);
        Task<(bool Success, string Message)> UpdateStokKgAsyncDirect(Guid id, decimal amountChange, DateTime tanggal);
        Task<(decimal StokKg, int Bulan, int Tahun)?> GetStockInfoAsync(Guid id);
    }
}
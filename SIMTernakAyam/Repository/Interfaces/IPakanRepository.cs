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
    }
}
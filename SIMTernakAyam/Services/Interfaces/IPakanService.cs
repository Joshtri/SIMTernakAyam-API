using SIMTernakAyam.Models;

namespace SIMTernakAyam.Services.Interfaces
{
    public interface IPakanService : IBaseService<Pakan>
    {
        Task<Pakan?> GetByNameAsync(string namaPakan);
        Task<IEnumerable<Pakan>> GetLowStockPakanAsync(int threshold = 10);
        Task<(bool Success, string Message)> UpdateStokAsync(Guid id, int newStok);
        Task<(bool Success, string Message)> ValidateUniqueNameAsync(string namaPakan, Guid? excludeId = null);
    }
}
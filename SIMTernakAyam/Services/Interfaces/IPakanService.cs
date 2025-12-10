using SIMTernakAyam.DTOs.Pakan;
using SIMTernakAyam.Models;

namespace SIMTernakAyam.Services.Interfaces
{
    public interface IPakanService : IBaseService<Pakan>
    {
        Task<Pakan?> GetByNameAsync(string namaPakan);
        Task<IEnumerable<Pakan>> GetLowStockPakanAsync(int threshold = 10);
        Task<(bool Success, string Message)> UpdateStokAsync(Guid id, int newStok);
        Task<(bool Success, string Message)> ValidateUniqueNameAsync(string namaPakan, Guid? excludeId = null);
        
        // Method baru untuk detail penggunaan stok
        Task<List<PakanResponseDto>> GetAllPakanWithUsageDetailAsync();
        Task<PakanResponseDto?> GetPakanWithUsageDetailAsync(Guid id);
        Task<object?> CheckStockAvailabilityAsync(Guid id, decimal jumlahDibutuhkan);
        
        // ? Diagnostic method untuk debug stok terpakai
        Task<object> GetStokDiagnosticAsync(Guid pakanId);
    }
}
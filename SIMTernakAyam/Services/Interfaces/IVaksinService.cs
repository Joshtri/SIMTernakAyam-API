using SIMTernakAyam.DTOs.Vaksin;
using SIMTernakAyam.Enums;
using SIMTernakAyam.Models;

namespace SIMTernakAyam.Services.Interfaces
{
    public interface IVaksinService : IBaseService<Vaksin>
    {
        Task<Vaksin?> GetByNameAsync(string namaVaksin);
        Task<IEnumerable<Vaksin>> GetLowStockVaksinAsync(int threshold = 5);
        Task<(bool Success, string Message)> UpdateStokAsync(Guid id, int newStok);
        Task<(bool Success, string Message)> ValidateUniqueNameAsync(string namaVaksin, Guid? excludeId = null);
        Task<IEnumerable<Vaksin>> GetByTypeAsync(VaksinVitaminTypeEnum tipe);
        
        // Method baru untuk detail penggunaan stok
        Task<List<VaksinResponseDto>> GetAllVaksinWithUsageDetailAsync();
        Task<VaksinResponseDto?> GetVaksinWithUsageDetailAsync(Guid id);
        Task<object?> CheckStockAvailabilityAsync(Guid id, int jumlahDibutuhkan);
    }
}
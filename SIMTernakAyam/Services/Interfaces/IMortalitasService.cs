using SIMTernakAyam.Models;
using SIMTernakAyam.DTOs.Mortalitas;

namespace SIMTernakAyam.Services.Interfaces
{
    public interface IMortalitasService : IBaseService<Mortalitas>
    {
        // Basic entity methods
        Task<IEnumerable<Mortalitas>> GetMortalitasByAyamAsync(Guid ayamId);
        Task<IEnumerable<Mortalitas>> GetMortalitasByPeriodAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<Mortalitas>> GetAllMortalitasWithDetailsAsync();
        Task<int> GetTotalMortalitasByKandangAsync(Guid kandangId);
        Task<int> GetTotalMortalitasByPeriodAsync(DateTime startDate, DateTime endDate);

        // Enhanced DTO methods (with calculations)
        Task<List<MortalitasResponseDto>> GetEnhancedMortalitasAsync(string? search = null, Guid? kandangId = null);
        Task<List<MortalitasResponseDto>> GetMortalitasByKandangAsync(Guid kandangId);
        Task<MortalitasResponseDto?> GetMortalitasWithDetailsAsync(Guid id);

        // Methods with IFormFile support for image upload
        Task<(bool Success, string Message, Mortalitas? Data)> CreateAsync(Mortalitas entity, IFormFile? fotoMortalitas = null);
        Task<(bool Success, string Message)> UpdateAsync(Mortalitas entity, IFormFile? fotoMortalitas = null);
    }
}
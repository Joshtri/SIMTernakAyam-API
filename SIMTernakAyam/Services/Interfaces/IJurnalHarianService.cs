using SIMTernakAyam.DTOs.JurnalHarian;

namespace SIMTernakAyam.Services.Interfaces
{
    public interface IJurnalHarianService
    {
        Task<JurnalHarianResponseDto> CreateAsync(CreateJurnalHarianDto dto, Guid petugasId, IFormFile? fotoKegiatan = null);
        Task<JurnalHarianResponseDto?> GetByIdAsync(Guid id);
        Task<List<JurnalHarianResponseDto>> GetAllAsync(Guid? petugasId = null, int page = 1, int pageSize = 10);
        Task<JurnalHarianResponseDto?> UpdateAsync(Guid id, UpdateJurnalHarianDto dto, Guid petugasId, IFormFile? fotoKegiatan = null);
        Task<bool> DeleteAsync(Guid id, Guid petugasId);
        Task<LaporanJurnalDto> GetLaporanAsync(DateTime startDate, DateTime endDate, Guid? petugasId = null);
        Task<int> GetTotalCountAsync(Guid? petugasId = null);
    }
}

using SIMTernakAyam.DTOs.Biaya;
using SIMTernakAyam.Models;

namespace SIMTernakAyam.Services.Interfaces
{
    public interface IBiayaService : IBaseService<Biaya>
    {
        Task<IEnumerable<Biaya>> GetBiayaByPetugasAsync(Guid petugasId);
        Task<IEnumerable<Biaya>> GetBiayaByOperasionalAsync(Guid operasionalId);
        Task<IEnumerable<Biaya>> GetBiayaByPeriodAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<Biaya>> GetBiayaByJenisAsync(string jenisBiaya);
        Task<decimal> GetTotalBiayaPeriodAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<Biaya>> GetAllBiayaWithDetailsAsync();

        /// <summary>
        /// Get biaya by month and year
        /// </summary>
        Task<IEnumerable<Biaya>> GetBiayaByBulanTahunAsync(int bulan, int tahun);

        /// <summary>
        /// Get biaya by kandang
        /// </summary>
        Task<IEnumerable<Biaya>> GetBiayaByKandangAsync(Guid kandangId);

        /// <summary>
        /// Get monthly recap of all biaya grouped by kandang
        /// </summary>
        Task<RekapBiayaBulananDto> GetRekapBiayaBulananAsync(int bulan, int tahun);
    }
}
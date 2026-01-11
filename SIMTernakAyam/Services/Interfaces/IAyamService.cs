using SIMTernakAyam.DTOs.Kandang;
using SIMTernakAyam.Models;

namespace SIMTernakAyam.Services.Interfaces
{
    public interface IAyamService : IBaseService<Ayam>
    {
        Task<IEnumerable<Ayam>> GetAyamByKandangAsync(Guid kandangId);
        Task<(bool Success, string Message, Ayam? Data)> AddAyamToKandangAsync(Ayam ayam);
        Task<int> GetTotalAyamCountInKandangAsync(Guid kandangId);
        Task<IEnumerable<Ayam>> GetAllAyamWithDetailsAsync();
        Task<Ayam?> GetAyamWithDetailsAsync(Guid id);

        /// <summary>
        /// Get ayam data with comprehensive stock information (harvest, mortality, survival rate)
        /// </summary>
        Task<IEnumerable<Ayam>> GetAllAyamWithStockInfoAsync();

        /// <summary>
        /// Get informasi kapasitas dan sisa ayam di kandang
        /// </summary>
        /// <param name="kandangId">ID kandang</param>
        /// <param name="tanggalMasukRencana">Tanggal masuk yang direncanakan user (optional). Jika diisi, cek apakah ada ayam sebelum tanggal ini</param>
        Task<KapasitasKandangDto> GetKapasitasKandangAsync(Guid kandangId, DateTime? tanggalMasukRencana = null);

        /// <summary>
        /// Get list ayam di kandang sorted by tanggal masuk (FIFO) dengan status hidup
        /// </summary>
        Task<List<Ayam>> GetAyamHidupFIFOAsync(Guid kandangId);

        /// <summary>
        /// Get ayam by tipe periode: ayam lama (IsAyamSisa=true) atau ayam baru (IsAyamSisa=false)
        /// </summary>
        /// <param name="kandangId">ID kandang</param>
        /// <param name="isAyamLama">true = ayam lama/sisa, false = ayam baru</param>
        /// <returns>List tuple (Ayam, JumlahHidup)</returns>
        Task<List<(Ayam Ayam, int JumlahHidup)>> GetAyamByPeriodeTypeAsync(Guid kandangId, bool isAyamLama);

        /// <summary>
        /// Create ayam baru dengan validasi kapasitas dan handle sisa ayam
        /// </summary>
        Task<(bool Success, string Message, Ayam? Data, KapasitasKandangDto? KapasitasInfo)> CreateWithValidationAsync(
            Ayam ayam,
            bool forceInput,
            string? alasanInput,
            Guid petugasId);
    }
}
using SIMTernakAyam.Models;

namespace SIMTernakAyam.Services.Interfaces
{
    public interface IKandangAsistenService : IBaseService<KandangAsisten>
    {
        /// <summary>
        /// Mendapatkan semua asisten untuk kandang tertentu
        /// </summary>
        Task<IEnumerable<KandangAsisten>> GetAsistensByKandangAsync(Guid kandangId);

        /// <summary>
        /// Mendapatkan semua kandang yang diasisteni oleh user tertentu
        /// </summary>
        Task<IEnumerable<KandangAsisten>> GetKandangsByAsistenAsync(Guid asistenId);

        /// <summary>
        /// Mendapatkan asisten aktif untuk kandang tertentu
        /// </summary>
        Task<IEnumerable<KandangAsisten>> GetActiveAsistensByKandangAsync(Guid kandangId);

        /// <summary>
        /// Mendapatkan data dengan detail (includes)
        /// </summary>
        Task<KandangAsisten?> GetWithDetailsAsync(Guid id);

        /// <summary>
        /// Mendapatkan semua data dengan detail (includes)
        /// </summary>
        Task<IEnumerable<KandangAsisten>> GetAllWithDetailsAsync();
    }
}

using SIMTernakAyam.Models;

namespace SIMTernakAyam.Repositories.Interfaces
{
    public interface IKandangAsistenRepository : IBaseRepository<KandangAsisten>
    {
        /// <summary>
        /// Mendapatkan semua asisten untuk kandang tertentu
        /// </summary>
        Task<IEnumerable<KandangAsisten>> GetAsistensByKandangIdAsync(Guid kandangId);

        /// <summary>
        /// Mendapatkan semua kandang yang diasisteni oleh user tertentu
        /// </summary>
        Task<IEnumerable<KandangAsisten>> GetKandangsByAsistenIdAsync(Guid asistenId);

        /// <summary>
        /// Cek apakah asisten sudah terdaftar di kandang tertentu
        /// </summary>
        Task<bool> IsAsistenExistsInKandangAsync(Guid kandangId, Guid asistenId, Guid? excludeId = null);

        /// <summary>
        /// Mendapatkan asisten aktif untuk kandang tertentu
        /// </summary>
        Task<IEnumerable<KandangAsisten>> GetActiveAsistensByKandangIdAsync(Guid kandangId);

        /// <summary>
        /// Mendapatkan data dengan include Kandang dan Asisten
        /// </summary>
        Task<KandangAsisten?> GetWithDetailsAsync(Guid id);

        /// <summary>
        /// Mendapatkan semua data dengan include Kandang dan Asisten
        /// </summary>
        Task<IEnumerable<KandangAsisten>> GetAllWithDetailsAsync();
    }
}

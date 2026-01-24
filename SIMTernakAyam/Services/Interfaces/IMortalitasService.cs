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

        /// <summary>
        /// Create mortalitas dengan Auto FIFO - sistem otomatis distribute ke ayam-ayam dengan prioritas FIFO
        /// </summary>
        /// <param name="kandangId">ID kandang</param>
        /// <param name="tanggalKematian">Tanggal kematian</param>
        /// <param name="waktuKematian">Waktu kematian (HH:mm)</param>
        /// <param name="jumlahKematian">Total jumlah kematian</param>
        /// <param name="penyebabKematian">Penyebab kematian</param>
        /// <param name="fotoMortalitas">File foto bukti mortalitas (optional)</param>
        /// <returns>List mortalitas yang berhasil dibuat</returns>
        Task<(bool Success, string Message, List<Mortalitas>? Data)> CreateMortalitasAutoFifoAsync(
            Guid kandangId,
            DateTime tanggalKematian,
            TimeOnly waktuKematian,
            int jumlahKematian,
            string penyebabKematian,
            IFormFile? fotoMortalitas = null);

        /// <summary>
        /// Create mortalitas dengan Manual Split - user menentukan jumlah dari ayam lama dan baru
        /// </summary>
        /// <param name="kandangId">ID kandang</param>
        /// <param name="tanggalKematian">Tanggal kematian</param>
        /// <param name="waktuKematian">Waktu kematian (HH:mm)</param>
        /// <param name="jumlahDariAyamLama">Jumlah kematian dari ayam periode lama/sisa</param>
        /// <param name="jumlahDariAyamBaru">Jumlah kematian dari ayam periode baru</param>
        /// <param name="penyebabKematian">Penyebab kematian</param>
        /// <param name="fotoMortalitas">File foto bukti mortalitas (optional)</param>
        /// <returns>List mortalitas yang berhasil dibuat</returns>
        Task<(bool Success, string Message, List<Mortalitas>? Data)> CreateMortalitasManualSplitAsync(
            Guid kandangId,
            DateTime tanggalKematian,
            TimeOnly waktuKematian,
            int jumlahDariAyamLama,
            int jumlahDariAyamBaru,
            string penyebabKematian,
            IFormFile? fotoMortalitas = null);
    }
}
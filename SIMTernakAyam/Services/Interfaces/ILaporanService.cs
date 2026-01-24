using SIMTernakAyam.DTOs.Laporan;

namespace SIMTernakAyam.Services.Interfaces
{
    public interface ILaporanService
    {
        /// <summary>
        /// Mendapatkan data kesehatan ayam per kandang
        /// </summary>
        Task<List<KesehatanKandangDto>> GetKesehatanKandangAsync();

        /// <summary>
        /// Mendapatkan data kesehatan ayam untuk kandang tertentu
        /// </summary>
        Task<KesehatanKandangDto?> GetKesehatanKandangByIdAsync(Guid kandangId);

        /// <summary>
        /// Mendapatkan laporan operasional berdasarkan periode
        /// </summary>
        /// <param name="startDate">Tanggal mulai</param>
        /// <param name="endDate">Tanggal selesai</param>
        Task<LaporanOperasionalDto> GetLaporanOperasionalAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Mendapatkan analisis produktivitas semua petugas
        /// </summary>
        /// <param name="year">Optional year filter. If null, shows all time data.</param>
        /// <param name="month">Optional month filter (1-12). Requires year parameter.</param>
        /// <param name="hasKandang">Optional filter: true = only staff managing kandang, false = only staff not managing kandang, null = all staff.</param>
        Task<List<AnalisisProduktivitasDto>> GetAnalisisProduktivitasAsync(int? year = null, int? month = null, bool? hasKandang = null);

        /// <summary>
        /// Mendapatkan analisis produktivitas petugas tertentu
        /// </summary>
        /// <param name="year">Optional year filter. If null, shows all time data.</param>
        /// <param name="month">Optional month filter (1-12). Requires year parameter.</param>
        /// <param name="hasKandang">Optional filter: true = only staff managing kandang, false = only staff not managing kandang, null = all staff.</param>
        Task<AnalisisProduktivitasDto?> GetAnalisisProduktivitasByPetugasAsync(Guid petugasId, int? year = null, int? month = null, bool? hasKandang = null);

        /// <summary>
        /// Generate PDF untuk laporan operasional
        /// </summary>
        Task<byte[]> GenerateLaporanOperasionalPdfAsync(Guid kandangId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Generate PDF untuk laporan kesehatan ayam
        /// </summary>
        Task<byte[]> GenerateLaporanKesehatanPdfAsync(Guid kandangId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Mendapatkan daftar batch/siklus yang tersedia
        /// </summary>
        Task<List<BatchOptionDto>> GetBatchesAsync();

        /// <summary>
        /// Mendapatkan laporan detail berdasarkan batch/siklus
        /// </summary>
        Task<LaporanBatchDto?> GetLaporanBatchAsync(Guid batchId);
    }
}

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
        Task<List<AnalisisProduktivitasDto>> GetAnalisisProduktivitasAsync();

        /// <summary>
        /// Mendapatkan analisis produktivitas petugas tertentu
        /// </summary>
        Task<AnalisisProduktivitasDto?> GetAnalisisProduktivitasByPetugasAsync(Guid petugasId);

        /// <summary>
        /// Generate PDF untuk laporan operasional
        /// </summary>
        Task<byte[]> GenerateLaporanOperasionalPdfAsync(Guid kandangId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Generate PDF untuk laporan kesehatan ayam
        /// </summary>
        Task<byte[]> GenerateLaporanKesehatanPdfAsync(Guid kandangId, DateTime startDate, DateTime endDate);
    }
}

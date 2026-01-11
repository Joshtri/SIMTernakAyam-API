using SIMTernakAyam.Models;

namespace SIMTernakAyam.Services.Interfaces
{
    /// <summary>
    /// Service untuk handle Auto FIFO logic (First In, First Out)
    /// Digunakan untuk panen dan mortalitas
    /// </summary>
    public interface IFifoService
    {
        /// <summary>
        /// Distribute jumlah (panen/mortalitas) ke ayam-ayam dengan logic FIFO
        /// Returns: List of (AyamId, Jumlah) yang harus di-proses
        /// </summary>
        Task<List<(Guid AyamId, int Jumlah)>> DistributeAsync(Guid kandangId, int totalJumlah);
    }
}

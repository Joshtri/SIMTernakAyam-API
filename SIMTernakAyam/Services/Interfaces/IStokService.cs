using SIMTernakAyam.Models;

namespace SIMTernakAyam.Services.Interfaces
{
    public interface IStokService
    {
        /// <summary>
        /// Kurangi stok pakan
        /// </summary>
        /// <param name="pakanId">ID Pakan</param>
        /// <param name="tanggal">Tanggal transaksi (untuk referensi masa stok)</param>
        /// <param name="jumlah">Jumlah yang akan dikurangi (dalam kg, desimal)</param>
        /// <returns>Success status dan pesan</returns>
        Task<(bool Success, string Message)> KurangiStokPakan(Guid pakanId, DateTime tanggal, decimal jumlah);

        /// <summary>
        /// Kurangi stok vaksin
        /// </summary>
        /// <param name="vaksinId">ID Vaksin</param>
        /// <param name="tanggal">Tanggal transaksi (untuk referensi masa stok)</param>
        /// <param name="jumlah">Jumlah yang akan dikurangi (dalam dosis)</param>
        /// <returns>Success status dan pesan</returns>
        Task<(bool Success, string Message)> KurangiStokVaksin(Guid vaksinId, DateTime tanggal, int jumlah);

        /// <summary>
        /// Tambah stok pakan (misalnya saat pemesanan baru datang)
        /// </summary>
        /// <param name="pakanId">ID Pakan</param>
        /// <param name="tanggal">Tanggal transaksi (untuk referensi masa stok)</param>
        /// <param name="jumlah">Jumlah yang akan ditambahkan (dalam kg, desimal)</param>
        /// <returns>Success status dan pesan</returns>
        Task<(bool Success, string Message)> TambahStokPakan(Guid pakanId, DateTime tanggal, decimal jumlah);

        /// <summary>
        /// Tambah stok vaksin (misalnya saat pemesanan baru datang)
        /// </summary>
        /// <param name="vaksinId">ID Vaksin</param>
        /// <param name="tanggal">Tanggal transaksi (untuk referensi masa stok)</param>
        /// <param name="jumlah">Jumlah yang akan ditambahkan (dalam dosis)</param>
        /// <returns>Success status dan pesan</returns>
        Task<(bool Success, string Message)> TambahStokVaksin(Guid vaksinId, DateTime tanggal, int jumlah);
    }
}
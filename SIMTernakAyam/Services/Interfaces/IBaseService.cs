using SIMTernakAyam.Models;

namespace SIMTernakAyam.Services.Interfaces
{
    /// <summary>
    /// Base interface untuk semua service dengan operasi CRUD standar
    /// Menggunakan generic type T yang harus inherit dari BaseModel
    /// </summary>
    public interface IBaseService<T> where T : BaseModel
    {
        /// <summary>
        /// Mendapatkan semua data
        /// </summary>
        Task<IEnumerable<T>> GetAllAsync();

        /// <summary>
        /// Mendapatkan data berdasarkan ID
        /// </summary>
        Task<T?> GetByIdAsync(Guid id);

        /// <summary>
        /// Membuat data baru dengan validasi
        /// </summary>
        Task<(bool Success, string Message, T? Data)> CreateAsync(T entity);

        /// <summary>
        /// Mengupdate data dengan validasi
        /// </summary>
        Task<(bool Success, string Message)> UpdateAsync(T entity);

        /// <summary>
        /// Menghapus data berdasarkan ID
        /// </summary>
        Task<(bool Success, string Message)> DeleteAsync(Guid id);

        /// <summary>
        /// Mengecek apakah data dengan ID tertentu ada
        /// </summary>
        Task<bool> ExistsAsync(Guid id);
    }
}

using SIMTernakAyam.Models;

namespace SIMTernakAyam.Repositories.Interfaces
{
    public interface IUserRepository : IBaseRepository<User>
    {
        /// <summary>
        /// Mencari user berdasarkan username
        /// </summary>
        Task<User?> GetByUsernameAsync(string username);

        /// <summary>
        /// Mencari user berdasarkan email
        /// </summary>
        Task<User?> GetByEmailAsync(string email);

        /// <summary>
        /// Memeriksa apakah username sudah digunakan
        /// </summary>
        Task<bool> IsUsernameExistsAsync(string username);

        /// <summary>
        /// Memeriksa apakah email sudah digunakan
        /// </summary>
        Task<bool> IsEmailExistsAsync(string email);

        /// <summary>
        /// Mendapatkan semua user berdasarkan role
        /// </summary>
        Task<IEnumerable<User>> GetUsersByRoleAsync(Enums.RoleEnum role);

        /// <summary>
        /// Mendapatkan user beserta kandang-kandangnya
        /// </summary>
        Task<User?> GetUserWithKandangsAsync(Guid userId);
    }
}

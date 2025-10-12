using SIMTernakAyam.Enums;
using SIMTernakAyam.Models;

namespace SIMTernakAyam.Services.Interfaces
{
    /// <summary>
    /// Interface untuk User Service yang extends IBaseService
    /// Mewarisi operasi CRUD standar dan menambahkan method spesifik untuk User
    /// </summary>
    public interface IUserService : IBaseService<User>
    {
        /// <summary>
        /// Mendapatkan user berdasarkan username
        /// </summary>
        Task<User?> GetUserByUsernameAsync(string username);

        /// <summary>
        /// Mendapatkan user berdasarkan email
        /// </summary>
        Task<User?> GetUserByEmailAsync(string email);

        /// <summary>
        /// Mendapatkan user berdasarkan role
        /// </summary>
        Task<IEnumerable<User>> GetUsersByRoleAsync(RoleEnum role);

        /// <summary>
        /// Validasi login user
        /// </summary>
        Task<(bool Success, string Message, User? User)> ValidateLoginAsync(string username, string password);

        /// <summary>
        /// Mengubah password user
        /// </summary>
        Task<(bool Success, string Message)> ChangePasswordAsync(Guid userId, string oldPassword, string newPassword);
    }
}

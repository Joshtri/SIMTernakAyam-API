using SIMTernakAyam.DTOs.Auth;
using SIMTernakAyam.DTOs.User;
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

        //Task<IEnumerable<User>> GetAllAsync(List<RoleEnum> roles);


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

        /// <summary>
        /// Mendapatkan profile user dengan informasi role-specific
        /// </summary>
        Task<UserProfileDto> GetUserProfileAsync(Guid userId);

        /// <summary>
        /// Update profile user (username dan fullname saja)
        /// </summary>
        Task<(bool Success, string Message)> UpdateProfileAsync(UpdateProfileDto dto);

        // ✅ Method baru untuk get user dengan informasi kandang
        Task<CurrentUserDto> GetCurrentUserWithKandangsAsync(Guid userId);
    }
}

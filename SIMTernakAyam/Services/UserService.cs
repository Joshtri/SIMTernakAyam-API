using SIMTernakAyam.Enums;
using SIMTernakAyam.Models;
using SIMTernakAyam.Repositories.Interfaces;
using SIMTernakAyam.Services.Interfaces;

namespace SIMTernakAyam.Services
{
    /// <summary>
    /// User Service yang mewarisi BaseService
    /// Menggunakan konsep OOP: Inheritance, Polymorphism, Encapsulation
    /// </summary>
    public class UserService : BaseService<User>, IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository) : base(userRepository)
        {
            _userRepository = userRepository;
        }

        #region Methods Specific to User

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await _userRepository.GetByUsernameAsync(username);
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _userRepository.GetByEmailAsync(email);
        }

        public async Task<IEnumerable<User>> GetUsersByRoleAsync(RoleEnum role)
        {
            return await _userRepository.GetUsersByRoleAsync(role);
        }

        public async Task<(bool Success, string Message, User? User)> ValidateLoginAsync(string username, string password)
        {
            try
            {
                var user = await _userRepository.GetByUsernameAsync(username);
                if (user == null)
                {
                    return (false, "Username atau password salah.", null);
                }

                // Verifikasi password
                if (!VerifyPassword(password, user.Password))
                {
                    return (false, "Username atau password salah.", null);
                }

                return (true, "Login berhasil.", user);
            }
            catch (Exception ex)
            {
                return (false, $"Terjadi kesalahan: {ex.Message}", null);
            }
        }

        public async Task<(bool Success, string Message)> ChangePasswordAsync(Guid userId, string oldPassword, string newPassword)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return (false, "User tidak ditemukan.");
                }

                // Verifikasi password lama
                if (!VerifyPassword(oldPassword, user.Password))
                {
                    return (false, "Password lama tidak sesuai.");
                }

                // Validasi password baru
                if (newPassword.Length < 6)
                {
                    return (false, "Password baru minimal 6 karakter.");
                }

                // Update password
                user.Password = HashPassword(newPassword);
                user.UpdateAt = DateTime.UtcNow;

                _userRepository.UpdateAsync(user);
                await _userRepository.SaveChangesAsync();

                return (true, "Password berhasil diubah.");
            }
            catch (Exception ex)
            {
                return (false, $"Terjadi kesalahan: {ex.Message}");
            }
        }

        #endregion

        #region Override Methods from BaseService - Custom Validation

        /// <summary>
        /// Override validasi sebelum create
        /// Validasi username dan email unik
        /// </summary>
        protected override async Task<ValidationResult> ValidateOnCreateAsync(User entity)
        {
            // Validasi username sudah ada
            if (await _userRepository.IsUsernameExistsAsync(entity.Username))
            {
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Username sudah digunakan."
                };
            }

            // Validasi email sudah ada
            if (await _userRepository.IsEmailExistsAsync(entity.Email))
            {
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Email sudah digunakan."
                };
            }

            return new ValidationResult { IsValid = true };
        }

        /// <summary>
        /// Override validasi sebelum update
        /// Validasi username dan email tidak digunakan user lain
        /// </summary>
        protected override async Task<ValidationResult> ValidateOnUpdateAsync(User entity, User existingEntity)
        {
            // Cek apakah username sudah digunakan oleh user lain
            var userWithSameUsername = await _userRepository.GetByUsernameAsync(entity.Username);
            if (userWithSameUsername != null && userWithSameUsername.Id != entity.Id)
            {
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Username sudah digunakan oleh user lain."
                };
            }

            // Cek apakah email sudah digunakan oleh user lain
            var userWithSameEmail = await _userRepository.GetByEmailAsync(entity.Email);
            if (userWithSameEmail != null && userWithSameEmail.Id != entity.Id)
            {
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Email sudah digunakan oleh user lain."
                };
            }

            return new ValidationResult { IsValid = true };
        }

        /// <summary>
        /// Hook sebelum create - Hash password
        /// </summary>
        protected override Task BeforeCreateAsync(User entity)
        {
            entity.Password = HashPassword(entity.Password);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Hook sebelum update - Update data dan hash password jika diubah
        /// </summary>
        protected override Task BeforeUpdateAsync(User entity, User existingEntity)
        {
            // Update data
            existingEntity.Username = entity.Username;
            existingEntity.Email = entity.Email;
            existingEntity.FullName = entity.FullName;
            existingEntity.NoWA = entity.NoWA;
            existingEntity.Role = entity.Role;

            // Jika password diubah, hash password baru
            if (!string.IsNullOrEmpty(entity.Password) && entity.Password != existingEntity.Password)
            {
                existingEntity.Password = HashPassword(entity.Password);
            }

            // Update entity reference untuk disimpan
            entity.Password = existingEntity.Password;
            entity.CreatedAt = existingEntity.CreatedAt;

            return Task.CompletedTask;
        }

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// Hash password menggunakan BCrypt
        /// </summary>
        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        /// <summary>
        /// Verifikasi password dengan hash
        /// </summary>
        private bool VerifyPassword(string password, string hashedPassword)
        {
            try
            {
                return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
            }
            catch
            {
                return false;
            }
        }

        #endregion
    }
}

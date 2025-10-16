using System.ComponentModel.DataAnnotations;

namespace SIMTernakAyam.DTOs.User
{
    /// <summary>
    /// DTO untuk request update profile
    /// </summary>
    public class UpdateProfileDto
    {
        [Required(ErrorMessage = "User ID wajib diisi.")]
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "Username wajib diisi.")]
        [StringLength(50, ErrorMessage = "Username maksimal 50 karakter.")]
        public string Username { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Nama maksimal 100 karakter.")]
        public string? FullName { get; set; }
    }
}
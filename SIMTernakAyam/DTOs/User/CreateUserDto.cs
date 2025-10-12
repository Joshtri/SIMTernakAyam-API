using SIMTernakAyam.Enums;
using System.ComponentModel.DataAnnotations;

namespace SIMTernakAyam.DTOs.User
{
    /// <summary>
    /// DTO untuk request create user baru
    /// </summary>
    public class CreateUserDto
    {
        [Required(ErrorMessage = "Username wajib diisi.")]
        [StringLength(50, ErrorMessage = "Username maksimal 50 karakter.")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password wajib diisi.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password minimal 6 karakter dan maksimal 100 karakter.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Konfirmasi password wajib diisi.")]
        [Compare("Password", ErrorMessage = "Password dan konfirmasi password tidak cocok.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Nama maksimal 100 karakter.")]
        public string? FullName { get; set; }

        [Required(ErrorMessage = "Email wajib diisi.")]
        [EmailAddress(ErrorMessage = "Format email tidak valid.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "No WA wajib diisi.")]
        [Phone(ErrorMessage = "Format nomor WhatsApp tidak valid.")]
        [RegularExpression(@"^(\+62|62|0)[0-9]{9,12}$", ErrorMessage = "Nomor WhatsApp harus berupa nomor Indonesia yang valid.")]
        public string NoWA { get; set; } = string.Empty;

        [Required(ErrorMessage = "Role wajib diisi.")]
        public RoleEnum Role { get; set; }
    }
}

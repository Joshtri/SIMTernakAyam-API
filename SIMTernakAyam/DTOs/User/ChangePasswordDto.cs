using System.ComponentModel.DataAnnotations;

namespace SIMTernakAyam.DTOs.User
{
    /// <summary>
    /// DTO untuk request ubah password
    /// </summary>
    public class ChangePasswordDto
    {
        [Required(ErrorMessage = "Password lama wajib diisi.")]
        public string OldPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password baru wajib diisi.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password baru minimal 6 karakter dan maksimal 100 karakter.")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Konfirmasi password wajib diisi.")]
        [Compare("NewPassword", ErrorMessage = "Password baru dan konfirmasi password tidak cocok.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}

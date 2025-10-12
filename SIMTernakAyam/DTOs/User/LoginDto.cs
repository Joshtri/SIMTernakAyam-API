using System.ComponentModel.DataAnnotations;

namespace SIMTernakAyam.DTOs.User
{
    /// <summary>
    /// DTO untuk request login
    /// </summary>
    public class LoginDto
    {
        [Required(ErrorMessage = "Username wajib diisi.")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password wajib diisi.")]
        public string Password { get; set; } = string.Empty;
    }
}

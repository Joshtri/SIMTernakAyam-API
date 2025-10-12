using System.ComponentModel.DataAnnotations;

namespace SIMTernakAyam.DTOs.Auth
{
    public class RefreshTokenDto
    {
        [Required(ErrorMessage = "Refresh token wajib diisi.")]
        public string RefreshToken { get; set; } = string.Empty;
    }
}
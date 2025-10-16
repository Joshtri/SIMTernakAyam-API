using SIMTernakAyam.DTOs.User;
using SIMTernakAyam.Services.Interfaces;
using System.Security.Claims;

namespace SIMTernakAyam.DTOs.Auth
{
    public class LoginResponseDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public string TokenType { get; set; } = "Bearer";
        public DateTime ExpiresAt { get; set; }
        public UserInfoDto User { get; set; } = new();

        public class UserInfoDto
        {
            public Guid Id { get; set; }
            public string Username { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string FullName { get; set; } = string.Empty;
            public string NoWA { get; set; } = string.Empty;
            public string Role { get; set; } = string.Empty;
        }

        public static LoginResponseDto FromUser(Models.User user, string accessToken, string refreshToken, DateTime expiresAt)
        {
            return new LoginResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = expiresAt,
                User = new UserInfoDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email ?? string.Empty,
                    FullName = user.FullName ?? string.Empty,
                    NoWA = user.NoWA ?? string.Empty,
                    Role = user.Role.ToString()
                }
            };
        }
    }
}
using SIMTernakAyam.Enums;

namespace SIMTernakAyam.DTOs.User
{
    /// <summary>
    /// DTO untuk response user (tanpa password)
    /// </summary>
    public class UserResponseDto
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string Email { get; set; } = string.Empty;
        public string NoWA { get; set; } = string.Empty;
        public RoleEnum Role { get; set; }
        public string RoleName => Role.ToString();
        public DateTime CreatedAt { get; set; }
        public DateTime UpdateAt { get; set; }

        /// <summary>
        /// Map dari User entity ke UserResponseDto
        /// </summary>
        public static UserResponseDto FromEntity(Models.User user)
        {
            return new UserResponseDto
            {
                Id = user.Id,
                Username = user.Username,
                FullName = user.FullName,
                Email = user.Email,
                NoWA = user.NoWA,
                Role = user.Role,
                CreatedAt = user.CreatedAt,
                UpdateAt = user.UpdateAt
            };
        }

        /// <summary>
        /// Map dari list User entity ke list UserResponseDto
        /// </summary>
        public static List<UserResponseDto> FromEntities(IEnumerable<Models.User> users)
        {
            return users.Select(FromEntity).ToList();
        }
    }
}

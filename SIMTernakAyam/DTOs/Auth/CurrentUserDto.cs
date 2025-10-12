namespace SIMTernakAyam.DTOs.Auth
{
    public class CurrentUserDto
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string NoWA { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdateAt { get; set; }

        public static CurrentUserDto FromUser(Models.User user)
        {
            return new CurrentUserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email ?? string.Empty,
                FullName = user.FullName ?? string.Empty,
                NoWA = user.NoWA ?? string.Empty,
                Role = user.Role.ToString(),
                CreatedAt = user.CreatedAt,
                UpdateAt = user.UpdateAt
            };
        }
    }
}
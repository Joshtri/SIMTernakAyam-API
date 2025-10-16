using SIMTernakAyam.Enums;

namespace SIMTernakAyam.DTOs.User
{
    /// <summary>
    /// DTO untuk response profile user dengan informasi role-specific
    /// </summary>
    public class UserProfileDto
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string Email { get; set; } = string.Empty;
        public string NoWA { get; set; } = string.Empty;
        public RoleEnum Role { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdateAt { get; set; }

        // Role-specific information
        public List<KandangProfileDto>? Kandangs { get; set; }

        public static UserProfileDto FromEntity(Models.User user)
        {
            return new UserProfileDto
            {
                Id = user.Id,
                Username = user.Username,
                FullName = user.FullName,
                Email = user.Email,
                NoWA = user.NoWA,
                Role = user.Role,
                CreatedAt = user.CreatedAt,
                UpdateAt = user.UpdateAt,
                Kandangs = user.Kandangs?.Select(k => KandangProfileDto.FromEntity(k)).ToList()
            };
        }
    }

    /// <summary>
    /// DTO untuk informasi kandang di profile (simplified)
    /// </summary>
    public class KandangProfileDto
    {
        public Guid Id { get; set; }
        public string NamaKandang { get; set; } = string.Empty;
        public int Kapasitas { get; set; }
        public string Lokasi { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        public static KandangProfileDto FromEntity(Models.Kandang kandang)
        {
            return new KandangProfileDto
            {
                Id = kandang.Id,
                NamaKandang = kandang.NamaKandang,
                Kapasitas = kandang.Kapasitas,
                Lokasi = kandang.Lokasi,
                CreatedAt = kandang.CreatedAt
            };
        }
    }
}
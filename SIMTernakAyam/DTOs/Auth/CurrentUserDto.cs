using SIMTernakAyam.Enums;

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

        // ? Informasi Kandang untuk Petugas
        public List<KandangInfoDto>? KandangsManaged { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdateAt { get; set; }

        public static CurrentUserDto FromUser(Models.User user, List<Models.Kandang>? kandangs = null)
        {
            var dto = new CurrentUserDto
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

            // Add kandang information for Petugas
            if (user.Role == RoleEnum.Petugas && kandangs != null && kandangs.Any())
            {
                dto.KandangsManaged = kandangs.Select(k => new KandangInfoDto
                {
                    Id = k.Id,
                    NamaKandang = k.NamaKandang,
                    Kapasitas = k.Kapasitas,
                    Lokasi = k.Lokasi
                }).ToList();
            }

            return dto;
        }
    }

    public class KandangInfoDto
    {
        public Guid Id { get; set; }
        public string NamaKandang { get; set; } = string.Empty;
        public int Kapasitas { get; set; }
        public string Lokasi { get; set; } = string.Empty;
    }
}
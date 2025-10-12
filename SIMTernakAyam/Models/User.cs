using SIMTernakAyam.Enums;
using System.ComponentModel.DataAnnotations;

namespace SIMTernakAyam.Models
{
    // inherit base field from BaseModel
    public class User : BaseModel
    {
 
        public string Username { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;
 
        public string? FullName { get; set; }
 
        public RoleEnum Role { get; set; }
 
        public string Email { get; set; } = string.Empty;
 
        public string NoWA { get; set; } = string.Empty;

        public ICollection<Kandang>? Kandangs { get; set; }
    }
}

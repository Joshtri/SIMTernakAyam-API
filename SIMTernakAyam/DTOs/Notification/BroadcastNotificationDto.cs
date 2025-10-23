using System.ComponentModel.DataAnnotations;

namespace SIMTernakAyam.DTOs.Notification
{
    public class BroadcastNotificationDto
    {
        [Required(ErrorMessage = "Title harus diisi")]
        [MaxLength(200, ErrorMessage = "Title maksimal 200 karakter")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Message harus diisi")]
        [MaxLength(1000, ErrorMessage = "Message maksimal 1000 karakter")]
        public string Message { get; set; } = string.Empty;

        [Required(ErrorMessage = "Type harus diisi")]
        public string Type { get; set; } = "system"; // info, warning, error, success, reminder, system, message

        [Required(ErrorMessage = "Priority harus diisi")]
        public string Priority { get; set; } = "medium"; // low, medium, high, urgent

        public string? LinkUrl { get; set; }

        // Target role: "Petugas", "Operator", "Pemilik", atau "all"
        public string? TargetRole { get; set; }
    }
}

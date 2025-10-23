using System.ComponentModel.DataAnnotations;

namespace SIMTernakAyam.DTOs.Notification
{
    public class CreateNotificationDto
    {
        [Required(ErrorMessage = "UserId harus diisi")]
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "Title harus diisi")]
        [MaxLength(200, ErrorMessage = "Title maksimal 200 karakter")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Message harus diisi")]
        [MaxLength(1000, ErrorMessage = "Message maksimal 1000 karakter")]
        public string Message { get; set; } = string.Empty;

        public string Type { get; set; } = "info";
    }
}
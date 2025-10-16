namespace SIMTernakAyam.DTOs.Notification
{
    public class CreateNotificationDto
    {
        public Guid UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = "Info"; // Info, Warning, Critical
    }
}
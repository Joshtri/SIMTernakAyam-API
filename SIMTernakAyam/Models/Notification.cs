namespace SIMTernakAyam.Models
{
    public class Notification : BaseModel
    {
        public Guid UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // "info", "warning", "danger", "success"
        public bool IsRead { get; set; } = false;
        public DateTime? ReadAt { get; set; }

        // Navigation property
        public User? User { get; set; }
    }
}

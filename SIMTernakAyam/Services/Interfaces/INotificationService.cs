using SIMTernakAyam.Models;

namespace SIMTernakAyam.Services.Interfaces
{
    public interface INotificationService
    {
        Task<IEnumerable<Notification>> GetUserNotificationsAsync(Guid userId);
        Task<IEnumerable<Notification>> GetUserUnreadNotificationsAsync(Guid userId);
        Task<Notification?> GetNotificationByIdAsync(Guid id);
        Task<Notification> CreateNotificationAsync(Guid userId, string title, string message, string type);
        Task<Notification> UpdateNotificationAsync(Notification notification);
        Task<bool> DeleteNotificationAsync(Guid id);
        Task<bool> MarkAsReadAsync(Guid id);
        Task<IEnumerable<Notification>> GetUserNotificationsByTypeAsync(Guid userId, string type);
        Task<int> GetUserUnreadCountAsync(Guid userId);
        Task<bool> MarkAllAsReadAsync(Guid userId);
    }
}
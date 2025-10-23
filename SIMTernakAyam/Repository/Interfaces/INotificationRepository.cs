using SIMTernakAyam.Models;

namespace SIMTernakAyam.Repository.Interfaces
{
    public interface INotificationRepository
    {
        Task<(IEnumerable<Notification> notifications, int total)> GetByUserIdAsync(
            Guid userId,
            bool? isRead = null,
            int page = 1,
            int limit = 10);

        Task<IEnumerable<Notification>> GetUnreadByUserIdAsync(Guid userId);
        Task<Notification?> GetByIdAsync(Guid id);
        Task<Notification> CreateAsync(Notification notification);
        Task<Notification> UpdateAsync(Notification notification);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> MarkAsReadAsync(Guid id, DateTime readAt);
        Task<int> GetUnreadCountByUserIdAsync(Guid userId);
        Task<IEnumerable<Guid>> GetUserIdsByRoleAsync(string role);
        Task<bool> MarkAllAsReadAsync(Guid userId);
    }
}
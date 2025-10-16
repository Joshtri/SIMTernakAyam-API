using SIMTernakAyam.Models;

namespace SIMTernakAyam.Repository.Interfaces
{
    public interface INotificationRepository
    {
        Task<IEnumerable<Notification>> GetByUserIdAsync(Guid userId);
        Task<IEnumerable<Notification>> GetUnreadByUserIdAsync(Guid userId);
        Task<Notification?> GetByIdAsync(Guid id);
        Task<Notification> CreateAsync(Notification notification);
        Task<Notification> UpdateAsync(Notification notification);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> MarkAsReadAsync(Guid id);
        Task<IEnumerable<Notification>> GetByUserIdAndTypeAsync(Guid userId, string type);
        Task<int> GetUnreadCountByUserIdAsync(Guid userId);
    }
}
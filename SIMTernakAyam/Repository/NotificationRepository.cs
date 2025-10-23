using Microsoft.EntityFrameworkCore;
using SIMTernakAyam.Data;
using SIMTernakAyam.Enums;
using SIMTernakAyam.Models;
using SIMTernakAyam.Repository.Interfaces;

namespace SIMTernakAyam.Repository
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly ApplicationDbContext _context;

        public NotificationRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<(IEnumerable<Notification> notifications, int total)> GetByUserIdAsync(
            Guid userId,
            bool? isRead = null,
            int page = 1,
            int limit = 10)
        {
            var query = _context.Notifications
                .Where(n => n.UserId == userId);

            // Apply filter
            if (isRead.HasValue)
            {
                query = query.Where(n => n.IsRead == isRead.Value);
            }

            var total = await query.CountAsync();

            var notifications = await query
                .OrderByDescending(n => n.CreatedAt)
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToListAsync();

            return (notifications, total);
        }

        public async Task<IEnumerable<Notification>> GetUnreadByUserIdAsync(Guid userId)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<Notification?> GetByIdAsync(Guid id)
        {
            return await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == id);
        }

        public async Task<Notification> CreateAsync(Notification notification)
        {
            notification.CreatedAt = DateTime.UtcNow;
            notification.UpdateAt = DateTime.UtcNow;

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            return notification;
        }

        public async Task<Notification> UpdateAsync(Notification notification)
        {
            notification.UpdateAt = DateTime.UtcNow;

            _context.Notifications.Update(notification);
            await _context.SaveChangesAsync();

            return notification;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null)
            {
                return false;
            }

            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> MarkAsReadAsync(Guid id, DateTime readAt)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null)
            {
                return false;
            }

            notification.IsRead = true;
            notification.ReadAt = readAt;
            notification.UpdateAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<int> GetUnreadCountByUserIdAsync(Guid userId)
        {
            return await _context.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead);
        }

        public async Task<IEnumerable<Guid>> GetUserIdsByRoleAsync(string role)
        {
            var query = _context.Users.AsQueryable();

            if (role.ToLower() != "all")
            {
                // Parse string role to enum
                if (Enum.TryParse<RoleEnum>(role, true, out var roleEnum))
                {
                    query = query.Where(u => u.Role == roleEnum);
                }
                else
                {
                    // If invalid role, return empty list
                    return new List<Guid>();
                }
            }

            return await query.Select(u => u.Id).ToListAsync();
        }

        public async Task<bool> MarkAllAsReadAsync(Guid userId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            var now = DateTime.UtcNow;
            foreach (var notification in notifications)
            {
                notification.IsRead = true;
                notification.ReadAt = now;
                notification.UpdateAt = now;
            }

            await _context.SaveChangesAsync();
            return true;
        }
    }
}

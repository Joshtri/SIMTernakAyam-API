using SIMTernakAyam.DTOs.Notification;
using SIMTernakAyam.Models;
using SIMTernakAyam.Repository.Interfaces;
using SIMTernakAyam.Services.Interfaces;
using SIMTernakAyam.Data;
using Microsoft.EntityFrameworkCore;

namespace SIMTernakAyam.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly ILogger<NotificationService> _logger;
        private readonly ApplicationDbContext _context;

        public NotificationService(INotificationRepository notificationRepository, ILogger<NotificationService> logger, ApplicationDbContext context)
        {
            _notificationRepository = notificationRepository;
            _logger = logger;
            _context = context;
        }

        public async Task<(IEnumerable<NotificationResponseDto> notifications, int total)> GetUserNotificationsAsync(
            Guid userId,
            bool? isRead = null,
            int page = 1,
            int limit = 10)
        {
            var (notifications, total) = await _notificationRepository.GetByUserIdAsync(
                userId, isRead, page, limit);

            var notificationDtos = notifications.Select(MapToDto);

            return (notificationDtos, total);
        }

        public async Task<NotificationResponseDto> CreateNotificationAsync(Guid userId, string message)
        {
            var notification = new Notification
            {
                UserId = userId,
                Message = message,
                IsRead = false
            };

            var created = await _notificationRepository.CreateAsync(notification);
            return MapToDto(created);
        }

        public async Task<bool> DeleteNotificationAsync(Guid id, Guid userId)
        {
            // Verifikasi bahwa notifikasi milik user
            var notification = await _notificationRepository.GetByIdAsync(id);
            if (notification == null || notification.UserId != userId)
            {
                return false;
            }

            return await _notificationRepository.DeleteAsync(id);
        }

        public async Task<NotificationResponseDto?> MarkAsReadAsync(Guid id, Guid userId)
        {
            var notification = await _notificationRepository.GetByIdAsync(id);
            if (notification == null || notification.UserId != userId)
            {
                return null;
            }

            var success = await _notificationRepository.MarkAsReadAsync(id, DateTime.UtcNow);
            if (!success)
            {
                return null;
            }

            // Reload notification
            notification = await _notificationRepository.GetByIdAsync(id);
            return notification != null ? MapToDto(notification) : null;
        }

        public async Task<int> GetUnreadCountAsync(Guid userId)
        {
            return await _notificationRepository.GetUnreadCountByUserIdAsync(userId);
        }

        public async Task<(bool Success, string Message, int NotificationsSent)> BroadcastNotificationAsync(
            BroadcastNotificationDto dto, 
            Guid senderId)
        {
            try
            {
                _logger.LogInformation("🔔 Broadcasting notification: {Title}", dto.Title);

                // Determine target users
                List<Guid> targetUserIds = new List<Guid>();

                if (string.IsNullOrEmpty(dto.TargetRole) || dto.TargetRole.ToLower() == "all" || dto.TargetRole.ToLower() == "semua")
                {
                    // Broadcast to ALL users
                    _logger.LogInformation("📢 Broadcasting to ALL users");
                    var allUsers = await _context.Users
                        .Where(u => u.Id != senderId) // Exclude sender
                        .Select(u => u.Id)
                        .ToListAsync();
                    targetUserIds.AddRange(allUsers);
                }
                else
                {
                    // Broadcast to specific role
                    _logger.LogInformation("📢 Broadcasting to role: {Role}", dto.TargetRole);
                    var roleUsers = await _notificationRepository.GetUserIdsByRoleAsync(dto.TargetRole);
                    targetUserIds.AddRange(roleUsers.Where(id => id != senderId)); // Exclude sender
                }

                if (!targetUserIds.Any())
                {
                    return (false, "Tidak ada user yang menjadi target untuk notifikasi ini", 0);
                }

                _logger.LogInformation("Found {Count} users to notify", targetUserIds.Count);

                // Create notifications for all target users
                int notificationsSent = 0;
                foreach (var userId in targetUserIds)
                {
                    var notification = new Notification
                    {
                        Id = Guid.NewGuid(), // Explicitly set ID
                        UserId = userId,
                        Title = dto.Title ?? string.Empty,
                        Message = dto.Message ?? string.Empty,
                        Type = dto.Type ?? "info",
                        Priority = dto.Priority ?? "medium",
                        LinkUrl = dto.LinkUrl,
                        IsRead = false,
                        CreatedAt = DateTime.UtcNow,
                        UpdateAt = DateTime.UtcNow
                    };

                    _context.Notifications.Add(notification);
                    notificationsSent++;
                }

                await _context.SaveChangesAsync();

                var targetDescription = string.IsNullOrEmpty(dto.TargetRole) || dto.TargetRole.ToLower() == "all" 
                    ? "semua pengguna" 
                    : $"role {dto.TargetRole}";

                _logger.LogInformation("✅ Broadcast notification sent successfully to {Count} users", notificationsSent);

                return (true, $"Notifikasi berhasil dikirim ke {notificationsSent} pengguna ({targetDescription})", notificationsSent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error broadcasting notification");
                
                // Log inner exception for more details
                if (ex.InnerException != null)
                {
                    _logger.LogError(ex.InnerException, "❌ Inner Exception");
                    return (false, $"Gagal mengirim broadcast: {ex.InnerException.Message}", 0);
                }
                
                return (false, $"Gagal mengirim broadcast: {ex.Message}", 0);
            }
        }

        private NotificationResponseDto MapToDto(Notification notification)
        {
            return new NotificationResponseDto
            {
                Id = notification.Id,
                UserId = notification.UserId,
                Title = notification.Title,
                Message = notification.Message,
                Type = notification.Type,
                Priority = notification.Priority,
                LinkUrl = notification.LinkUrl,
                IsRead = notification.IsRead,
                CreatedAt = notification.CreatedAt,
                ReadAt = notification.ReadAt
            };
        }

        // Auto-notification helper methods
        public async Task NotifyAyamAddedAsync(Guid petugasId, string petugasName, string kandangName, int jumlahAyam, Guid kandangId)
        {
            // Not implemented yet
            await Task.CompletedTask;
        }

        public async Task NotifyMortalitasAsync(Guid petugasId, string petugasName, string kandangName, int jumlahMati, string penyebab, Guid kandangId)
        {
            // Not implemented yet
            await Task.CompletedTask;
        }

        public async Task NotifyPanenAsync(Guid petugasId, string petugasName, string kandangName, int jumlahPanen, decimal beratTotal, Guid kandangId)
        {
            try
            {
                _logger.LogInformation("🔔 Creating notification for panen");

                var pemilikIds = await _notificationRepository.GetUserIdsByRoleAsync("Pemilik");
                var operatorIds = await _notificationRepository.GetUserIdsByRoleAsync("Operator");

                var allSupervisors = pemilikIds.Concat(operatorIds).Distinct().ToList();

                _logger.LogInformation("Found {Count} supervisors to notify", allSupervisors.Count);

                foreach (var supervisorId in allSupervisors)
                {
                    if (supervisorId == petugasId) continue; // Skip sender

                    var notification = new Notification
                    {
                        UserId = supervisorId,
                        Title = "Panen Ayam Baru",
                        Message = $"{petugasName} melakukan panen di {kandangName}: {jumlahPanen} ekor (Total berat: {beratTotal:N2} kg)",
                        Type = "info",
                        Priority = "medium",
                        LinkUrl = $"/kandang/{kandangId}",
                        IsRead = false,
                        CreatedAt = DateTime.UtcNow,
                        UpdateAt = DateTime.UtcNow
                    };

                    _context.Notifications.Add(notification);
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("✅ Panen notifications created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error creating panen notifications");
                // Don't throw, notification is non-critical
            }
        }

        public async Task NotifyOperasionalAsync(Guid petugasId, string petugasName, string jenisKegiatan, string kandangName, Guid kandangId, Guid operasionalId)
        {
            // Not implemented yet
            await Task.CompletedTask;
        }

        public async Task NotifyBiayaAsync(Guid petugasId, string petugasName, string jenisBiaya, decimal jumlah, Guid? kandangId, Guid biayaId)
        {
            // Not implemented yet
            await Task.CompletedTask;
        }

        public async Task NotifyJurnalHarianAsync(Guid petugasId, string petugasName, string judulKegiatan, Guid? kandangId, Guid jurnalId)
        {
            try
            {
                _logger.LogInformation("🔔 Creating notification for jurnal harian");

                var pemilikIds = await _notificationRepository.GetUserIdsByRoleAsync("Pemilik");
                var operatorIds = await _notificationRepository.GetUserIdsByRoleAsync("Operator");

                var allSupervisors = pemilikIds.Concat(operatorIds).Distinct().ToList();

                _logger.LogInformation("Found {Count} supervisors to notify", allSupervisors.Count);

                foreach (var supervisorId in allSupervisors)
                {
                    if (supervisorId == petugasId) continue; // Skip sender

                    var notification = new Notification
                    {
                        UserId = supervisorId,
                        Title = "Jurnal Harian Baru",
                        Message = $"{petugasName} membuat jurnal harian: {judulKegiatan}",
                        Type = "info",
                        IsRead = false,
                        CreatedAt = DateTime.UtcNow,
                        UpdateAt = DateTime.UtcNow
                    };

                    _context.Notifications.Add(notification);
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("✅ Notifications created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error creating notifications");
                throw;
            }
        }
    }
}

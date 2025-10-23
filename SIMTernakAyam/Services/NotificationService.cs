using SIMTernakAyam.DTOs.Notification;
using SIMTernakAyam.Models;
using SIMTernakAyam.Repository.Interfaces;
using SIMTernakAyam.Services.Interfaces;
using SIMTernakAyam.Data;

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

        private NotificationResponseDto MapToDto(Notification notification)
        {
            return new NotificationResponseDto
            {
                Id = notification.Id,
                UserId = notification.UserId,
                Title = notification.Title,
                Message = notification.Message,
                Type = notification.Type,
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
            // Not implemented yet
            await Task.CompletedTask;
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

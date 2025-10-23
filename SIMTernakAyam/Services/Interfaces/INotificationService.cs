using SIMTernakAyam.DTOs.Notification;

namespace SIMTernakAyam.Services.Interfaces
{
    public interface INotificationService
    {
        Task<(IEnumerable<NotificationResponseDto> notifications, int total)> GetUserNotificationsAsync(
            Guid userId,
            bool? isRead = null,
            int page = 1,
            int limit = 10);

        Task<NotificationResponseDto> CreateNotificationAsync(Guid userId, string message);
        Task<bool> DeleteNotificationAsync(Guid id, Guid userId);
        Task<NotificationResponseDto?> MarkAsReadAsync(Guid id, Guid userId);
        Task<int> GetUnreadCountAsync(Guid userId);

        // Auto-notification helpers
        Task NotifyAyamAddedAsync(Guid petugasId, string petugasName, string kandangName, int jumlahAyam, Guid kandangId);
        Task NotifyMortalitasAsync(Guid petugasId, string petugasName, string kandangName, int jumlahMati, string penyebab, Guid kandangId);
        Task NotifyPanenAsync(Guid petugasId, string petugasName, string kandangName, int jumlahPanen, decimal beratTotal, Guid kandangId);
        Task NotifyOperasionalAsync(Guid petugasId, string petugasName, string jenisKegiatan, string kandangName, Guid kandangId, Guid operasionalId);
        Task NotifyBiayaAsync(Guid petugasId, string petugasName, string jenisBiaya, decimal jumlah, Guid? kandangId, Guid biayaId);
        Task NotifyJurnalHarianAsync(Guid petugasId, string petugasName, string judulKegiatan, Guid? kandangId, Guid jurnalId);
    }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIMTernakAyam.DTOs.Notification;
using SIMTernakAyam.Enums;
using SIMTernakAyam.Services.Interfaces;
using System.Security.Claims;

namespace SIMTernakAyam.Controllers
{
    [Authorize]
    [Route("api/notifications")]
    public class NotificationController : BaseController
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        /// <summary>
        /// GET /api/notifications - Ambil daftar notifikasi user yang sedang login
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetNotifications(
            [FromQuery] bool? is_read,
            [FromQuery] int page = 1,
            [FromQuery] int limit = 10)
        {
            try
            {
                var userId = GetCurrentUserId();
                var (notifications, total) = await _notificationService.GetUserNotificationsAsync(
                    userId, is_read, page, limit);

                var totalPages = (int)Math.Ceiling((double)total / limit);
                var response = new
                {
                    success = true,
                    message = "Berhasil mengambil notifikasi",
                    data = notifications,
                    pagination = new
                    {
                        page = page,
                        limit = limit,
                        total = total,
                        totalPages = totalPages
                    }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        /// <summary>
        /// POST /api/notifications - Kirim notifikasi ke user tertentu
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateNotification([FromBody] CreateNotificationDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var notification = await _notificationService.CreateNotificationAsync(dto.UserId, dto.Message);

                return Success(notification, "Notifikasi berhasil dikirim", 201);
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        /// <summary>
        /// PUT /api/notifications/{id}/read - Tandai notifikasi sebagai sudah dibaca
        /// </summary>
        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkAsRead(Guid id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var notification = await _notificationService.MarkAsReadAsync(id, userId);

                if (notification == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Notifikasi tidak ditemukan atau tidak memiliki akses"
                    });
                }

                return Success(notification, "Notifikasi ditandai sebagai sudah dibaca");
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        /// <summary>
        /// DELETE /api/notifications/{id} - Hapus notifikasi
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNotification(Guid id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var success = await _notificationService.DeleteNotificationAsync(id, userId);

                if (!success)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Notifikasi tidak ditemukan atau tidak memiliki akses"
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Notifikasi berhasil dihapus"
                });
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        /// <summary>
        /// GET /api/notifications/unread-count - Ambil jumlah notifikasi yang belum dibaca
        /// </summary>
        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            try
            {
                var userId = GetCurrentUserId();
                var count = await _notificationService.GetUnreadCountAsync(userId);

                return Success(new { count }, "Berhasil mengambil jumlah notifikasi belum dibaca");
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                throw new UnauthorizedAccessException("User tidak terautentikasi");
            }
            return Guid.Parse(userIdClaim);
        }
    }
}

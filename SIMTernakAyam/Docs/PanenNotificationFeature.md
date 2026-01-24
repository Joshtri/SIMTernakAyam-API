# Fitur Notifikasi Panen Otomatis

## Overview

Fitur ini secara otomatis mengirimkan notifikasi kepada **Pemilik** dan **Operator** ketika ada aktivitas panen ayam di kandang. Notifikasi membantu pemilik untuk selalu update tentang aktivitas panen tanpa harus membuka aplikasi secara berkala.

## ?? Tujuan

- Memberikan informasi real-time kepada Pemilik tentang aktivitas panen
- Meningkatkan transparansi operasional peternakan
- Memudahkan monitoring tanpa harus selalu login ke sistem
- Tracking histori panen melalui notifikasi

## ?? Spesifikasi

### Trigger Events

Notifikasi akan dikirim otomatis saat:
1. **Panen Manual** dibuat (POST `/api/panen`)
2. **Panen Auto FIFO** dibuat (POST `/api/panen/auto-fifo`)
3. **Panen Manual Split** dibuat (POST `/api/panen/manual-split`)

### Target Recipients

- **Role: Pemilik** ? Menerima semua notifikasi panen
- **Role: Operator** ? Menerima notifikasi panen (kecuali yang dibuat oleh diri sendiri)

### Notification Content

```
Title: "Panen Ayam Baru"
Message: "{Nama Petugas} melakukan panen di {Nama Kandang}: {Jumlah Ekor} ekor (Total berat: {Total Berat} kg)"
Type: "info"
Priority: "medium"
LinkUrl: "/kandang/{kandangId}"
```

**Contoh:**
```
Title: "Panen Ayam Baru"
Message: "John Doe melakukan panen di Kandang A: 100 ekor (Total berat: 180.00 kg)"
Type: "info"
Priority: "medium"
LinkUrl: "/kandang/1bc6ba27-0e92-42aa-b3fa-67b3e78e2cf4"
```

## ?? Implementasi Teknis

### Backend Changes

#### 1. NotificationService.cs

```csharp
public async Task NotifyPanenAsync(
    Guid petugasId, 
    string petugasName, 
    string kandangName, 
    int jumlahPanen, 
    decimal beratTotal, 
    Guid kandangId)
{
    try
    {
        _logger.LogInformation("?? Creating notification for panen");

        // Get all Pemilik and Operator users
        var pemilikIds = await _notificationRepository.GetUserIdsByRoleAsync("Pemilik");
        var operatorIds = await _notificationRepository.GetUserIdsByRoleAsync("Operator");

        var allSupervisors = pemilikIds.Concat(operatorIds).Distinct().ToList();

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
        _logger.LogInformation("? Panen notifications created successfully");
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "? Error creating panen notifications");
        // Don't throw, notification is non-critical
    }
}
```

#### 2. PanenService.cs

**Dependency Injection:**
```csharp
private readonly INotificationService _notificationService;
private readonly IKandangRepository _kandangRepository;
private readonly IUserRepository _userRepository;

public PanenService(
    // ... existing dependencies
    INotificationService notificationService,
    IKandangRepository kandangRepository,
    IUserRepository userRepository)
{
    // ... existing initializations
    _notificationService = notificationService;
    _kandangRepository = kandangRepository;
    _userRepository = userRepository;
}
```

**Lifecycle Hook:**
```csharp
protected override async Task AfterCreateAsync(Panen entity)
{
    // Send notification after successful panen
    await SendPanenNotificationAsync(entity);
}

private async Task SendPanenNotificationAsync(Panen panen)
{
    try
    {
        // Get ayam and kandang info
        var ayam = await _ayamRepository.GetByIdAsync(panen.AyamId);
        if (ayam == null) return;

        var kandang = await _kandangRepository.GetByIdAsync(ayam.KandangId);
        if (kandang == null) return;

        // Get petugas info
        var petugas = await _userRepository.GetByIdAsync(kandang.PetugasId);
        if (petugas == null) return;

        var totalBerat = panen.JumlahEkorPanen * panen.BeratRataRata;

        await _notificationService.NotifyPanenAsync(
            petugas.Id,
            petugas.NamaLengkap,
            kandang.NamaKandang,
            panen.JumlahEkorPanen,
            totalBerat,
            kandang.Id
        );
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error sending panen notification: {ex.Message}");
    }
}
```

**Auto FIFO & Manual Split:**
```csharp
// In CreatePanenAutoFifoAsync
if (panenList.Any())
{
    await SendAutoFifoPanenNotificationAsync(kandangId, jumlahEkorPanen, beratRataRata);
}

// In CreatePanenManualSplitAsync
if (panenList.Any())
{
    var totalBerat = totalJumlah * beratRataRata;
    await SendManualSplitPanenNotificationAsync(kandangId, totalJumlah, totalBerat);
}
```

## ?? Frontend Integration

### 1. Get Notifications

**Endpoint:**
```http
GET /api/notification?page=1&limit=20
Authorization: Bearer {pemilik_token}
```

**Response:**
```json
{
  "success": true,
  "message": "Berhasil mengambil 3 notifikasi.",
  "data": {
    "notifications": [
      {
        "id": "guid",
        "userId": "guid",
        "title": "Panen Ayam Baru",
        "message": "John Doe melakukan panen di Kandang A: 100 ekor (Total berat: 180.00 kg)",
        "type": "info",
        "priority": "medium",
        "linkUrl": "/kandang/1bc6ba27-0e92-42aa-b3fa-67b3e78e2cf4",
        "isRead": false,
        "createdAt": "2026-01-25T10:00:00Z",
        "readAt": null
      }
    ],
    "total": 3,
    "page": 1,
    "limit": 20,
    "totalPages": 1
  }
}
```

### 2. Get Unread Count

**Endpoint:**
```http
GET /api/notification/unread-count
Authorization: Bearer {pemilik_token}
```

**Response:**
```json
{
  "success": true,
  "message": "Anda memiliki 5 notifikasi yang belum dibaca.",
  "data": 5
}
```

### 3. Mark as Read

**Endpoint:**
```http
PUT /api/notification/{id}/read
Authorization: Bearer {pemilik_token}
```

**Response:**
```json
{
  "success": true,
  "message": "Notifikasi berhasil ditandai sudah dibaca.",
  "data": {
    "id": "guid",
    "isRead": true,
    "readAt": "2026-01-25T11:00:00Z"
  }
}
```

### UI Components (React/Next.js Example)

```typescript
// components/NotificationBell.tsx
import { useState, useEffect } from 'react';
import { Bell } from 'lucide-react';

interface Notification {
  id: string;
  title: string;
  message: string;
  type: string;
  priority: string;
  linkUrl?: string;
  isRead: boolean;
  createdAt: string;
}

export function NotificationBell() {
  const [notifications, setNotifications] = useState<Notification[]>([]);
  const [unreadCount, setUnreadCount] = useState(0);
  const [isOpen, setIsOpen] = useState(false);

  useEffect(() => {
    fetchUnreadCount();
    fetchNotifications();
    
    // Poll every 30 seconds
    const interval = setInterval(() => {
      fetchUnreadCount();
      fetchNotifications();
    }, 30000);

    return () => clearInterval(interval);
  }, []);

  const fetchUnreadCount = async () => {
    const response = await fetch('/api/notification/unread-count', {
      headers: {
        'Authorization': `Bearer ${localStorage.getItem('token')}`
      }
    });
    const data = await response.json();
    setUnreadCount(data.data);
  };

  const fetchNotifications = async () => {
    const response = await fetch('/api/notification?page=1&limit=10', {
      headers: {
        'Authorization': `Bearer ${localStorage.getItem('token')}`
      }
    });
    const data = await response.json();
    setNotifications(data.data.notifications);
  };

  const markAsRead = async (id: string) => {
    await fetch(`/api/notification/${id}/read`, {
      method: 'PUT',
      headers: {
        'Authorization': `Bearer ${localStorage.getItem('token')}`
      }
    });
    fetchUnreadCount();
    fetchNotifications();
  };

  return (
    <div className="relative">
      <button
        onClick={() => setIsOpen(!isOpen)}
        className="relative p-2 rounded-full hover:bg-gray-100"
      >
        <Bell className="w-6 h-6" />
        {unreadCount > 0 && (
          <span className="absolute top-0 right-0 inline-flex items-center justify-center px-2 py-1 text-xs font-bold leading-none text-white transform translate-x-1/2 -translate-y-1/2 bg-red-600 rounded-full">
            {unreadCount}
          </span>
        )}
      </button>

      {isOpen && (
        <div className="absolute right-0 mt-2 w-80 bg-white rounded-lg shadow-lg z-50">
          <div className="p-4 border-b">
            <h3 className="font-semibold">Notifikasi</h3>
          </div>
          <div className="max-h-96 overflow-y-auto">
            {notifications.length === 0 ? (
              <div className="p-4 text-center text-gray-500">
                Tidak ada notifikasi
              </div>
            ) : (
              notifications.map((notif) => (
                <div
                  key={notif.id}
                  className={`p-4 border-b cursor-pointer hover:bg-gray-50 ${
                    !notif.isRead ? 'bg-blue-50' : ''
                  }`}
                  onClick={() => {
                    markAsRead(notif.id);
                    if (notif.linkUrl) {
                      window.location.href = notif.linkUrl;
                    }
                  }}
                >
                  <div className="flex justify-between items-start">
                    <div className="flex-1">
                      <p className="font-semibold text-sm">{notif.title}</p>
                      <p className="text-sm text-gray-600 mt-1">{notif.message}</p>
                      <p className="text-xs text-gray-400 mt-1">
                        {new Date(notif.createdAt).toLocaleString('id-ID')}
                      </p>
                    </div>
                    {!notif.isRead && (
                      <span className="w-2 h-2 bg-blue-600 rounded-full ml-2"></span>
                    )}
                  </div>
                </div>
              ))
            )}
          </div>
        </div>
      )}
    </div>
  );
}
```

## ?? Testing

### Test Cases

1. **Basic Notification**
   - Create panen manual
   - Check notification created for Pemilik
   - Verify notification content

2. **Auto FIFO Notification**
   - Create panen auto FIFO
   - Verify notification sent with correct total

3. **Manual Split Notification**
   - Create panen manual split
   - Verify notification includes both ayam lama and baru

4. **Multiple Recipients**
   - Create panen with multiple Pemilik users
   - Verify all Pemilik receive notification

5. **Self-exclusion**
   - Operator creates panen
   - Verify Operator doesn't receive own notification

### Test HTTP File

See: `Tests/PanenNotificationTests.http`

## ?? Database Impact

### New Records

Setiap panen akan membuat 1 notifikasi untuk setiap Pemilik dan Operator (kecuali pembuat panen).

**Example:**
- 1 Pemilik user
- 2 Operator users (1 yang buat panen)
- Total notifikasi: 1 (Pemilik) + 1 (Operator lain) = 2 notifications

### Query to Check

```sql
-- Check notifications for specific user
SELECT * FROM "Notifications" 
WHERE "UserId" = 'user-guid-here' 
ORDER BY "CreatedAt" DESC;

-- Check all panen notifications
SELECT * FROM "Notifications" 
WHERE "Title" = 'Panen Ayam Baru' 
ORDER BY "CreatedAt" DESC;

-- Count unread notifications
SELECT COUNT(*) 
FROM "Notifications" 
WHERE "UserId" = 'user-guid-here' 
  AND "IsRead" = false;
```

## ?? UI/UX Recommendations

### Notification Display

1. **Bell Icon with Badge**
   - Show unread count on bell icon
   - Red badge for urgent notifications
   - Animate bell when new notification arrives

2. **Notification Dropdown**
   - Show recent 10 notifications
   - Mark as read on click
   - Link to kandang detail page
   - Show relative time (e.g., "5 minutes ago")

3. **Notification Page**
   - Full list of all notifications
   - Filter by type/priority
   - Bulk mark as read
   - Pagination

### Real-time Updates

Consider using:
- **WebSockets** for real-time push notifications
- **SSE (Server-Sent Events)** for one-way updates
- **Polling** (30-60 seconds) for simple implementation

## ?? Future Enhancements

1. **Push Notifications**
   - Web Push API for browser notifications
   - Mobile push notifications (Firebase)

2. **Email Notifications**
   - Daily summary email
   - Configurable notification preferences

3. **SMS Notifications**
   - Critical alerts via SMS
   - For urgent situations

4. **Notification Preferences**
   - User can choose which notifications to receive
   - Quiet hours configuration
   - Notification frequency settings

5. **Notification Categories**
   - Group by category (Panen, Mortalitas, Operasional, etc.)
   - Different icon/color for each category

## ?? Troubleshooting

### Notifications not showing up?

1. **Check user role**
   ```sql
   SELECT "Role" FROM "Users" WHERE "Id" = 'user-guid';
   ```

2. **Check notification records**
   ```sql
   SELECT * FROM "Notifications" 
   WHERE "Title" = 'Panen Ayam Baru' 
   ORDER BY "CreatedAt" DESC 
   LIMIT 10;
   ```

3. **Check logs**
   - Look for "?? Creating notification for panen"
   - Look for "? Panen notifications created successfully"
   - Look for any error messages

4. **Verify dependencies**
   - INotificationService is registered in DI
   - IKandangRepository is registered
   - IUserRepository is registered

### Notification sent but not displayed?

1. **Check frontend API call**
   - Verify token is valid
   - Check response from `/api/notification`

2. **Check UserId**
   - Notification UserId matches logged-in user

3. **Check IsRead flag**
   - Filter might be hiding read notifications

## ?? Changelog

### v1.0.0 (2026-01-25)
- ? Implemented automatic panen notifications
- ? Support for manual, auto FIFO, and manual split panen
- ? Notifications sent to Pemilik and Operator roles
- ? LinkUrl to kandang detail page
- ? Non-blocking notification (errors don't break panen creation)

## ?? Credits

- Backend: Notification service implementation
- Testing: HTTP test file created
- Documentation: Feature documentation

---

**Need help?** Contact the development team or open an issue on GitHub.

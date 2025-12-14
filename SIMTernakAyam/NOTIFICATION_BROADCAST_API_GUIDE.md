# ?? NOTIFICATION BROADCAST API - DOCUMENTATION

## ?? **OVERVIEW**

Endpoint broadcast notification memungkinkan admin (Pemilik/Operator) untuk mengirim notifikasi secara massal ke:
- **Semua pengguna** di sistem
- **Role tertentu** (Petugas, Operator, atau Pemilik)

---

## ?? **AUTHORIZATION**

- **Endpoint**: `POST /api/notifications/broadcast`
- **Authorization**: `Bearer Token` (Required)
- **Allowed Roles**: `Pemilik`, `Operator` only
- **Petugas**: ? Tidak diizinkan untuk broadcast

---

## ?? **REQUEST FORMAT**

### **Endpoint**
```http
POST https://localhost:7195/api/notifications/broadcast
Content-Type: application/json
Authorization: Bearer YOUR_TOKEN_HERE
```

### **Request Body**
```json
{
  "title": "Judul Notifikasi",
  "message": "Isi pesan notifikasi",
  "type": "info",
  "priority": "medium",
  "linkUrl": "/optional-link",
  "targetRole": "all"
}
```

### **Field Descriptions**

| Field | Type | Required | Description | Valid Values |
|-------|------|----------|-------------|--------------|
| `title` | string | ? Yes | Judul notifikasi (max 200 char) | Any string |
| `message` | string | ? Yes | Isi pesan (max 1000 char) | Any string |
| `type` | string | ? Yes | Tipe notifikasi | `info`, `warning`, `error`, `success`, `reminder`, `system`, `message` |
| `priority` | string | ? Yes | Tingkat prioritas | `low`, `medium`, `high`, `urgent` |
| `linkUrl` | string | ? No | Link ke halaman detail (optional) | Any valid URL/path |
| `targetRole` | string | ? No | Target penerima notifikasi | `all`, `Petugas`, `Operator`, `Pemilik` (default: `all`) |

---

## ?? **RESPONSE FORMAT**

### **Success Response (201 Created)**
```json
{
  "success": true,
  "message": "Notifikasi berhasil dikirim ke 15 pengguna (semua pengguna)",
  "data": {
    "notificationsSent": 15,
    "targetRole": "all",
    "title": "Pengumuman Penting",
    "message": "Sistem akan maintenance pada hari Minggu..."
  },
  "errors": null,
  "statusCode": 201,
  "timestamp": "2024-12-14T10:30:00+07:00"
}
```

### **Error Responses**

#### **Validation Error (400 Bad Request)**
```json
{
  "success": false,
  "message": "Validation failed",
  "errors": {
    "Title": ["Title harus diisi"],
    "Message": ["Message harus diisi"]
  },
  "statusCode": 400,
  "timestamp": "2024-12-14T10:30:00+07:00"
}
```

#### **Unauthorized (401)**
```json
{
  "success": false,
  "message": "Unauthorized",
  "statusCode": 401,
  "timestamp": "2024-12-14T10:30:00+07:00"
}
```

#### **Forbidden (403)**
```json
{
  "success": false,
  "message": "User tidak memiliki akses untuk melakukan broadcast",
  "statusCode": 403,
  "timestamp": "2024-12-14T10:30:00+07:00"
}
```

#### **No Target Users (400)**
```json
{
  "success": false,
  "message": "Tidak ada user yang menjadi target untuk notifikasi ini",
  "statusCode": 400,
  "timestamp": "2024-12-14T10:30:00+07:00"
}
```

---

## ?? **NOTIFICATION TYPES & PRIORITIES**

### **Types**

| Type | Icon | Use Case | Color Hint |
|------|------|----------|------------|
| `info` | ?? | Informasi umum | Blue |
| `warning` | ?? | Peringatan | Yellow/Orange |
| `error` | ? | Error/masalah urgent | Red |
| `success` | ? | Berita baik/pencapaian | Green |
| `reminder` | ?? | Pengingat tugas | Blue |
| `system` | ?? | Update sistem | Gray |
| `message` | ?? | Pesan personal | Purple |

### **Priorities**

| Priority | Icon | Use Case | Behavior Hint |
|----------|------|----------|---------------|
| `low` | ?? | Info tidak mendesak | Normal notification |
| `medium` | ?? | Info penting tapi tidak urgent | Standard alert |
| `high` | ?? | Perlu perhatian segera | Highlighted |
| `urgent` | ?? | Sangat urgent/darurat | Popup alert + sound |

---

## ?? **USE CASES & EXAMPLES**

### **1. Pengumuman Sistem (All Users)**
```json
{
  "title": "?? Maintenance Terjadwal",
  "message": "Sistem akan maintenance pada Minggu, 15 Des 2024 pukul 22:00 - 23:00 WIB",
  "type": "system",
  "priority": "high",
  "linkUrl": null,
  "targetRole": "all"
}
```

### **2. Reminder untuk Petugas**
```json
{
  "title": "?? Reminder: Jadwal Pembersihan",
  "message": "Hari ini jadwal pembersihan menyeluruh semua kandang. Pastikan selesai sebelum sore.",
  "type": "reminder",
  "priority": "medium",
  "linkUrl": "/operasional",
  "targetRole": "Petugas"
}
```

### **3. Update Harga Pasar (All Users)**
```json
{
  "title": "?? Update Harga Pasar Ayam",
  "message": "Harga pasar ayam broiler naik menjadi Rp 28.000/kg per hari ini",
  "type": "success",
  "priority": "high",
  "linkUrl": "/harga-pasar",
  "targetRole": "all"
}
```

### **4. Alert Mortalitas Tinggi (All Users)**
```json
{
  "title": "?? Alert: Mortalitas Tinggi",
  "message": "Mortalitas di Kandang C mencapai 5% minggu ini. Segera lakukan pengecekan!",
  "type": "warning",
  "priority": "urgent",
  "linkUrl": "/mortalitas",
  "targetRole": "all"
}
```

### **5. Laporan untuk Pemilik**
```json
{
  "title": "?? Laporan Bulanan Tersedia",
  "message": "Laporan keuntungan bulan November 2024 sudah dapat dilihat",
  "type": "info",
  "priority": "high",
  "linkUrl": "/laporan/bulanan?bulan=11&tahun=2024",
  "targetRole": "Pemilik"
}
```

---

## ?? **WORKFLOW**

```
???????????????????????????????????????????????????????????????
?  Admin (Pemilik/Operator) Creates Broadcast                 ?
?  POST /api/notifications/broadcast                          ?
???????????????????????????????????????????????????????????????
                            ?
                            ?
???????????????????????????????????????????????????????????????
?  System Determines Target Users                             ?
?  - If targetRole = "all" ? Get all users                   ?
?  - If targetRole = specific role ? Get users by role       ?
?  - Exclude sender from notification                         ?
???????????????????????????????????????????????????????????????
                            ?
                            ?
???????????????????????????????????????????????????????????????
?  Create Notification Records                                 ?
?  - Loop through each target user                            ?
?  - Create notification record in database                   ?
?  - Set isRead = false for all                               ?
???????????????????????????????????????????????????????????????
                            ?
                            ?
???????????????????????????????????????????????????????????????
?  Return Success Response                                     ?
?  - notificationsSent count                                  ?
?  - targetRole description                                   ?
???????????????????????????????????????????????????????????????
                            ?
                            ?
???????????????????????????????????????????????????????????????
?  Users See Notifications                                     ?
?  - GET /api/notifications                                   ?
?  - Notification appears in their list                       ?
?  - Unread count increases                                   ?
???????????????????????????????????????????????????????????????
```

---

## ?? **TESTING GUIDE**

### **Prerequisites**
1. Have at least 2 users with different roles in database
2. Login as Pemilik or Operator to get token
3. Use the token for broadcast requests

### **Test Scenarios**

#### **1. Basic Broadcast (All Users)**
```http
POST /api/notifications/broadcast
Authorization: Bearer YOUR_TOKEN

{
  "title": "Test Broadcast",
  "message": "Testing broadcast to all users",
  "type": "info",
  "priority": "low",
  "targetRole": "all"
}
```
**Expected**: All users (except sender) receive notification

#### **2. Role-Specific Broadcast**
```http
POST /api/notifications/broadcast
Authorization: Bearer YOUR_TOKEN

{
  "title": "Test Petugas Only",
  "message": "This should only go to Petugas",
  "type": "info",
  "priority": "medium",
  "targetRole": "Petugas"
}
```
**Expected**: Only users with role "Petugas" receive notification

#### **3. Verify Notifications Received**
```http
GET /api/notifications?page=1&limit=10
Authorization: Bearer USER_TOKEN
```
**Expected**: Broadcasted notification appears in user's notification list

#### **4. Authorization Test (Should Fail)**
```http
# Login as Petugas first, then try to broadcast
POST /api/notifications/broadcast
Authorization: Bearer PETUGAS_TOKEN

{
  "title": "Should Fail",
  "message": "Petugas cannot broadcast",
  "type": "info",
  "priority": "low",
  "targetRole": "all"
}
```
**Expected**: 403 Forbidden error

---

## ?? **BEST PRACTICES**

### **1. Title Guidelines**
- ? Keep it short and descriptive (< 50 chars recommended)
- ? Use emojis for quick visual identification
- ? Start with action word or category
- ? Avoid all caps (except for URGENT cases)

### **2. Message Guidelines**
- ? Clear and concise
- ? Include actionable information
- ? Mention deadline if applicable
- ? Avoid technical jargon for general broadcasts

### **3. Type Selection**
- Use `info` for general announcements
- Use `warning` for issues that need attention
- Use `error` for urgent problems
- Use `success` for positive news
- Use `reminder` for tasks/schedules
- Use `system` for technical updates

### **4. Priority Selection**
- `low`: Nice to know information
- `medium`: Should read within a day
- `high`: Should read within hours
- `urgent`: Read immediately

### **5. Target Role Selection**
- Use `all` sparingly (avoid notification fatigue)
- Target specific roles when possible
- Consider creating separate broadcasts for different audiences

---

## ?? **TROUBLESHOOTING**

### **Problem: Broadcast returns 0 notifications sent**
**Solution**: Check if there are users with the target role in database

### **Problem: 403 Forbidden error**
**Solution**: Ensure user has Pemilik or Operator role

### **Problem: Users not receiving notifications**
**Solution**: 
1. Check if broadcast was successful (notificationsSent > 0)
2. Verify users are not the sender (sender excluded)
3. Check database for notification records

### **Problem: Validation errors**
**Solution**: Ensure all required fields are provided with valid values

---

## ?? **DATABASE SCHEMA**

### **Notification Model**
```csharp
public class Notification : BaseModel
{
    public Guid UserId { get; set; }
    public string Title { get; set; }
    public string Message { get; set; }
    public string Type { get; set; }
    public string Priority { get; set; }
    public string? LinkUrl { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public User? User { get; set; }
}
```

---

## ?? **IMPLEMENTATION CHECKLIST**

- [x] Create `BroadcastNotificationDto`
- [x] Add `Priority` and `LinkUrl` fields to `Notification` model
- [x] Create migration for new fields
- [x] Implement `BroadcastNotificationAsync` in service
- [x] Add broadcast endpoint in controller
- [x] Add authorization (`[Authorize(Roles = "Pemilik,Operator")]`)
- [x] Create test file with various scenarios
- [x] Update `NotificationResponseDto` to include new fields
- [x] Test with different roles and scenarios

---

## ?? **RELATED ENDPOINTS**

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/api/notifications` | GET | Get user notifications |
| `/api/notifications/{id}/read` | PUT | Mark notification as read |
| `/api/notifications/{id}` | DELETE | Delete notification |
| `/api/notifications/unread-count` | GET | Get unread count |
| `/api/notifications/broadcast` | POST | **Broadcast notification** |

---

**Created**: 2024-12-14  
**Status**: ? Implemented & Ready to Use  
**Test File**: `SIMTernakAyam/Tests/NotificationBroadcastTests.http`

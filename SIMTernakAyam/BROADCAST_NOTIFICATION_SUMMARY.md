# ?? BROADCAST NOTIFICATION - QUICK SUMMARY

## ? **STATUS: IMPLEMENTED**

Endpoint `/api/notifications/broadcast` **SUDAH DITAMBAHKAN** ke sistem!

---

## ?? **QUICK START**

### **1. Endpoint**
```http
POST https://localhost:7195/api/notifications/broadcast
```

### **2. Authorization**
- **Required**: Bearer Token
- **Allowed Roles**: `Pemilik`, `Operator` only
- ? **Petugas**: Tidak diizinkan

### **3. Request Example**
```json
{
  "title": "Pengumuman Penting",
  "message": "Sistem akan maintenance hari Minggu",
  "type": "info",
  "priority": "high",
  "linkUrl": null,
  "targetRole": "all"
}
```

### **4. Response Example**
```json
{
  "success": true,
  "message": "Notifikasi berhasil dikirim ke 15 pengguna (semua pengguna)",
  "data": {
    "notificationsSent": 15,
    "targetRole": "all",
    "title": "Pengumuman Penting",
    "message": "Sistem akan maintenance hari Minggu"
  }
}
```

---

## ?? **FIELDS**

| Field | Required | Values |
|-------|----------|--------|
| `title` | ? | Any string (max 200) |
| `message` | ? | Any string (max 1000) |
| `type` | ? | `info`, `warning`, `error`, `success`, `reminder`, `system`, `message` |
| `priority` | ? | `low`, `medium`, `high`, `urgent` |
| `linkUrl` | ? | Optional URL/path |
| `targetRole` | ? | `all`, `Petugas`, `Operator`, `Pemilik` (default: `all`) |

---

## ?? **TARGET OPTIONS**

### **Broadcast ke Semua Pengguna**
```json
{
  "targetRole": "all"
  // atau kosongkan field ini
}
```

### **Broadcast ke Role Tertentu**
```json
{
  "targetRole": "Petugas"  // Hanya petugas
}
```
```json
{
  "targetRole": "Operator"  // Hanya operator
}
```
```json
{
  "targetRole": "Pemilik"   // Hanya pemilik
}
```

---

## ?? **USE CASES**

### **1. Pengumuman Sistem**
```json
{
  "title": "?? Maintenance Terjadwal",
  "message": "Sistem akan maintenance Minggu 22:00-23:00 WIB",
  "type": "system",
  "priority": "high",
  "targetRole": "all"
}
```

### **2. Reminder untuk Petugas**
```json
{
  "title": "?? Pembersihan Kandang",
  "message": "Jangan lupa pembersihan kandang hari ini",
  "type": "reminder",
  "priority": "medium",
  "targetRole": "Petugas"
}
```

### **3. Update Harga Pasar**
```json
{
  "title": "?? Harga Pasar Update",
  "message": "Harga ayam naik jadi Rp 28.000/kg",
  "type": "success",
  "priority": "high",
  "linkUrl": "/harga-pasar",
  "targetRole": "all"
}
```

### **4. Alert Urgent**
```json
{
  "title": "?? URGENT: Wabah Penyakit",
  "message": "Ditemukan gejala penyakit di Kandang A3!",
  "type": "error",
  "priority": "urgent",
  "linkUrl": "/kandang/a3",
  "targetRole": "all"
}
```

---

## ?? **CHANGES MADE**

### **1. Model Update**
- ? Added `Priority` field to `Notification` model
- ? Added `LinkUrl` field to `Notification` model
- ? Created migration: `AddPriorityAndLinkUrlToNotification`

### **2. Service Implementation**
- ? Added `BroadcastNotificationAsync` method to `INotificationService`
- ? Implemented broadcast logic in `NotificationService`
- ? Support for role-based targeting
- ? Exclude sender from receiving notification

### **3. Controller**
- ? Added `POST /api/notifications/broadcast` endpoint
- ? Added `[Authorize(Roles = "Pemilik,Operator")]` for security
- ? Validation for request DTO

### **4. DTOs**
- ? Created `BroadcastNotificationDto` with validation
- ? Updated `NotificationResponseDto` to include `Priority` and `LinkUrl`

---

## ?? **TESTING**

Test file tersedia di:
```
SIMTernakAyam/Tests/NotificationBroadcastTests.http
```

### **Test Scenarios Included:**
1. ? Broadcast to all users
2. ? Broadcast to specific role (Petugas, Operator, Pemilik)
3. ? Different types (info, warning, error, success, reminder)
4. ? Different priorities (low, medium, high, urgent)
5. ? Validation tests (missing fields, invalid values)
6. ? Authorization tests (unauthorized, wrong role)
7. ? Real-world scenarios

---

## ?? **DOCUMENTATION**

Full documentation tersedia di:
```
SIMTernakAyam/NOTIFICATION_BROADCAST_API_GUIDE.md
```

Includes:
- Complete API reference
- Field descriptions
- Use cases & examples
- Workflow diagrams
- Best practices
- Troubleshooting guide

---

## ?? **READY TO USE!**

Endpoint sudah **SIAP DIGUNAKAN**. Silakan:
1. Run migration: `dotnet ef database update`
2. Test menggunakan file `.http`
3. Integrate ke frontend

---

**Created**: 2024-12-14  
**Status**: ? Fully Implemented  
**Next Step**: Run migration & Test

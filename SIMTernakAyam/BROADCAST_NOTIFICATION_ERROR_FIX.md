# ?? TROUBLESHOOTING: Broadcast Notification Error 400

## ? **ERROR YANG TERJADI**

```json
{
    "success": false,
    "message": "Gagal mengirim broadcast: An error occurred while saving the entity changes. See the inner exception for details."
}
```

**Endpoint**: `POST /api/notifications/broadcast`  
**Status Code**: 400 Bad Request

---

## ?? **ROOT CAUSE ANALYSIS**

### **Penyebab Utama**

Error ini terjadi karena:

1. **Migration belum dijalankan** - Field `Priority` dan `LinkUrl` belum ada di database
2. **Data existing notification** memiliki `Priority` yang null atau empty string
3. **Constraint violation** saat menyimpan notification baru

---

## ? **SOLUSI LENGKAP**

### **Step 1: Run Migration**

```bash
# Pastikan Anda di directory root project
cd SIMTernakAyam

# Run migration untuk menambahkan field Priority dan LinkUrl
dotnet ef database update
```

**Expected Output:**
```
Applying migration '20251214142316_AddPriorityAndLinkUrlToNotification'.
ALTER TABLE "Notifications" ADD "LinkUrl" text;
ALTER TABLE "Notifications" ADD "Priority" text NOT NULL DEFAULT '';
Done.
```

---

### **Step 2: Fix Existing Data**

Ada 2 cara untuk fix existing notifications dengan Priority empty:

#### **Option A: Using SQL Script**

```sql
-- File: Scripts/FixNotificationPriority.sql
-- Run this in pgAdmin or psql

UPDATE "Notifications"
SET "Priority" = 'medium'
WHERE "Priority" IS NULL OR "Priority" = '';
```

#### **Option B: Using Entity Framework**

Buat migration baru yang akan update data existing:

```csharp
// This is automatically handled by EF Core
// Migration akan set default value untuk Priority
```

---

### **Step 3: Verify Database Schema**

Check jika field sudah ada:

```sql
-- Check table structure
SELECT column_name, data_type, column_default 
FROM information_schema.columns 
WHERE table_name = 'Notifications' 
AND column_name IN ('Priority', 'LinkUrl');
```

**Expected Result:**
| column_name | data_type | column_default |
|-------------|-----------|----------------|
| Priority | text | '' |
| LinkUrl | text | NULL |

---

### **Step 4: Test Broadcast Again**

```http
POST https://localhost:7195/api/notifications/broadcast
Content-Type: application/json
Authorization: Bearer YOUR_TOKEN

{
  "title": "Test Broadcast",
  "message": "Testing after fix",
  "type": "info",
  "priority": "medium",
  "targetRole": "all"
}
```

**Expected Response (Success):**
```json
{
  "success": true,
  "message": "Notifikasi berhasil dikirim ke 5 pengguna (semua pengguna)",
  "data": {
    "notificationsSent": 5,
    "targetRole": "all",
    "title": "Test Broadcast",
    "message": "Testing after fix"
  },
  "statusCode": 201
}
```

---

## ?? **DEBUGGING CHECKLIST**

### **1. Check Migration Status**

```bash
dotnet ef migrations list
```

Look for:
- ? `20251214142316_AddPriorityAndLinkUrlToNotification` (applied)
- ? `20251214143046_FixNotificationPriorityDefault` (applied)

---

### **2. Check Database**

```sql
-- Check if table exists
SELECT EXISTS (
  SELECT FROM information_schema.tables 
  WHERE table_name = 'Notifications'
);

-- Check if Priority column exists
SELECT column_name 
FROM information_schema.columns 
WHERE table_name = 'Notifications' 
AND column_name = 'Priority';

-- Check existing notifications
SELECT "Id", "Title", "Priority", "LinkUrl" 
FROM "Notifications" 
LIMIT 5;
```

---

### **3. Check User Data**

Jika error "Tidak ada user yang menjadi target":

```sql
-- Check if there are users in database
SELECT "Id", "Username", "Role" 
FROM "Users";

-- Check users by role
SELECT COUNT(*) as total, "Role"
FROM "Users"
GROUP BY "Role";
```

---

### **4. Check Application Logs**

Lihat log aplikasi untuk detail error:

```
?? Broadcasting notification: Test Broadcast
?? Broadcasting to ALL users
Found 5 users to notify
? Broadcast notification sent successfully to 5 users
```

atau jika ada error:

```
? Error broadcasting notification
? Inner Exception: Cannot insert NULL into column 'Priority'
```

---

## ??? **COMMON ERRORS & FIXES**

### **Error 1: "Column 'Priority' does not exist"**

**Fix:**
```bash
dotnet ef database update
```

---

### **Error 2: "Cannot insert NULL into column 'Priority'"**

**Fix:**
```sql
UPDATE "Notifications"
SET "Priority" = 'medium'
WHERE "Priority" IS NULL OR "Priority" = '';
```

---

### **Error 3: "Tidak ada user yang menjadi target"**

**Cause:** Tidak ada user dengan role yang ditarget atau semua user adalah sender

**Fix:**
1. Check if there are users in database
2. Try dengan `targetRole: "all"`
3. Make sure ada user selain yang login

```sql
-- Add test user if needed
INSERT INTO "Users" ("Id", "Username", "Email", "Password", "Role", "CreatedAt", "UpdateAt")
VALUES (
  gen_random_uuid(),
  'testuser',
  'test@example.com',
  '$2a$11$hashedpassword', -- Use proper bcrypt hash
  'Petugas',
  NOW(),
  NOW()
);
```

---

### **Error 4: "403 Forbidden"**

**Cause:** User tidak memiliki role Pemilik atau Operator

**Fix:**
Login dengan user yang memiliki role `Pemilik` atau `Operator`:

```http
POST https://localhost:7195/api/auth/login
Content-Type: application/json

{
  "username": "admin",  // User with Pemilik or Operator role
  "password": "admin123"
}
```

---

### **Error 5: "401 Unauthorized"**

**Cause:** Token tidak valid atau expired

**Fix:**
1. Login ulang untuk mendapat token baru
2. Pastikan token di-include dalam Authorization header:
   ```
   Authorization: Bearer YOUR_TOKEN_HERE
   ```

---

## ?? **VERIFICATION STEPS**

### **1. After Fix, Verify Schema**

```sql
\d "Notifications"
```

**Expected:**
```
Column      | Type      | Nullable | Default
------------|-----------|----------|--------
Id          | uuid      | not null | 
UserId      | uuid      | not null | 
Title       | text      | not null | ''
Message     | text      | not null | ''
Type        | text      | not null | ''
Priority    | text      | not null | ''  <-- NEW
LinkUrl     | text      | null     |      <-- NEW
IsRead      | boolean   | not null | false
ReadAt      | timestamp | null     | 
CreatedAt   | timestamp | not null | 
UpdateAt    | timestamp | not null | 
```

---

### **2. Test with Different Scenarios**

```http
### Test 1: Minimal fields
POST /api/notifications/broadcast
{
  "title": "Test",
  "message": "Message",
  "type": "info",
  "priority": "medium"
}

### Test 2: With LinkUrl
POST /api/notifications/broadcast
{
  "title": "Test",
  "message": "Message",
  "type": "info",
  "priority": "high",
  "linkUrl": "/test"
}

### Test 3: Specific role
POST /api/notifications/broadcast
{
  "title": "Test",
  "message": "Message",
  "type": "info",
  "priority": "medium",
  "targetRole": "Petugas"
}
```

---

### **3. Check Notifications Created**

```sql
-- See latest notifications
SELECT 
  "Id",
  "UserId",
  "Title",
  "Type",
  "Priority",
  "LinkUrl",
  "CreatedAt"
FROM "Notifications"
ORDER BY "CreatedAt" DESC
LIMIT 10;
```

---

## ? **SUCCESS INDICATORS**

After fix, you should see:

1. ? Migration applied successfully
2. ? Priority column exists in database
3. ? All existing notifications have Priority value
4. ? Broadcast request returns 201 Created
5. ? Response shows `notificationsSent > 0`
6. ? New notifications visible in database
7. ? Users can see notifications in their list

---

## ?? **QUICK FIX SUMMARY**

```bash
# 1. Run migration
dotnet ef database update

# 2. Fix existing data (in PostgreSQL)
UPDATE "Notifications" 
SET "Priority" = 'medium' 
WHERE "Priority" IS NULL OR "Priority" = '';

# 3. Test broadcast
# Use file: Tests/NotificationBroadcastDebug.http

# 4. Verify
# Check database and application logs
```

---

## ?? **RELATED FILES**

| File | Description |
|------|-------------|
| `Scripts/FixNotificationPriority.sql` | SQL script to fix existing data |
| `Tests/NotificationBroadcastDebug.http` | Debug test suite |
| `Models/Notification.cs` | Model with Priority & LinkUrl |
| `Services/NotificationService.cs` | Service with improved error handling |
| `Migrations/AddPriorityAndLinkUrlToNotification.cs` | Migration file |

---

## ?? **PREVENTION**

To prevent this error in future:

1. **Always run migrations** after model changes
2. **Provide default values** for new required fields
3. **Test with existing data** before production
4. **Use proper error logging** to catch detailed errors
5. **Validate DTOs** before processing

---

**Fixed**: 2024-12-14  
**Status**: ? Resolved  
**Migration**: `AddPriorityAndLinkUrlToNotification` applied

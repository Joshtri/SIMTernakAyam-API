# FIX: Auto Notifikasi Saat Create Jurnal Harian

## Masalah:
1. Data jurnal harian POST berhasil tapi tidak tersimpan ke database
2. Tidak ada notifikasi otomatis saat buat jurnal harian

## Solusi Yang Sudah Diterapkan:

### 1. FIX BUG PENYIMPANAN DATA (JurnalHarianService.cs)

**File**: `Services/JurnalHarianService.cs`

**Line 61**: Tambah `SaveChangesAsync()` di method `CreateAsync`:
```csharp
await _jurnalRepository.AddAsync(jurnal);
await _jurnalRepository.SaveChangesAsync();  // ✅ ADDED
```

**Line 123**: Tambah `SaveChangesAsync()` di method `UpdateAsync`:
```csharp
_jurnalRepository.UpdateAsync(jurnal);
await _jurnalRepository.SaveChangesAsync();  // ✅ ADDED
```

### 2. INTEGRASI NOTIFIKASI OTOMATIS (JurnalHarianService.cs)

**Dependency Injection Ditambahkan:**
```csharp
private readonly IUserRepository _userRepository;
private readonly IKandangRepository _kandangRepository;
private readonly INotificationService _notificationService;
private readonly ILogger<JurnalHarianService> _logger;
```

**Kode Notifikasi Otomatis (Line 63-87):**
```csharp
// Send notification to supervisors (Pemilik & Operator)
try
{
    _logger.LogInformation("Mencoba membuat notifikasi untuk jurnal harian ID: {JurnalId}", jurnal.Id);

    var petugas = await _userRepository.GetByIdAsync(petugasId);
    _logger.LogInformation("Data petugas ditemukan: {PetugasName}", petugas?.FullName ?? "Tidak ditemukan");

    var petugasName = petugas?.FullName ?? "Petugas";

    await _notificationService.NotifyJurnalHarianAsync(
        petugasId,
        petugasName,
        dto.JudulKegiatan,
        dto.KandangId,
        jurnal.Id
    );

    _logger.LogInformation("✅ Notifikasi berhasil dikirim untuk jurnal harian ID: {JurnalId}", jurnal.Id);
}
catch (Exception ex)
{
    _logger.LogError(ex, "❌ Error saat membuat notifikasi untuk jurnal harian ID: {JurnalId}", jurnal.Id);
}
```

## Detail Notifikasi Yang Dibuat:

Saat POST `/api/jurnal-harian`, otomatis akan:

1. **Simpan data jurnal harian ke database** ✅
2. **Buat notifikasi ke SEMUA user dengan role:**
   - **Pemilik**
   - **Operator**

### Format Notifikasi:
```json
{
  "userId": "<ID Pemilik/Operator>",
  "senderId": "<ID Petugas yang buat jurnal>",
  "title": "Jurnal Harian Dibuat",
  "message": "{Nama Petugas} membuat jurnal harian: {Judul Kegiatan}",
  "type": "info",
  "priority": 2,  // medium (1=low, 2=medium, 3=high, 4=urgent)
  "linkUrl": "/jurnal/{jurnalId}",
  "metadata": "{\"kandangId\":\"...\",\"jurnalId\":\"...\",\"judulKegiatan\":\"...\",\"action\":\"jurnal\"}",
  "isRead": false
}
```

## Cara Test:

### 1. **Jalankan Aplikasi:**
```bash
cd C:\Users\LENOVO\source\repos\SIMTernakAyam\SIMTernakAyam
dotnet run
```

### 2. **POST Jurnal Harian:**
```http
POST http://localhost:5000/api/jurnal-harian
Content-Type: application/json
Authorization: Bearer {token}

{
  "tanggal": "2025-10-10T12:36",
  "judulKegiatan": "Pembersihan Kandang",
  "deskripsiKegiatan": "Membersihkan kandang ayam",
  "waktuMulai": "12:38",
  "waktuSelesai": "17:39",
  "kandangId": "fd082b84-41c8-45d4-8480-ddb918f74cca",
  "catatan": "Kandang sudah bersih"
}
```

### 3. **Periksa Log di Console:**
Harus muncul:
```
info: SIMTernakAyam.Services.JurnalHarianService[0]
      Mencoba membuat notifikasi untuk jurnal harian ID: {guid}
info: SIMTernakAyam.Services.JurnalHarianService[0]
      Data petugas ditemukan: {nama_petugas}
info: SIMTernakAyam.Services.JurnalHarianService[0]
      ✅ Notifikasi berhasil dikirim untuk jurnal harian ID: {guid}
```

### 4. **Cek Database:**
```sql
-- Cek jurnal harian tersimpan
SELECT * FROM jurnal_harian ORDER BY created_at DESC LIMIT 1;

-- Cek notifikasi terbuat
SELECT * FROM notifications ORDER BY created_at DESC LIMIT 5;

-- Cek notifikasi untuk Pemilik/Operator
SELECT n.*, u.full_name, u.role
FROM notifications n
JOIN users u ON n.user_id = u.id
WHERE n.title = 'Jurnal Harian Dibuat'
ORDER BY n.created_at DESC;
```

### 5. **Test API Notifikasi:**
```http
GET http://localhost:5000/api/notifications
Authorization: Bearer {token_pemilik_atau_operator}
```

## Troubleshooting:

### Jika Notifikasi Tidak Muncul:

1. **Cek log error di console** - ada `❌ Error` message?
2. **Cek ada user role Pemilik/Operator di database?**
   ```sql
   SELECT id, full_name, username, role FROM users WHERE role IN ('Pemilik', 'Operator');
   ```
3. **Cek table Notifications ada?**
   ```sql
   SELECT COUNT(*) FROM notifications;
   ```

### Jika Ada Error:

Check log di console untuk detail error message. Common issues:
- Database connection error
- User tidak ditemukan
- Notification repository error

## File Yang Dimodifikasi:

1. ✅ `Services/JurnalHarianService.cs` - Tambah notifikasi logic
2. ✅ `Services/NotificationService.cs` - Method `NotifyJurnalHarianAsync` sudah ada
3. ✅ `Repository/NotificationRepository.cs` - Semua method sudah lengkap
4. ✅ `Program.cs` - Dependency injection sudah terdaftar

## Status: ✅ SELESAI

Kode backend sudah siap. Tinggal test saja!

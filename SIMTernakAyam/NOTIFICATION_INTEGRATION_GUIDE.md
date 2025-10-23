# Panduan Integrasi Notifikasi Otomatis

## 📋 Overview
Sistem notifikasi otomatis telah dibuat di `NotificationService` dengan helper methods yang dapat dipanggil dari controller manapun ketika ada aktivitas penting.

## 🎯 Kapan Notifikasi Dikirim?

### 1. **Data Ayam** - `NotifyAyamAddedAsync()`
**Trigger**: Ketika petugas menambahkan ayam baru ke kandang
- **Priority**: Medium
- **Type**: Info
- **Dikirim ke**: Pemilik & Operator
- **Link**: `/kandang/{kandangId}`

### 2. **Mortalitas** - `NotifyMortalitasAsync()`
**Trigger**: Ketika ada laporan ayam mati
- **Priority**: **HIGH** (urgent)
- **Type**: Warning
- **Dikirim ke**: Pemilik & Operator
- **Link**: `/kandang/{kandangId}/mortalitas`

### 3. **Panen** - `NotifyPanenAsync()`
**Trigger**: Ketika petugas mencatat hasil panen
- **Priority**: Medium
- **Type**: Success
- **Dikirim ke**: Pemilik & Operator
- **Link**: `/kandang/{kandangId}/panen`

### 4. **Operasional** - `NotifyOperasionalAsync()`
**Trigger**: Ketika petugas melakukan kegiatan operasional (vaksinasi, pemberian pakan, dll)
- **Priority**: Low
- **Type**: Info
- **Dikirim ke**: Pemilik & Operator
- **Link**: `/operasional/{operasionalId}`

### 5. **Biaya** - `NotifyBiayaAsync()`
**Trigger**: Ketika ada pencatatan biaya
- **Priority**: **HIGH** (penting untuk keuangan)
- **Type**: Info
- **Dikirim ke**: Pemilik & Operator
- **Link**: `/biaya/{biayaId}`

### 6. **Jurnal Harian** - `NotifyJurnalHarianAsync()`
**Trigger**: Ketika petugas membuat jurnal harian
- **Priority**: Low
- **Type**: Info
- **Dikirim ke**: Pemilik & Operator
- **Link**: `/jurnal/{jurnalId}`

---

## 💻 Cara Implementasi di Controller

### Step 1: Inject INotificationService
```csharp
public class YourController : BaseController
{
    private readonly IYourService _yourService;
    private readonly INotificationService _notificationService;

    public YourController(
        IYourService yourService,
        INotificationService notificationService)
    {
        _yourService = yourService;
        _notificationService = notificationService;
    }
}
```

### Step 2: Panggil Helper Method Setelah Operasi Sukses

#### Contoh 1: AyamController - Tambah Ayam
```csharp
[HttpPost]
public async Task<IActionResult> CreateAyam([FromBody] CreateAyamDto dto)
{
    try
    {
        var userId = GetCurrentUserId();
        var ayam = await _ayamService.CreateAyamAsync(dto);

        // 🔔 KIRIM NOTIFIKASI
        var user = await _userService.GetByIdAsync(userId);
        var kandang = await _kandangService.GetByIdAsync(dto.KandangId);

        await _notificationService.NotifyAyamAddedAsync(
            userId,
            user.FullName ?? user.Username,
            kandang.NamaKandang,
            1, // jumlah ayam yang ditambahkan
            dto.KandangId
        );

        return Success(ayam, "Ayam berhasil ditambahkan", 201);
    }
    catch (Exception ex)
    {
        return Error(ex.Message);
    }
}
```

#### Contoh 2: MortalitasController - Lapor Kematian
```csharp
[HttpPost]
public async Task<IActionResult> CreateMortalitas([FromBody] CreateMortalitasDto dto)
{
    try
    {
        var userId = GetCurrentUserId();
        var mortalitas = await _mortalitasService.CreateMortalitasAsync(dto);

        // 🔔 KIRIM NOTIFIKASI (URGENT!)
        var user = await _userService.GetByIdAsync(userId);
        var ayam = await _ayamService.GetByIdAsync(dto.AyamId);
        var kandang = await _kandangService.GetByIdAsync(ayam.KandangId);

        await _notificationService.NotifyMortalitasAsync(
            userId,
            user.FullName ?? user.Username,
            kandang.NamaKandang,
            dto.JumlahMati,
            dto.Penyebab,
            ayam.KandangId
        );

        return Success(mortalitas, "Mortalitas berhasil dicatat", 201);
    }
    catch (Exception ex)
    {
        return Error(ex.Message);
    }
}
```

#### Contoh 3: PanenController - Catat Panen
```csharp
[HttpPost]
public async Task<IActionResult> CreatePanen([FromBody] CreatePanenDto dto)
{
    try
    {
        var userId = GetCurrentUserId();
        var panen = await _panenService.CreatePanenAsync(dto);

        // 🔔 KIRIM NOTIFIKASI
        var user = await _userService.GetByIdAsync(userId);
        var ayam = await _ayamService.GetByIdAsync(dto.AyamId);
        var kandang = await _kandangService.GetByIdAsync(ayam.KandangId);

        await _notificationService.NotifyPanenAsync(
            userId,
            user.FullName ?? user.Username,
            kandang.NamaKandang,
            dto.JumlahPanen,
            dto.BeratRataRata * dto.JumlahPanen, // total berat
            ayam.KandangId
        );

        return Success(panen, "Panen berhasil dicatat", 201);
    }
    catch (Exception ex)
    {
        return Error(ex.Message);
    }
}
```

#### Contoh 4: OperasionalController - Catat Kegiatan
```csharp
[HttpPost]
public async Task<IActionResult> CreateOperasional([FromBody] CreateOperasionalDto dto)
{
    try
    {
        var userId = GetCurrentUserId();
        var operasional = await _operasionalService.CreateOperasionalAsync(dto);

        // 🔔 KIRIM NOTIFIKASI
        var user = await _userService.GetByIdAsync(userId);
        var kandang = await _kandangService.GetByIdAsync(dto.KandangId);
        var jenisKegiatan = await _jenisKegiatanService.GetByIdAsync(dto.JenisKegiatanId);

        await _notificationService.NotifyOperasionalAsync(
            userId,
            user.FullName ?? user.Username,
            jenisKegiatan.NamaKegiatan,
            kandang.NamaKandang,
            dto.KandangId,
            operasional.Id
        );

        return Success(operasional, "Operasional berhasil dicatat", 201);
    }
    catch (Exception ex)
    {
        return Error(ex.Message);
    }
}
```

#### Contoh 5: BiayaController - Catat Biaya
```csharp
[HttpPost]
public async Task<IActionResult> CreateBiaya([FromBody] CreateBiayaDto dto)
{
    try
    {
        var userId = GetCurrentUserId();
        var biaya = await _biayaService.CreateBiayaAsync(dto);

        // 🔔 KIRIM NOTIFIKASI (HIGH PRIORITY!)
        var user = await _userService.GetByIdAsync(userId);

        await _notificationService.NotifyBiayaAsync(
            userId,
            user.FullName ?? user.Username,
            dto.JenisBiaya,
            dto.Jumlah,
            dto.KandangId, // bisa null
            biaya.Id
        );

        return Success(biaya, "Biaya berhasil dicatat", 201);
    }
    catch (Exception ex)
    {
        return Error(ex.Message);
    }
}
```

#### Contoh 6: JurnalHarianController - Buat Jurnal
```csharp
[HttpPost]
public async Task<IActionResult> CreateJurnal([FromBody] CreateJurnalHarianDto dto)
{
    try
    {
        var userId = GetCurrentUserId();
        var jurnal = await _jurnalHarianService.CreateJurnalAsync(dto);

        // 🔔 KIRIM NOTIFIKASI
        var user = await _userService.GetByIdAsync(userId);

        await _notificationService.NotifyJurnalHarianAsync(
            userId,
            user.FullName ?? user.Username,
            dto.JudulKegiatan,
            dto.KandangId, // bisa null
            jurnal.Id
        );

        return Success(jurnal, "Jurnal harian berhasil dibuat", 201);
    }
    catch (Exception ex)
    {
        return Error(ex.Message);
    }
}
```

---

## 🎨 Flow Notifikasi

```
Petugas Input Data
        ↓
Controller Create Method
        ↓
Service Create (Save to DB)
        ↓
✅ Success
        ↓
🔔 NotificationService.NotifyXxxAsync()
        ↓
Kirim ke Pemilik & Operator
        ↓
Mereka terima notifikasi real-time
```

---

## 🔍 Metadata yang Disimpan

Setiap notifikasi menyimpan metadata dalam format JSON:

```json
{
  "kandangId": "uuid",
  "jumlahAyam": 100,
  "action": "add_ayam"
}
```

Metadata ini bisa digunakan frontend untuk:
- Filter notifikasi by kandang
- Grouping notifikasi by action type
- Deep linking ke halaman detail
- Analytics & reporting

---

## ⚡ Best Practices

1. **Panggil notifikasi SETELAH operasi sukses**
   ```csharp
   var result = await _service.CreateAsync(dto);
   // ✅ Baru panggil notifikasi
   await _notificationService.NotifyXxxAsync(...);
   ```

2. **Jangan block response karena notifikasi**
   - Notifikasi dipanggil async tapi tidak di-await jika ingin faster response
   - Atau gunakan background job (Hangfire) untuk produksi

3. **Handle error gracefully**
   ```csharp
   try {
       await _notificationService.NotifyXxxAsync(...);
   } catch (Exception ex) {
       // Log error tapi jangan fail request
       _logger.LogWarning("Failed to send notification: {Error}", ex.Message);
   }
   ```

4. **User info caching**
   - Jika perlu, cache user info untuk avoid multiple DB calls

---

## 📊 Priority Levels

| Priority | Kasus | Warna (Frontend) |
|----------|-------|------------------|
| **LOW** | Jurnal Harian, Operasional rutin | 🔵 Blue |
| **MEDIUM** | Tambah Ayam, Panen | 🟢 Green |
| **HIGH** | Biaya, Mortalitas | 🟠 Orange |
| **URGENT** | Emergency (jika diperlukan) | 🔴 Red |

---

## 🚀 Next Steps

Setelah implementasi di controller:
1. Test manual dengan Postman/Swagger
2. Verifikasi notifikasi masuk ke database
3. Test di frontend untuk real-time display
4. Implementasi SignalR untuk push notification (opsional)

---

**Dibuat**: 22 Oktober 2025
**Auto-notification System**: v1.0

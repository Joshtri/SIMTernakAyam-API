# ?? Deprecation Notice: Auto FIFO untuk Mortalitas

## Ringkasan Perubahan

Method `CreateMortalitasAutoFifoAsync` **TIDAK LAGI MENGGUNAKAN FIFO** (First In First Out) karena dalam realita, mortalitas ayam tidak selalu mengikuti pola FIFO.

### Alasan Perubahan

1. **Mortalitas tidak selalu FIFO**: Yang mati bisa ayam baru atau ayam lama, tidak selalu yang paling tua duluan
2. **Berbeda dengan Panen**: Panen mungkin masih bisa menggunakan FIFO (harvest ayam terlama duluan), tapi mortalitas berbeda
3. **Kebutuhan Akurasi**: User perlu menentukan sendiri distribusi kematian untuk akurasi data yang lebih baik

---

## API Changes

### ? Method yang Deprecated

```csharp
// Endpoint: POST /api/mortalitas/auto-fifo
// DTO: CreateMortalitasAutoFifoDto

// Method ini sekarang akan return error message:
// "Method ini tidak menggunakan Auto FIFO lagi. 
//  Silakan gunakan CreateMortalitasManualSplitAsync untuk menentukan distribusi kematian, 
//  atau gunakan CreateAsync biasa dengan memilih AyamId secara manual."
```

### ? Method yang Harus Digunakan

#### 1. **Manual Split** (RECOMMENDED)

```csharp
// Endpoint: POST /api/mortalitas
// Mode: "manual-split"

{
  "kandangId": "guid",
  "tanggalKematian": "2024-01-15T10:00:00Z",
  "jumlahKematian": 10,
  "penyebabKematian": "Penyakit",
  "mode": "manual-split",
  "jumlahDariAyamLama": 6,
  "jumlahDariAyamBaru": 4,
  "fotoMortalitasBase64": "data:image/jpeg;base64,...", // optional
  "fotoMortalitasFileName": "mortalitas.jpg" // optional
}
```

**Keterangan:**
- `jumlahDariAyamLama`: Jumlah kematian dari ayam periode lama (ayam paling tua di kandang)
- `jumlahDariAyamBaru`: Jumlah kematian dari ayam periode baru (ayam paling baru di kandang)
- Total `jumlahDariAyamLama + jumlahDariAyamBaru` harus sama dengan `jumlahKematian`

**Response:**
```json
{
  "status": "success",
  "message": "Berhasil membuat 2 record mortalitas. Total kematian: 10 ekor (Ayam lama: 6, Ayam baru: 4).",
  "data": [
    {
      "id": "guid",
      "ayamId": "guid-ayam-lama",
      "jumlahKematian": 6,
      ...
    },
    {
      "id": "guid",
      "ayamId": "guid-ayam-baru",
      "jumlahKematian": 4,
      ...
    }
  ]
}
```

#### 2. **Manual Selection** (Alternative)

```csharp
// Endpoint: POST /api/mortalitas (gunakan CreateAsync biasa)
// Pilih AyamId secara manual

{
  "ayamId": "guid-yang-dipilih-manual",
  "tanggalKematian": "2024-01-15T10:00:00Z",
  "jumlahKematian": 10,
  "penyebabKematian": "Penyakit",
  "fotoMortalitasBase64": "data:image/jpeg;base64,...", // optional
  "fotoMortalitasFileName": "mortalitas.jpg" // optional
}
```

---

## Migration Guide

### Untuk Frontend Developer

#### Sebelum (Auto FIFO - DEPRECATED)
```javascript
// ? JANGAN GUNAKAN INI LAGI
const response = await fetch('/api/mortalitas/auto-fifo', {
  method: 'POST',
  body: JSON.stringify({
    kandangId: 'xxx',
    tanggalKematian: '2024-01-15T10:00:00Z',
    jumlahKematian: 10,
    penyebabKematian: 'Penyakit'
  })
});
```

#### Sesudah (Manual Split - RECOMMENDED)
```javascript
// ? GUNAKAN INI
const response = await fetch('/api/mortalitas', {
  method: 'POST',
  body: JSON.stringify({
    kandangId: 'xxx',
    tanggalKematian: '2024-01-15T10:00:00Z',
    jumlahKematian: 10,
    penyebabKematian: 'Penyakit',
    mode: 'manual-split',
    jumlahDariAyamLama: 6,  // User tentukan sendiri
    jumlahDariAyamBaru: 4   // User tentukan sendiri
  })
});
```

### UI/UX Recommendation

Buat form dengan 2 input field:
1. **Jumlah dari Ayam Lama** (slider/input)
2. **Jumlah dari Ayam Baru** (slider/input)
3. Auto-calculate total dan validasi bahwa total harus sama dengan jumlah kematian

Contoh UI:
```
???????????????????????????????????????????
? Total Kematian: 10 ekor                 ?
???????????????????????????????????????????
? Ayam Lama (masuk: 2023-01-01)          ?
? ???????????????? 6 ekor                ?
?                                         ?
? Ayam Baru (masuk: 2024-01-10)          ?
? ???????????????? 4 ekor                ?
???????????????????????????????????????????
? Total: 10 ekor ?                       ?
???????????????????????????????????????????
```

---

## Technical Details

### Service Layer Changes

```csharp
// File: MortalitasService.cs

// Method CreateMortalitasAutoFifoAsync sekarang return error
public async Task<(bool Success, string Message, List<Mortalitas>? Data)> 
    CreateMortalitasAutoFifoAsync(...)
{
    return (false, "Method ini tidak menggunakan Auto FIFO lagi...", null);
}

// Method CreateMortalitasManualSplitAsync - FULL IMPLEMENTATION
public async Task<(bool Success, string Message, List<Mortalitas>? Data)> 
    CreateMortalitasManualSplitAsync(
        Guid kandangId,
        DateTime tanggalKematian,
        int jumlahDariAyamLama,
        int jumlahDariAyamBaru,
        string penyebabKematian,
        IFormFile? fotoMortalitas = null)
{
    // 1. Validasi input
    // 2. Get ayam lama (oldest) dan ayam baru (newest) dari kandang
    // 3. Validasi stok tersedia untuk masing-masing ayam
    // 4. Create 2 record mortalitas (untuk ayam lama dan ayam baru)
    // 5. Return list hasil create
}
```

---

## FAQ

### Q: Apakah endpoint `/api/mortalitas/auto-fifo` akan dihapus?
**A:** Tidak dihapus untuk backward compatibility, tapi akan return error message. Sebaiknya segera migrate ke `manual-split`.

### Q: Apakah Panen juga tidak menggunakan FIFO?
**A:** Panen masih bisa menggunakan FIFO (pilih ayam terlama duluan untuk panen), tergantung keputusan business. Tapi untuk Mortalitas, FIFO tidak cocok karena yang mati bisa ayam baru atau lama.

### Q: Bagaimana kalau saya tetap ingin menggunakan logic FIFO?
**A:** Anda bisa set `jumlahDariAyamLama = total` dan `jumlahDariAyamBaru = 0` di manual-split. Tapi tetap harus manual, tidak otomatis.

### Q: Apakah CreateAsync biasa masih bisa digunakan?
**A:** Ya, masih bisa. Tapi Anda harus pilih `ayamId` secara manual (1 ayam per request).

---

## Testing

### Test Case: Manual Split

```http
POST /api/mortalitas
Content-Type: application/json

{
  "kandangId": "{{kandangId}}",
  "tanggalKematian": "2024-01-15T10:00:00Z",
  "jumlahKematian": 10,
  "penyebabKematian": "Penyakit X",
  "mode": "manual-split",
  "jumlahDariAyamLama": 6,
  "jumlahDariAyamBaru": 4
}
```

### Test Case: Auto FIFO (Should Fail)

```http
POST /api/mortalitas/auto-fifo
Content-Type: application/json

{
  "kandangId": "{{kandangId}}",
  "tanggalKematian": "2024-01-15T10:00:00Z",
  "jumlahKematian": 10,
  "penyebabKematian": "Penyakit X"
}

# Expected Response:
# {
#   "status": "error",
#   "message": "Method ini tidak menggunakan Auto FIFO lagi. Silakan gunakan CreateMortalitasManualSplitAsync..."
# }
```

---

## Changelog

### Version 2.0 (2024-01-15)
- ? Deprecated `CreateMortalitasAutoFifoAsync` (tidak menggunakan FIFO lagi)
- ? Full implementation `CreateMortalitasManualSplitAsync` (manual distribution)
- ?? Updated DTOs documentation
- ?? Changed default mode from `auto-fifo` to `manual-split`

### Version 1.0 (Previous)
- ? `CreateMortalitasAutoFifoAsync` menggunakan FIFO otomatis

---

## Contact

Jika ada pertanyaan atau butuh bantuan migration, silakan hubungi tim development.

# ✅ Jurnal Harian - Format Request Yang Benar

## 🔴 Masalah Sebelumnya

Error yang Anda dapat:
```json
{
    "errors": {
        "$.waktuMulai": [
            "The JSON value could not be converted to System.TimeSpan"
        ]
    }
}
```

**Penyebab**: Format waktu harus berupa **string** bukan TimeSpan object.

---

## ✅ FORMAT YANG BENAR

### POST /api/jurnal-harian

**URL**: `https://localhost:7195/api/jurnal-harian`

**Method**: `POST`

**Headers**:
```
Content-Type: application/json
Authorization: Bearer <your-jwt-token>
```

**Body (JSON)**:
```json
{
  "tanggal": "2025-10-22T00:00:00",
  "judulKegiatan": "Pembersihan Kandang A1",
  "deskripsiKegiatan": "Melakukan pembersihan menyeluruh pada kandang A1 termasuk penggantian alas dan pembersihan tempat makan",
  "waktuMulai": "08:00",
  "waktuSelesai": "10:30",
  "kandangId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "catatan": "Semua berjalan lancar, tidak ada masalah"
}
```

### ⚠️ PENTING - Format Waktu

Waktu harus berupa **STRING** dengan format:
- ✅ `"08:00"` (HH:mm)
- ✅ `"08:00:00"` (HH:mm:ss)
- ✅ `"14:30"`
- ✅ `"23:59:59"`

❌ **JANGAN** gunakan format ini:
- ❌ `"08:00:00.0000000"` (terlalu panjang)
- ❌ `8` (number)
- ❌ Object TimeSpan

---

## 📋 Field Wajib & Opsional

### Wajib (Required):
| Field | Type | Format | Contoh |
|-------|------|--------|--------|
| `tanggal` | string (datetime) | ISO 8601 | `"2025-10-22T00:00:00"` |
| `judulKegiatan` | string | Max 200 char | `"Pembersihan Kandang"` |
| `deskripsiKegiatan` | string | Max 1000 char | `"Deskripsi lengkap..."` |
| `waktuMulai` | string | HH:mm atau HH:mm:ss | `"08:00"` |
| `waktuSelesai` | string | HH:mm atau HH:mm:ss | `"10:30"` |

### Opsional:
| Field | Type | Format | Contoh |
|-------|------|--------|--------|
| `kandangId` | string (UUID) | GUID | `"3fa85f64-5717-4562-b3fc-2c963f66afa6"` |
| `catatan` | string | Max 500 char | `"Catatan tambahan"` |

---

## 💡 Contoh Request Lengkap

### 1️⃣ Dengan Kandang ID
```json
{
  "tanggal": "2025-10-22T08:00:00",
  "judulKegiatan": "Vaksinasi ND",
  "deskripsiKegiatan": "Melakukan vaksinasi Newcastle Disease pada semua ayam di kandang A1",
  "waktuMulai": "08:00",
  "waktuSelesai": "10:00",
  "kandangId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "catatan": "Vaksinasi berjalan lancar, tidak ada reaksi negatif"
}
```

### 2️⃣ Tanpa Kandang ID (Kegiatan Umum)
```json
{
  "tanggal": "2025-10-22T00:00:00",
  "judulKegiatan": "Rapat Koordinasi",
  "deskripsiKegiatan": "Rapat koordinasi dengan tim manajemen membahas strategi produksi bulan depan",
  "waktuMulai": "13:00",
  "waktuSelesai": "15:00"
}
```

### 3️⃣ Dengan Detik (HH:mm:ss)
```json
{
  "tanggal": "2025-10-22T00:00:00",
  "judulKegiatan": "Monitoring Suhu",
  "deskripsiKegiatan": "Monitoring suhu kandang setiap 30 menit",
  "waktuMulai": "06:00:00",
  "waktuSelesai": "18:30:00",
  "kandangId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "catatan": "Suhu stabil antara 28-32 derajat"
}
```

---

## 🔍 Response Sukses

```json
{
  "success": true,
  "message": "Jurnal harian berhasil dibuat.",
  "data": {
    "id": "123e4567-e89b-12d3-a456-426614174000",
    "tanggal": "2025-10-22T00:00:00",
    "judulKegiatan": "Pembersihan Kandang A1",
    "deskripsiKegiatan": "Melakukan pembersihan menyeluruh...",
    "waktuMulai": "08:00:00",
    "waktuSelesai": "10:30:00",
    "petugasId": "user-id-123",
    "petugasName": "Budi Santoso",
    "kandangId": "kandang-id-456",
    "kandangName": "Kandang A1",
    "catatan": "Semua berjalan lancar",
    "fotoKegiatan": null,
    "createdAt": "2025-10-22T08:15:30"
  },
  "statusCode": 201,
  "timestamp": "2025-10-22T08:15:30.123Z"
}
```

---

## ❌ Error Yang Mungkin Terjadi

### 1. Format Waktu Salah
```json
{
  "errors": {
    "WaktuMulai": [
      "Format waktu mulai harus HH:mm atau HH:mm:ss"
    ]
  }
}
```
✅ **Solusi**: Gunakan format `"08:00"` atau `"08:00:00"`

### 2. Waktu Selesai <= Waktu Mulai
```json
{
  "success": false,
  "message": "Waktu selesai harus lebih besar dari waktu mulai"
}
```
✅ **Solusi**: Pastikan `waktuSelesai` lebih besar dari `waktuMulai`

### 3. Field Required Kosong
```json
{
  "errors": {
    "JudulKegiatan": [
      "Judul kegiatan harus diisi"
    ]
  }
}
```
✅ **Solusi**: Isi semua field yang wajib (required)

### 4. Kandang ID Tidak Valid
```json
{
  "success": false,
  "message": "Kandang tidak ditemukan"
}
```
✅ **Solusi**: Pastikan `kandangId` valid atau set `null` jika kegiatan umum

### 5. Unauthorized (401)
```json
{
  "success": false,
  "message": "Unauthorized"
}
```
✅ **Solusi**: Pastikan JWT token valid dan belum expired

---

## 🧪 Testing dengan Postman

1. **Buka Postman**
2. **Method**: POST
3. **URL**: `https://localhost:7195/api/jurnal-harian`
4. **Headers**:
   - Add: `Content-Type: application/json`
   - Add: `Authorization: Bearer <your-token>`
5. **Body** → raw → JSON:
   ```json
   {
     "tanggal": "2025-10-22T00:00:00",
     "judulKegiatan": "Test Jurnal",
     "deskripsiKegiatan": "Test deskripsi kegiatan",
     "waktuMulai": "08:00",
     "waktuSelesai": "10:00",
     "catatan": "Test catatan"
   }
   ```
6. **Send**

---

## 🚀 Cara Dapatkan JWT Token

Jika belum punya token:

### POST /api/auth/login
```json
{
  "username": "your-username",
  "password": "your-password"
}
```

Response akan berisi `token`:
```json
{
  "success": true,
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "user": { ... }
  }
}
```

Copy token tersebut dan gunakan di header:
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

---

## 📝 Tips

1. ✅ Gunakan format `"08:00"` untuk waktu (lebih simple)
2. ✅ `kandangId` boleh `null` untuk kegiatan umum
3. ✅ `catatan` boleh kosong atau `null`
4. ✅ Pastikan `waktuSelesai > waktuMulai`
5. ✅ Format tanggal: `"2025-10-22T00:00:00"` atau `"2025-10-22"`

---

**Problem Solved! 🎉**

Updated: 22 Oktober 2025

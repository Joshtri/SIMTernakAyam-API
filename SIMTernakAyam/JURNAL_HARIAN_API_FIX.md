# ✅ Fix: Jurnal Harian API Endpoint

## 🔴 Masalah Sebelumnya

Endpoint `POST /api/jurnal-harian` mengembalikan **404 Not Found** karena:
- Controller hanya menerima `Content-Type: multipart/form-data`
- Jika Anda mengirim JSON (`Content-Type: application/json`), endpoint tidak ditemukan

## ✅ Solusi

Sekarang ada **2 endpoint berbeda** untuk fleksibilitas:

### 1️⃣ POST /api/jurnal-harian (JSON - Tanpa Foto)

**Untuk create jurnal harian tanpa upload foto**

**Content-Type**: `application/json`

**Request Body**:
```json
{
  "tanggal": "2025-10-22T00:00:00",
  "judulKegiatan": "Pembersihan Kandang",
  "deskripsiKegiatan": "Membersihkan kandang A1 secara menyeluruh",
  "waktuMulai": "08:00:00",
  "waktuSelesai": "10:00:00",
  "kandangId": "uuid-kandang-id",
  "catatan": "Semua berjalan lancar"
}
```

**Response**:
```json
{
  "success": true,
  "message": "Jurnal harian berhasil dibuat.",
  "data": {
    "id": "uuid",
    "tanggal": "2025-10-22T00:00:00",
    "judulKegiatan": "Pembersihan Kandang",
    "deskripsiKegiatan": "Membersihkan kandang A1 secara menyeluruh",
    "waktuMulai": "08:00:00",
    "waktuSelesai": "10:00:00",
    "petugasId": "uuid",
    "petugasName": "Budi Santoso",
    "kandangId": "uuid-kandang-id",
    "kandangName": "Kandang A1",
    "catatan": "Semua berjalan lancar",
    "fotoKegiatan": null,
    "createdAt": "2025-10-22T08:00:00"
  }
}
```

---

### 2️⃣ POST /api/jurnal-harian/with-photo (Form-Data - Dengan Foto)

**Untuk create jurnal harian dengan upload foto**

**Content-Type**: `multipart/form-data`

**Form Fields**:
- `tanggal`: 2025-10-22
- `judulKegiatan`: Pembersihan Kandang
- `deskripsiKegiatan`: Membersihkan kandang A1
- `waktuMulai`: 08:00:00
- `waktuSelesai`: 10:00:00
- `kandangId`: uuid-kandang-id
- `catatan`: Semua berjalan lancar
- `fotoKegiatan`: [FILE] (image file)

**Response**: Sama seperti endpoint JSON

---

## 📝 Update Endpoint

### 1️⃣ PUT /api/jurnal-harian/{id} (JSON - Tanpa Foto)

**Content-Type**: `application/json`

**Request Body**:
```json
{
  "tanggal": "2025-10-22T00:00:00",
  "judulKegiatan": "Pembersihan Kandang (Updated)",
  "deskripsiKegiatan": "Update deskripsi",
  "waktuMulai": "08:00:00",
  "waktuSelesai": "11:00:00",
  "kandangId": "uuid-kandang-id",
  "catatan": "Update catatan"
}
```

---

### 2️⃣ PUT /api/jurnal-harian/{id}/with-photo (Form-Data - Dengan Foto)

**Content-Type**: `multipart/form-data`

Form fields sama seperti create

---

## 🎯 Cara Pakai

### Dengan Postman/Thunder Client:

#### **Tanpa Foto (JSON)**:
1. Method: `POST`
2. URL: `https://localhost:7195/api/jurnal-harian`
3. Headers:
   - `Content-Type: application/json`
   - `Authorization: Bearer <your-token>`
4. Body > raw > JSON:
   ```json
   {
     "tanggal": "2025-10-22",
     "judulKegiatan": "Test Jurnal",
     "deskripsiKegiatan": "Deskripsi test",
     "waktuMulai": "08:00:00",
     "waktuSelesai": "10:00:00",
     "kandangId": "your-kandang-id",
     "catatan": "Catatan test"
   }
   ```

#### **Dengan Foto (Form-Data)**:
1. Method: `POST`
2. URL: `https://localhost:7195/api/jurnal-harian/with-photo`
3. Headers:
   - `Authorization: Bearer <your-token>`
   - **JANGAN** set Content-Type (auto-detect)
4. Body > form-data:
   - Add all fields sebagai Text
   - Add `fotoKegiatan` sebagai File

---

## 🔍 Troubleshooting

### Masih dapat 404?
✅ **Stop aplikasi** dan restart:
```bash
# Stop aplikasi yang sedang running
# Kemudian jalankan ulang
cd C:\Users\LENOVO\source\repos\SIMTernakAyam\SIMTernakAyam
dotnet run
```

### Dapat 401 Unauthorized?
✅ Pastikan Anda sudah login dan mengirim JWT token di header:
```
Authorization: Bearer <your-jwt-token>
```

### Dapat 415 Unsupported Media Type?
✅ Pastikan `Content-Type` header sesuai:
- JSON endpoint → `Content-Type: application/json`
- Form-data endpoint → Jangan set Content-Type, biarkan auto

### Dapat 400 Bad Request?
✅ Cek validasi field:
- `tanggal`: Required, format datetime
- `judulKegiatan`: Required, max 200 chars
- `deskripsiKegiatan`: Required, max 1000 chars
- `waktuMulai`: Required, format time (HH:mm:ss)
- `waktuSelesai`: Required, format time (HH:mm:ss)
- `kandangId`: Optional (UUID)
- `catatan`: Optional, max 500 chars
- `fotoKegiatan`: Optional (hanya di endpoint with-photo)

---

## 📊 Summary

| Endpoint | Method | Content-Type | Foto | Use Case |
|----------|--------|--------------|------|----------|
| `/api/jurnal-harian` | POST | application/json | ❌ No | Quick entry tanpa foto |
| `/api/jurnal-harian/with-photo` | POST | multipart/form-data | ✅ Yes | Entry dengan dokumentasi foto |
| `/api/jurnal-harian/{id}` | PUT | application/json | ❌ No | Update data tanpa ubah foto |
| `/api/jurnal-harian/{id}/with-photo` | PUT | multipart/form-data | ✅ Yes | Update dengan foto baru |
| `/api/jurnal-harian` | GET | - | - | List jurnal |
| `/api/jurnal-harian/{id}` | GET | - | - | Detail jurnal |
| `/api/jurnal-harian/{id}` | DELETE | - | - | Hapus jurnal |
| `/api/jurnal-harian/laporan` | GET | - | - | Laporan jurnal |

---

**Selamat mencoba! 🚀**

Updated: 22 Oktober 2025

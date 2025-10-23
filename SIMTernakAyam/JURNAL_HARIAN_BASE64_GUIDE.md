# 📸 Jurnal Harian dengan Foto Base64 - Panduan Lengkap

## ✅ Masalah Sebelumnya (SUDAH DIPERBAIKI)

**Error**:
```json
{
  "statusCode": 500,
  "message": "Deserialization of interface types is not supported. Type 'Microsoft.AspNetCore.Http.IFormFile'"
}
```

**Penyebab**: Field `fotoKegiatan` dengan tipe `IFormFile` tidak bisa di-deserialize dari JSON.

**Solusi**: Sekarang menggunakan **Base64 string** untuk foto! ✨

---

## 🎯 Pilih Endpoint Yang Tepat

### 📱 Untuk Frontend Web/Mobile → Gunakan JSON + Base64

**Endpoint**: `POST /api/jurnal-harian` (JSON)

✅ **Keuntungan**:
- Simple untuk frontend (React, Vue, Angular, Flutter, etc)
- Bisa kirim semua data dalam satu request JSON
- Mudah handle di JavaScript/TypeScript
- Support foto dalam bentuk base64

❌ **Kekurangan**:
- File size jadi ~33% lebih besar (karena base64 encoding)

### 📁 Untuk Upload File Besar → Gunakan Form-Data

**Endpoint**: `POST /api/jurnal-harian/with-photo` (Form-Data)

✅ **Keuntungan**:
- File size lebih kecil (original size)
- Lebih efisien untuk foto besar

❌ **Kekurangan**:
- Lebih ribet di frontend
- Harus handle FormData

---

## 🚀 REKOMENDASI: Pakai JSON + Base64

Untuk kebanyakan kasus, **gunakan JSON + Base64** karena lebih mudah di frontend!

---

## 📝 Format Request JSON (Tanpa Foto)

### POST /api/jurnal-harian

```json
{
  "tanggal": "2025-10-22T00:00:00",
  "judulKegiatan": "Pembersihan Kandang A1",
  "deskripsiKegiatan": "Melakukan pembersihan menyeluruh",
  "waktuMulai": "08:00",
  "waktuSelesai": "10:30",
  "kandangId": "your-kandang-uuid",
  "catatan": "Semua lancar"
}
```

---

## 📸 Format Request JSON (Dengan Foto Base64)

### POST /api/jurnal-harian

```json
{
  "tanggal": "2025-10-22T00:00:00",
  "judulKegiatan": "Pembersihan Kandang A1",
  "deskripsiKegiatan": "Melakukan pembersihan menyeluruh",
  "waktuMulai": "08:00",
  "waktuSelesai": "10:30",
  "kandangId": "your-kandang-uuid",
  "catatan": "Semua lancar",
  "fotoKegiatanBase64": "data:image/jpeg;base64,/9j/4AAQSkZJRgABAQAAAQABAAD...",
  "fotoKegiatanFileName": "kandang_a1.jpg"
}
```

### Field Foto:

| Field | Type | Required | Keterangan |
|-------|------|----------|------------|
| `fotoKegiatanBase64` | string | ❌ No | Base64 string dari foto |
| `fotoKegiatanFileName` | string | ❌ No | Nama file (optional, auto-generate jika kosong) |

---

## 💻 Cara Convert Foto ke Base64 di Frontend

### 1️⃣ React / JavaScript

```javascript
// Function untuk convert file ke base64
const convertToBase64 = (file) => {
  return new Promise((resolve, reject) => {
    const reader = new FileReader();
    reader.readAsDataURL(file);
    reader.onload = () => resolve(reader.result);
    reader.onerror = (error) => reject(error);
  });
};

// Contoh usage saat submit form
const handleSubmit = async (e) => {
  e.preventDefault();

  let fotoBase64 = null;
  if (selectedFile) {
    fotoBase64 = await convertToBase64(selectedFile);
  }

  const data = {
    tanggal: "2025-10-22T00:00:00",
    judulKegiatan: "Pembersihan Kandang",
    deskripsiKegiatan: "Deskripsi lengkap",
    waktuMulai: "08:00",
    waktuSelesai: "10:00",
    kandangId: kandangId,
    catatan: catatan,
    fotoKegiatanBase64: fotoBase64,
    fotoKegiatanFileName: selectedFile?.name
  };

  const response = await fetch('https://localhost:7195/api/jurnal-harian', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`
    },
    body: JSON.stringify(data)
  });

  const result = await response.json();
  console.log(result);
};
```

### 2️⃣ Vue.js

```javascript
// Method di component
methods: {
  async convertToBase64(file) {
    return new Promise((resolve, reject) => {
      const reader = new FileReader();
      reader.readAsDataURL(file);
      reader.onload = () => resolve(reader.result);
      reader.onerror = reject;
    });
  },

  async submitJurnal() {
    let fotoBase64 = null;
    if (this.selectedFile) {
      fotoBase64 = await this.convertToBase64(this.selectedFile);
    }

    const payload = {
      tanggal: this.tanggal,
      judulKegiatan: this.judulKegiatan,
      deskripsiKegiatan: this.deskripsiKegiatan,
      waktuMulai: this.waktuMulai,
      waktuSelesai: this.waktuSelesai,
      kandangId: this.kandangId,
      catatan: this.catatan,
      fotoKegiatanBase64: fotoBase64,
      fotoKegiatanFileName: this.selectedFile?.name
    };

    const response = await this.$http.post('/api/jurnal-harian', payload);
    console.log(response.data);
  }
}
```

### 3️⃣ Axios Example

```javascript
import axios from 'axios';

const submitJurnalHarian = async (formData, photoFile) => {
  let fotoBase64 = null;

  if (photoFile) {
    // Convert to base64
    const reader = new FileReader();
    fotoBase64 = await new Promise((resolve) => {
      reader.onload = (e) => resolve(e.target.result);
      reader.readAsDataURL(photoFile);
    });
  }

  const payload = {
    tanggal: formData.tanggal,
    judulKegiatan: formData.judulKegiatan,
    deskripsiKegiatan: formData.deskripsiKegiatan,
    waktuMulai: formData.waktuMulai,
    waktuSelesai: formData.waktuSelesai,
    kandangId: formData.kandangId,
    catatan: formData.catatan,
    fotoKegiatanBase64: fotoBase64,
    fotoKegiatanFileName: photoFile?.name
  };

  try {
    const response = await axios.post(
      'https://localhost:7195/api/jurnal-harian',
      payload,
      {
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${localStorage.getItem('token')}`
        }
      }
    );

    return response.data;
  } catch (error) {
    console.error('Error:', error.response?.data);
    throw error;
  }
};
```

### 4️⃣ Flutter / Dart

```dart
import 'dart:convert';
import 'dart:io';
import 'package:http/http.dart' as http;

Future<void> submitJurnalHarian(File? imageFile) async {
  String? base64Image;

  if (imageFile != null) {
    final bytes = await imageFile.readAsBytes();
    base64Image = 'data:image/jpeg;base64,${base64Encode(bytes)}';
  }

  final payload = {
    'tanggal': '2025-10-22T00:00:00',
    'judulKegiatan': 'Pembersihan Kandang',
    'deskripsiKegiatan': 'Deskripsi lengkap',
    'waktuMulai': '08:00',
    'waktuSelesai': '10:00',
    'kandangId': kandangId,
    'catatan': catatan,
    'fotoKegiatanBase64': base64Image,
    'fotoKegiatanFileName': imageFile?.path.split('/').last,
  };

  final response = await http.post(
    Uri.parse('https://localhost:7195/api/jurnal-harian'),
    headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer $token',
    },
    body: jsonEncode(payload),
  );

  if (response.statusCode == 201) {
    print('Success: ${response.body}');
  } else {
    print('Error: ${response.body}');
  }
}
```

---

## 🎨 HTML Form Example

```html
<!DOCTYPE html>
<html>
<head>
    <title>Jurnal Harian Form</title>
</head>
<body>
    <form id="jurnalForm">
        <input type="date" id="tanggal" required>
        <input type="text" id="judulKegiatan" required>
        <textarea id="deskripsiKegiatan" required></textarea>
        <input type="time" id="waktuMulai" required>
        <input type="time" id="waktuSelesai" required>
        <input type="file" id="fotoKegiatan" accept="image/*">
        <button type="submit">Submit</button>
    </form>

    <script>
        document.getElementById('jurnalForm').addEventListener('submit', async (e) => {
            e.preventDefault();

            const fileInput = document.getElementById('fotoKegiatan');
            let base64Photo = null;
            let fileName = null;

            if (fileInput.files.length > 0) {
                const file = fileInput.files[0];
                fileName = file.name;

                // Convert to base64
                base64Photo = await new Promise((resolve) => {
                    const reader = new FileReader();
                    reader.onloadend = () => resolve(reader.result);
                    reader.readAsDataURL(file);
                });
            }

            const payload = {
                tanggal: document.getElementById('tanggal').value + 'T00:00:00',
                judulKegiatan: document.getElementById('judulKegiatan').value,
                deskripsiKegiatan: document.getElementById('deskripsiKegiatan').value,
                waktuMulai: document.getElementById('waktuMulai').value,
                waktuSelesai: document.getElementById('waktuSelesai').value,
                fotoKegiatanBase64: base64Photo,
                fotoKegiatanFileName: fileName
            };

            const response = await fetch('https://localhost:7195/api/jurnal-harian', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': 'Bearer YOUR_TOKEN_HERE'
                },
                body: JSON.stringify(payload)
            });

            const result = await response.json();
            console.log(result);
            alert(result.message);
        });
    </script>
</body>
</html>
```

---

## 📊 Summary Endpoints

| Method | Endpoint | Content-Type | Foto | Use Case |
|--------|----------|--------------|------|----------|
| **POST** | `/api/jurnal-harian` | `application/json` | ✅ Base64 | **RECOMMENDED** untuk frontend |
| POST | `/api/jurnal-harian/with-photo` | `multipart/form-data` | ✅ File | Untuk upload file besar |
| **PUT** | `/api/jurnal-harian/{id}` | `application/json` | ✅ Base64 | **RECOMMENDED** untuk frontend |
| PUT | `/api/jurnal-harian/{id}/with-photo` | `multipart/form-data` | ✅ File | Untuk upload file besar |
| GET | `/api/jurnal-harian` | - | - | List jurnal |
| GET | `/api/jurnal-harian/{id}` | - | - | Detail jurnal |
| DELETE | `/api/jurnal-harian/{id}` | - | - | Hapus jurnal |

---

## 🎯 REKOMENDASI UNTUK FRONTEND

✅ **Gunakan**: `POST /api/jurnal-harian` (JSON + Base64)

**Alasan**:
1. Lebih mudah di-handle di JavaScript/TypeScript
2. Satu request untuk semua data
3. Tidak perlu FormData
4. Support CORS lebih baik
5. Easier debugging

---

## 🔍 Testing dengan Postman

### Request:
**Method**: POST
**URL**: `https://localhost:7195/api/jurnal-harian`
**Headers**:
```
Content-Type: application/json
Authorization: Bearer <your-token>
```

**Body (raw JSON)**:
```json
{
  "tanggal": "2025-10-22T00:00:00",
  "judulKegiatan": "Test Jurnal",
  "deskripsiKegiatan": "Test deskripsi",
  "waktuMulai": "08:00",
  "waktuSelesai": "10:00",
  "fotoKegiatanBase64": "data:image/jpeg;base64,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",
  "fotoKegiatanFileName": "kandang_clean.jpg"
}
```

---

## ✅ Response Sukses

```json
{
  "success": true,
  "message": "Jurnal harian berhasil dibuat.",
  "data": {
    "id": "uuid-jurnal",
    "tanggal": "2025-10-22T00:00:00",
    "judulKegiatan": "Pembersihan Kandang A1",
    "deskripsiKegiatan": "Melakukan pembersihan menyeluruh",
    "waktuMulai": "08:00:00",
    "waktuSelesai": "10:30:00",
    "petugasId": "uuid-petugas",
    "petugasName": "Budi Santoso",
    "kandangId": "uuid-kandang",
    "kandangName": "Kandang A1",
    "catatan": "Semua lancar",
    "fotoKegiatan": "/uploads/jurnal_abc123.jpg",
    "createdAt": "2025-10-22T08:30:00"
  },
  "statusCode": 201
}
```

---

## 🔄 Update dengan Foto Base64

### PUT /api/jurnal-harian/{id}

```json
{
  "tanggal": "2025-10-22T00:00:00",
  "judulKegiatan": "Pembersihan Kandang A1 (Updated)",
  "deskripsiKegiatan": "Update deskripsi",
  "waktuMulai": "08:00",
  "waktuSelesai": "11:00",
  "fotoKegiatanBase64": "data:image/png;base64,iVBORw0KGg...",
  "fotoKegiatanFileName": "new_photo.png"
}
```

---

## ⚠️ RESTART APLIKASI!

Setelah perubahan ini, **WAJIB restart**:
```bash
cd C:\Users\LENOVO\source\repos\SIMTernakAyam\SIMTernakAyam
dotnet run
```

---

**Problem Solved! 🎉**

Updated: 22 Oktober 2025

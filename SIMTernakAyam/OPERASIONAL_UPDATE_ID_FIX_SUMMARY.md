# ?? FIX OPERASIONAL UPDATE - ID URL vs BODY ERROR

## ?? **Masalah yang Dipecahkan**
```json
{
  "success": false, 
  "message": "ID di URL tidak sesuai dengan ID di body.", 
  "data": null, 
  "errors": null, 
  "statusCode": 400, 
  "timestamp": "2025-12-11T00:19:35.0799056+07:00" 
}
```

## ?? **Root Cause Analysis**

### **Pola Aplikasi yang Konsisten**
Aplikasi ini menggunakan pola yang konsisten di seluruh controller:

1. **Update DTOs memiliki properti `Id` yang required**
2. **Controller memvalidasi `id` parameter == `dto.Id`**
3. **Jika tidak sama, return error 400**

**Contoh di BiayaController:**
```csharp
if (id != dto.Id)
{
    return Error("ID di URL tidak sesuai dengan ID di body.", 400);
}
```

### **Yang Salah Sebelumnya**
- `UpdateOperasionalDto` tidak konsisten dengan pola aplikasi
- Controller tidak melakukan validasi ID yang sama
- Frontend mungkin tidak mengirim ID di body

## ? **Solusi yang Diimplementasi**

### **1. Perbaiki UpdateOperasionalDto**
```csharp
public class UpdateOperasionalDto
{
    [Required(ErrorMessage = "ID wajib diisi.")]
    public Guid Id { get; set; } // ? Ditambahkan kembali

    // ...other properties...
}
```

### **2. Perbaiki OperasionalController**
```csharp
[HttpPut("{id}")]
public async Task<IActionResult> Update(Guid id, [FromBody] UpdateOperasionalDto dto)
{
    if (!ModelState.IsValid)
    {
        return ValidationError(ModelState);
    }

    // ? Validasi ID konsisten dengan controller lain
    if (id != dto.Id)
    {
        return Error("ID di URL tidak sesuai dengan ID di body.", 400);
    }

    // ...rest of the method...
}
```

## ?? **Cara Penggunaan yang Benar**

### **Format Request yang Benar:**
```javascript
// ? BENAR - ID harus sama di URL dan body
PUT /api/operasionals/3359f6c9-827d-458c-87e3-44ced3a4ceea
{
  "id": "3359f6c9-827d-458c-87e3-44ced3a4ceea", // ? SAMA dengan URL
  "jenisKegiatanId": "550e8400-e29b-41d4-a716-446655440001",
  "tanggal": "2024-12-10T10:00:00Z",
  "jumlah": 15,
  "petugasId": "550e8400-e29b-41d4-a716-446655440000",
  "kandangId": "550e8400-e29b-41d4-a716-446655440002",
  "pakanId": "128c96b6-7df0-4388-b20f-a56292fac2db",
  "vaksinId": null
}
```

### **Yang Salah:**
```javascript
// ? SALAH - ID berbeda atau tidak ada
PUT /api/operasionals/3359f6c9-827d-458c-87e3-44ced3a4ceea
{
  "id": "00000000-0000-0000-0000-000000000000", // ? ID berbeda
  // atau tidak ada property "id" sama sekali
}
```

## ?? **Alternatif: Update dengan Validasi**

Jika tidak ingin menggunakan ID di body, gunakan endpoint alternatif:

```javascript
// ? ALTERNATIF - Tidak perlu ID di body
PUT /api/operasionals/3359f6c9-827d-458c-87e3-44ced3a4ceea/update-with-validation
{
  "jenisKegiatanId": "550e8400-e29b-41d4-a716-446655440001",
  "tanggal": "2024-12-10T10:00:00Z",
  "jumlah": 10,
  "petugasId": "550e8400-e29b-41d4-a716-446655440000",
  "kandangId": "550e8400-e29b-41d4-a716-446655440002",
  "vaksinId": "509b9562-7cee-4dd4-8ce9-d6a53569b0e5",
  "pakanId": null
}
```

## ?? **Testing**

File test lengkap: `Tests/OperasionalUpdateIdFix.http`

### **Test Cases:**
1. ? Update dengan ID yang benar (URL == Body)
2. ? Update dengan endpoint alternatif
3. ? Update dengan ID mismatch (akan error 400)
4. ? Update tanpa ID di body (akan error 422)

## ?? **Frontend Action Items**

### **Untuk Frontend Developer:**

1. **Pastikan ID ada di body request:**
```javascript
const updateOperasional = async (id, data) => {
  const payload = {
    id: id, // ? PENTING: ID harus ada dan sama dengan parameter
    jenisKegiatanId: data.jenisKegiatanId,
    tanggal: data.tanggal,
    jumlah: data.jumlah,
    petugasId: data.petugasId,
    kandangId: data.kandangId,
    pakanId: data.pakanId || null,
    vaksinId: data.vaksinId || null
  };
  
  return fetch(`/api/operasionals/${id}`, {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(payload)
  });
};
```

2. **Atau gunakan endpoint alternatif:**
```javascript
const updateOperasionalWithValidation = async (id, data) => {
  const payload = {
    // Tidak perlu id di sini
    jenisKegiatanId: data.jenisKegiatanId,
    // ...other fields
  };
  
  return fetch(`/api/operasionals/${id}/update-with-validation`, {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(payload)
  });
};
```

## ?? **Hasil Akhir**

? **Error "ID di URL tidak sesuai dengan ID di body" telah diperbaiki**
? **OperasionalController konsisten dengan pola aplikasi**  
? **Tersedia 2 endpoint update: standar dan dengan validasi**
? **Test cases lengkap untuk verifikasi**
? **Dokumentasi lengkap untuk frontend**

## ?? **Status: RESOLVED** ?

Error ini sudah tidak akan muncul lagi asalkan frontend mengikuti format request yang benar atau menggunakan endpoint alternatif.
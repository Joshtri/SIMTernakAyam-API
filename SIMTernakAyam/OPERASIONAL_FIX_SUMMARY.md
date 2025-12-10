# ? OPERASIONAL PAKAN & VAKSIN - SOLUSI LENGKAP

## ?? **Masalah yang Dipecahkan**
> **Operasional tidak menyimpan `pakanId` dan `vaksinId` sehingga tracking stok tidak akurat dan `stokTerpakai` selalu 0**

## ?? **Solusi yang Diimplementasi**

### 1. **Enhanced OperasionalController** ?
- ? **Endpoint Baru**: `GET /api/operasionals/form-data` - Data lengkap untuk form
- ? **Endpoint Baru**: `POST /api/operasionals/create-with-validation` - Create dengan validasi built-in  
- ? **Enhanced**: `POST /api/operasionals/validate-stock` - Validasi stok real-time
- ? **Fixed**: Mapping `PakanId` dan `VaksinId` di semua endpoint

### 2. **DTOs Sudah Support Pakan/Vaksin** ?
- ? `CreateOperasionalDto` - Ada `PakanId` & `VaksinId`
- ? `UpdateOperasionalDto` - Ada `PakanId` & `VaksinId`
- ? `OperasionalResponseDto` - Menampilkan nama pakan & vaksin

### 3. **New API Endpoints**

#### **GET /api/operasionals/form-data**
```json
{
  "success": true,
  "data": {
    "jenisKegiatan": [...],
    "vaksins": [
      {
        "id": "509b9562-7cee-4dd4-8ce9-d6a53569b0e5",
        "namaVaksin": "Medivac ND La Sota",
        "stokTersisa": 3,
        "statusStok": "Menipis",
        "info": "Medivac ND La Sota - Sisa: 3 dosis (Menipis)"
      }
    ],
    "pakans": [...]
  }
}
```

#### **POST /api/operasionals/create-with-validation**
```javascript
// Request
{
  "jenisKegiatanId": "guid",
  "tanggal": "2025-12-10T10:00:00Z",
  "jumlah": 2,
  "petugasId": "guid",
  "kandangId": "guid",
  "vaksinId": "509b9562-7cee-4dd4-8ce9-d6a53569b0e5", // ? PENTING!
  "pakanId": null
}

// Response
{
  "success": true,
  "data": {
    "operasional": {
      "id": "new-guid",
      "vaksinId": "509b9562-7cee-4dd4-8ce9-d6a53569b0e5",
      "vaksinNama": "Medivac ND La Sota", // ? Sekarang ada!
      "pakanId": null,
      "pakanNama": null
    },
    "stockValidation": [...]
  }
}
```

## ?? **Frontend Integration Guide**

### **1. Load Form Data**
```javascript
const loadFormData = async () => {
  const response = await fetch('/api/operasionals/form-data');
  const data = await response.json();
  
  if (data.success) {
    // Populate dropdown vaksin
    populateSelect('vaksinSelect', data.data.vaksins, 'id', 'info');
    
    // Populate dropdown pakan  
    populateSelect('pakanSelect', data.data.pakans, 'id', 'info');
  }
};
```

### **2. Create Operasional dengan Pakan/Vaksin**
```javascript
const createOperasional = async (formData) => {
  const payload = {
    jenisKegiatanId: formData.jenisKegiatanId,
    tanggal: formData.tanggal,
    jumlah: parseInt(formData.jumlah),
    petugasId: formData.petugasId,
    kandangId: formData.kandangId,
    vaksinId: formData.vaksinId || null,  // ? Kirim ID vaksin
    pakanId: formData.pakanId || null     // ? Kirim ID pakan
  };
  
  const response = await fetch('/api/operasionals/create-with-validation', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(payload)
  });
  
  const result = await response.json();
  
  if (result.success) {
    console.log('? Operasional created:', result.data.operasional);
    console.log('?? Stock validation:', result.data.stockValidation);
  }
};
```

### **3. Form HTML Template**
```html
<form id="operasionalForm">
  <!-- Jenis Kegiatan -->
  <select name="jenisKegiatanId" required>
    <option value="">-- Pilih Jenis Kegiatan --</option>
  </select>
  
  <!-- Vaksin (Conditional) -->
  <select name="vaksinId" id="vaksinSelect">
    <option value="">-- Pilih Vaksin (Opsional) --</option>
  </select>
  
  <!-- Pakan (Conditional) -->
  <select name="pakanId" id="pakanSelect">
    <option value="">-- Pilih Pakan (Opsional) --</option>
  </select>
  
  <!-- Jumlah -->
  <input type="number" name="jumlah" min="1" required>
  
  <!-- Other fields... -->
  
  <button type="submit">Simpan</button>
</form>
```

## ?? **Testing yang Tersedia**

File test: `Tests/OperasionalPakanVaksinTests.http`

### **Key Test Cases**:
1. ? `GET /api/operasionals/form-data` - Load form data
2. ? `POST /api/operasionals/create-with-validation` - Create dengan vaksin
3. ? `POST /api/operasionals/create-with-validation` - Create dengan pakan  
4. ? `PUT /api/operasionals/{id}` - Update operasional existing
5. ? `POST /api/operasionals/validate-stock` - Validasi stok
6. ? Error handling untuk stok tidak cukup

## ?? **Expected Results**

### **Before (Masalah)**:
```json
{
  "pakanId": null,
  "pakanNama": null,
  "vaksinId": null,  
  "vaksinNama": null
}
```
> ? Tidak ada tracking pakan/vaksin yang digunakan

### **After (Solusi)**:
```json
{
  "pakanId": "128c96b6-7df0-4388-b20f-a56292fac2db",
  "pakanNama": "Pakan Finisher BR-2",
  "vaksinId": "509b9562-7cee-4dd4-8ce9-d6a53569b0e5",
  "vaksinNama": "Medivac ND La Sota"
}
```
> ? Ada tracking lengkap pakan/vaksin yang digunakan

### **Stok Tracking Fix**:
```json
// GET /api/vaksins/with-usage-detail
{
  "namaVaksin": "Medivac ND La Sota",
  "stok": 3,
  "stokAwal": 10,
  "stokTerpakai": 7,     // ? Sekarang terhitung benar
  "stokTersisa": 3,      // ? Sisa yang akurat
  "statusStok": "Menipis"
}
```

## ?? **Action Items untuk Frontend Developer**

### **IMMEDIATE (Harus dilakukan):**
1. ? **Update form operasional** - Tambahkan dropdown vaksin & pakan
2. ? **Gunakan endpoint baru** - `/api/operasionals/create-with-validation`
3. ? **Load form data** - Dari `/api/operasionals/form-data`
4. ? **Send vaksinId/pakanId** - Dalam payload create/update

### **RECOMMENDED:**
1. ?? **Real-time validation** - Panggil validate-stock saat input berubah
2. ?? **Stock warnings** - Tampilkan alert jika stok menipis
3. ?? **UI indicators** - Color coding berdasarkan statusStok
4. ?? **Mobile responsive** - Pastikan dropdown bekerja di mobile

## ?? **Endpoint Summary**

| Method | Endpoint | Purpose |
|--------|----------|---------|
| `GET` | `/api/operasionals/form-data` | ? Data untuk form |
| `POST` | `/api/operasionals/create-with-validation` | ? Create dengan validasi |
| `POST` | `/api/operasionals/validate-stock` | Validasi stok |
| `POST` | `/api/operasionals` | Create biasa |
| `PUT` | `/api/operasionals/{id}` | Update operasional |
| `GET` | `/api/operasionals` | List semua |

## ? **Hasil Akhir**

Setelah frontend diupdate dengan endpoint baru:

1. ? **Operasional akan menyimpan `vaksinId` dan `pakanId`**
2. ? **Response akan menampilkan nama vaksin/pakan**  
3. ? **`stokTerpakai` akan terhitung dengan benar**
4. ? **Tracking stok akan akurat per periode**
5. ? **Validasi stok akan mencegah overuse**
6. ? **Dashboard akan menampilkan data usage yang benar**

---

## ?? **CRITICAL - Frontend Must Do**

**Frontend HARUS mengirim `vaksinId` atau `pakanId` dalam payload jika operasional menggunakan vaksin/pakan:**

```javascript
// ? SALAH - Tidak mengirim vaksinId/pakanId
{
  "jenisKegiatanId": "guid",
  "jumlah": 2,
  "tanggal": "...",
  // missing vaksinId/pakanId
}

// ? BENAR - Mengirim vaksinId jika menggunakan vaksin
{
  "jenisKegiatanId": "guid",
  "jumlah": 2,  
  "tanggal": "...",
  "vaksinId": "509b9562-7cee-4dd4-8ce9-d6a53569b0e5" // ? HARUS ada!
}
```

**Status**: ? **READY FOR FRONTEND INTEGRATION**
# 🔧 Operasional API - Panduan Pakan & Vaksin

## 🎯 Masalah yang Dipecahkan
> **Operasional tidak menyimpan `pakanId` dan `vaksinId`** sehingga tracking stok tidak akurat.

## 🚀 Solusi

### 1. **Endpoint Baru untuk Form Data**
```http
GET /api/operasionals/form-data
```

**Response**:
```json
{
  "success": true,
  "data": {
    "jenisKegiatan": [
      {
        "id": "guid",
        "nama": "Pemberian Pakan",
        "deskripsi": "Pemberian pakan harian",
        "biayaDefault": 5000,
        "membutuhkanVaksin": false,
        "membutuhkanPakan": true
      }
    ],
    "vaksins": [
      {
        "id": "509b9562-7cee-4dd4-8ce9-d6a53569b0e5",
        "namaVaksin": "Medivac ND La Sota",
        "stokTersisa": 3,
        "statusStok": "Menipis",
        "isStokCukup": true,
        "info": "Medivac ND La Sota - Sisa: 3 dosis (Menipis)"
      }
    ],
    "pakans": [
      {
        "id": "128c96b6-7df0-4388-b20f-a56292fac2db",
        "namaPakan": "Pakan Finisher BR-2",
        "stokTersisaKg": 5000.0,
        "statusStok": "Aman",
        "isStokCukup": true,
        "info": "Pakan Finisher BR-2 - Sisa: 5000.0 kg (Aman)"
      }
    ]
  }
}
```

### 2. **Create Operasional dengan Validasi Stok**
```http
POST /api/operasionals/create-with-validation
Content-Type: application/json

{
  "jenisKegiatanId": "4dd041a4-90c3-493e-913b-531ece5274f1",
  "tanggal": "2025-12-10T10:00:00Z",
  "jumlah": 2,
  "petugasId": "15deaabd-dc4c-413f-97ef-2bb341f3fea4",
  "kandangId": "fd082b84-41c8-45d4-8480-ddb918f74cca",
  "vaksinId": "509b9562-7cee-4dd4-8ce9-d6a53569b0e5",
  "pakanId": null
}
```

**Response Success**:
```json
{
  "success": true,
  "message": "Operasional berhasil dibuat dengan validasi stok.",
  "data": {
    "operasional": {
      "id": "new-guid",
      "jenisKegiatanId": "4dd041a4-90c3-493e-913b-531ece5274f1",
      "jenisKegiatanNama": "Vaksinasi",
      "tanggal": "2025-12-10T10:00:00Z",
      "jumlah": 2,
      "petugasId": "15deaabd-dc4c-413f-97ef-2bb341f3fea4",
      "petugasNama": "Inock Makur",
      "kandangId": "fd082b84-41c8-45d4-8480-ddb918f74cca",
      "kandangNama": "Kandang 2",
      "vaksinId": "509b9562-7cee-4dd4-8ce9-d6a53569b0e5",
      "vaksinNama": "Medivac ND La Sota",
      "pakanId": null,
      "pakanNama": null,
      "totalBiaya": 10000
    },
    "stockValidation": [
      {
        "type": "Vaksin",
        "data": {
          "vaksinId": "509b9562-7cee-4dd4-8ce9-d6a53569b0e5",
          "namaVaksin": "Medivac ND La Sota",
          "stokTersedia": 3,
          "jumlahDibutuhkan": 2,
          "isAvailable": true,
          "stokKurang": 0,
          "statusStok": "Menipis",
          "rekomendasi": "Stok akan menipis setelah penggunaan."
        }
      }
    ]
  }
}
```

### 3. **Update Operasional Existing dengan Pakan/Vaksin**
```http
PUT /api/operasionals/{id}
Content-Type: application/json

{
  "id": "3359f6c9-827d-458c-87e3-44ced3a4ceea",
  "jenisKegiatanId": "4dd041a4-90c3-493e-913b-531ece5274f1",
  "tanggal": "2025-10-11T00:00:00",
  "jumlah": 5,
  "petugasId": "15deaabd-dc4c-413f-97ef-2bb341f3fea4",
  "kandangId": "fd082b84-41c8-45d4-8480-ddb918f74cca",
  "vaksinId": "509b9562-7cee-4dd4-8ce9-d6a53569b0e5",
  "pakanId": null
}
```

## 💻 Frontend Implementation Guide

### 1. **Load Form Data**
```javascript
// Load data untuk dropdown
const loadFormData = async () => {
  try {
    const response = await fetch('/api/operasionals/form-data');
    const result = await response.json();
    
    if (result.success) {
      // Populate dropdowns
      populateJenisKegiatanDropdown(result.data.jenisKegiatan);
      populateVaksinDropdown(result.data.vaksins);
      populatePakanDropdown(result.data.pakans);
    }
  } catch (error) {
    console.error('Error loading form data:', error);
  }
};

const populateVaksinDropdown = (vaksins) => {
  const select = document.getElementById('vaksinSelect');
  select.innerHTML = '<option value="">-- Pilih Vaksin (Opsional) --</option>';
  
  vaksins.forEach(vaksin => {
    const option = document.createElement('option');
    option.value = vaksin.id;
    option.textContent = vaksin.info;
    
    // Add class berdasarkan status stok
    if (vaksin.statusStok === 'Kritis') {
      option.className = 'text-red-600';
    } else if (vaksin.statusStok === 'Menipis') {
      option.className = 'text-yellow-600';
    }
    
    select.appendChild(option);
  });
};
```

### 2. **Validasi Stok Real-time**
```javascript
const validateStockRealTime = async (vaksinId, pakanId, jumlah) => {
  try {
    const response = await fetch('/api/operasionals/validate-stock', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({
        vaksinId: vaksinId,
        pakanId: pakanId,
        jumlah: parseInt(jumlah)
      })
    });
    
    const result = await response.json();
    
    if (!result.success) {
      // Tampilkan error
      showStockError(result.message);
      return false;
    }
    
    // Tampilkan warning jika ada
    if (result.data.warnings && result.data.warnings.length > 0) {
      showStockWarnings(result.data.warnings);
    }
    
    return true;
  } catch (error) {
    console.error('Error validating stock:', error);
    return false;
  }
};

// Event listener untuk real-time validation
document.getElementById('jumlahInput').addEventListener('input', debounce(async (e) => {
  const vaksinId = document.getElementById('vaksinSelect').value;
  const pakanId = document.getElementById('pakanSelect').value;
  const jumlah = e.target.value;
  
  if (jumlah && (vaksinId || pakanId)) {
    await validateStockRealTime(vaksinId, pakanId, jumlah);
  }
}, 500));
```

### 3. **Submit Form dengan Validasi**
```javascript
const submitOperasionalForm = async (formData) => {
  try {
    // Step 1: Validasi stok terlebih dahulu
    const isStockValid = await validateStockRealTime(
      formData.vaksinId, 
      formData.pakanId, 
      formData.jumlah
    );
    
    if (!isStockValid) {
      return;
    }
    
    // Step 2: Submit dengan endpoint yang sudah ada validasi built-in
    const response = await fetch('/api/operasionals/create-with-validation', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({
        jenisKegiatanId: formData.jenisKegiatanId,
        tanggal: formData.tanggal,
        jumlah: parseInt(formData.jumlah),
        petugasId: formData.petugasId,
        kandangId: formData.kandangId,
        vaksinId: formData.vaksinId || null,
        pakanId: formData.pakanId || null
      })
    });
    
    const result = await response.json();
    
    if (result.success) {
      showSuccessMessage('Operasional berhasil dibuat!');
      
      // Tampilkan info stok setelah operasional
      if (result.data.stockValidation) {
        showStockUpdateInfo(result.data.stockValidation);
      }
      
      // Redirect atau refresh data
      window.location.href = '/operasional';
    } else {
      showErrorMessage(result.message);
    }
    
  } catch (error) {
    console.error('Error submitting operasional:', error);
    showErrorMessage('Terjadi kesalahan saat menyimpan data.');
  }
};
```

### 4. **Form HTML Template**
```html
<form id="operasionalForm">
  <!-- Jenis Kegiatan -->
  <div class="form-group">
    <label for="jenisKegiatanSelect">Jenis Kegiatan *</label>
    <select id="jenisKegiatanSelect" name="jenisKegiatanId" required>
      <option value="">-- Pilih Jenis Kegiatan --</option>
    </select>
  </div>
  
  <!-- Vaksin (conditional) -->
  <div class="form-group" id="vaksinGroup" style="display: none;">
    <label for="vaksinSelect">Vaksin</label>
    <select id="vaksinSelect" name="vaksinId">
      <option value="">-- Pilih Vaksin --</option>
    </select>
    <small class="text-info">*Opsional, hanya jika operasional menggunakan vaksin</small>
  </div>
  
  <!-- Pakan (conditional) -->
  <div class="form-group" id="pakanGroup" style="display: none;">
    <label for="pakanSelect">Pakan</label>
    <select id="pakanSelect" name="pakanId">
      <option value="">-- Pilih Pakan --</option>
    </select>
    <small class="text-info">*Opsional, hanya jika operasional menggunakan pakan</small>
  </div>
  
  <!-- Jumlah -->
  <div class="form-group">
    <label for="jumlahInput">Jumlah *</label>
    <input type="number" id="jumlahInput" name="jumlah" min="1" required>
    <div id="stockValidationInfo" class="mt-2"></div>
  </div>
  
  <!-- Other fields... -->
  
  <button type="submit">Simpan Operasional</button>
</form>
```

## 🔧 Testing

### Test Create Operasional dengan Vaksin
```http
POST /api/operasionals/create-with-validation
Content-Type: application/json

{
  "jenisKegiatanId": "4dd041a4-90c3-493e-913b-531ece5274f1",
  "tanggal": "2025-12-10T10:00:00Z",
  "jumlah": 2,
  "petugasId": "15deaabd-dc4c-413f-97ef-2bb341f3fea4",
  "kandangId": "fd082b84-41c8-45d4-8480-ddb918f74cca",
  "vaksinId": "509b9562-7cee-4dd4-8ce9-d6a53569b0e5"
}
```

### Test Update Operasional yang sudah ada
```http
PUT /api/operasionals/3359f6c9-827d-458c-87e3-44ced3a4ceea
Content-Type: application/json

{
  "id": "3359f6c9-827d-458c-87e3-44ced3a4ceea",
  "jenisKegiatanId": "4dd041a4-90c3-493e-913b-531ece5274f1",
  "tanggal": "2025-10-11T00:00:00",
  "jumlah": 2,
  "petugasId": "15deaabd-dc4c-413f-97ef-2bb341f3fea4",
  "kandangId": "fd082b84-41c8-45d4-8480-ddb918f74cca",
  "vaksinId": "509b9562-7cee-4dd4-8ce9-d6a53569b0e5",
  "pakanId": null
}
```

## 🎯 Key Points
    
1. **Selalu kirim `vaksinId` atau `pakanId`** dalam payload jika operasional menggunakan vaksin/pakan
2. **Gunakan endpoint `create-with-validation`** untuk create baru dengan validasi built-in
3. **Validasi stok real-time** sebelum submit form
4. **Load form data** dari endpoint `/form-data` untuk mendapatkan info stok terkini
5. **Handle response** dengan informasi stok validation

## ✅ Hasil Expected

Setelah implementasi ini:
- ✅ `vaksinId` dan `pakanId` akan tersimpan di database
- ✅ `stokTerpakai` akan terhitung dengan benar
- ✅ Response akan menampilkan nama vaksin/pakan
- ✅ Validasi stok akan berfungsi sempurna
- ✅ Frontend mendapat data lengkap untuk dropdown

---

**Status**: Ready for Frontend Integration  
**Next**: Implement frontend form dengan endpoint baru ini
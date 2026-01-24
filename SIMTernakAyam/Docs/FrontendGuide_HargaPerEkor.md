# ?? Frontend Guide: Perubahan Harga Per Ekor

> **?? BREAKING CHANGE**: Sistem harga pasar berubah dari **per kilogram** menjadi **per ekor**

## ?? Table of Contents
- [Overview](#overview)
- [API Changes](#api-changes)
- [Response Structure Changes](#response-structure-changes)
- [UI/UX Changes Required](#uiux-changes-required)
- [Migration Checklist](#migration-checklist)
- [Testing Guide](#testing-guide)

---

## ?? Overview

### Perubahan Konsep Bisnis
| **Sebelum** | **Sesudah** |
|-------------|-------------|
| Harga ayam dihitung berdasarkan **berat (kg)** | Harga ayam dihitung berdasarkan **jumlah ekor** |
| Formula: `Total = Berat × Harga/kg` | Formula: `Total = Jumlah Ekor × Harga/ekor` |
| Butuh input **berat rata-rata** untuk estimasi | **Tidak butuh** input berat rata-rata |
| Field `BeratRataRata` penting | Field `BeratRataRata` **deprecated** (hanya untuk tracking internal) |

### Alasan Perubahan
? Lebih sederhana untuk user (tidak perlu timbang ayam dulu)  
? Sesuai praktik bisnis di lapangan (penjualan per ekor)  
? Mengurangi kompleksitas input data  
? Perhitungan keuntungan lebih akurat per ekor  

---

## ?? API Changes

### 1?? **Harga Pasar API**

#### **GET `/api/harga-pasar`** - List Harga Pasar
**Response Structure Changes:**

```json
// ? SEBELUM (OLD)
{
  "data": [
    {
      "id": "guid",
      "harga_per_kg": 35000,  // ? DEPRECATED
      "tanggal_mulai": "2025-01-01",
      "is_aktif": true
    }
  ]
}

// ? SESUDAH (NEW)
{
  "data": [
    {
      "id": "guid",
      "harga_per_ekor": 42000,  // ? NEW FIELD
      "tanggal_mulai": "2025-01-01",
      "is_aktif": true
    }
  ]
}
```

**Frontend Action Required:**
- ?? Ubah field `harga_per_kg` ? `harga_per_ekor`
- ?? Update label UI: "Harga Per Kg" ? "Harga Per Ekor"
- ?? Update format currency: Tetap `Rp 42.000`

---

#### **POST `/api/harga-pasar`** - Create Harga Pasar
**Request Body Changes:**

```json
// ? SEBELUM (OLD)
{
  "harga_per_kg": 35000,  // ? DEPRECATED
  "tanggal_mulai": "2025-01-01",
  "wilayah": "Kupang"
}

// ? SESUDAH (NEW)
{
  "harga_per_ekor": 42000,  // ? NEW FIELD
  "tanggal_mulai": "2025-01-01",
  "wilayah": "Kupang"
}
```

**Validation Changes:**
```javascript
// ? OLD
harga_per_kg: {
  required: true,
  min: 1,
  max: 1000000  // Rp 1 juta per kg
}

// ? NEW
harga_per_ekor: {
  required: true,
  min: 10000,     // Minimal Rp 10.000 per ekor
  max: 100000     // Maksimal Rp 100.000 per ekor
}
```

**Frontend Action Required:**
- ?? Update form field name
- ?? Update validation rules
- ?? Update placeholder: "Masukkan harga per ekor (contoh: 42000)"
- ?? Update helper text: "Harga dalam rupiah per ekor ayam"

---

#### **GET `/api/harga-pasar/estimasi-keuntungan`** - Estimasi Keuntungan
**Query Parameters Changes:**

```http
### ? SEBELUM (OLD)
GET /api/harga-pasar/estimasi-keuntungan?total_ayam=100&berat_rata_rata=2.5&tanggal_referensi=2025-01-15

### ? SESUDAH (NEW)
GET /api/harga-pasar/estimasi-keuntungan?total_ayam=100&tanggal_referensi=2025-01-15
# ?? Parameter 'berat_rata_rata' DIHAPUS
```

**Response Structure Changes:**

```json
// ? SEBELUM (OLD)
{
  "success": true,
  "data": {
    "total_ayam": 100,
    "berat_rata_rata": 2.5,        // ? DEPRECATED
    "total_berat": 250,             // ? DEPRECATED
    "harga_per_kg": 35000,          // ? DEPRECATED
    "total_pendapatan": 8750000     // 250 kg × Rp 35.000
  }
}

// ? SESUDAH (NEW)
{
  "success": true,
  "data": {
    "total_ayam": 100,
    "harga_per_ekor": 42000,        // ? NEW
    "total_pendapatan": 4200000     // 100 ekor × Rp 42.000
  }
}
```

**Frontend Action Required:**
- ?? **Hapus** form input "Berat Rata-Rata"
- ?? **Hapus** tampilan "Total Berat (kg)"
- ?? Update query params (remove `berat_rata_rata`)
- ?? Update response mapping
- ?? Simplify UI form (hanya input jumlah ayam + tanggal)

---

### 2?? **Panen API**

#### **GET `/api/panen/{id}/analisis-keuntungan`** - Analisis Keuntungan Panen
**Response Structure Changes:**

```json
// ? SEBELUM (OLD)
{
  "success": true,
  "data": {
    "panen_id": "guid",
    "tanggal_panen": "2025-01-15",
    "jumlah_ayam": 50,
    "berat_rata_rata": 2.3,             // ? DEPRECATED
    "total_berat_kg": 115,              // ? DEPRECATED
    "harga_pasar_per_kg": 35000,        // ? DEPRECATED
    "pendapatan_kotor": 4025000,        // 115 kg × Rp 35.000
    "total_biaya_operasional": 2500000,
    "keuntungan_bersih": 1525000,
    "margin_keuntungan": 37.89,
    "roi": 61.0,
    "harga_pokok_produksi": 21739,      // per kg
    "status_keuntungan": "Untung"
  }
}

// ? SESUDAH (NEW)
{
  "success": true,
  "data": {
    "panen_id": "guid",
    "tanggal_panen": "2025-01-15",
    "jumlah_ayam": 50,
    "harga_pasar_per_ekor": 42000,      // ? NEW
    "pendapatan_kotor": 2100000,        // 50 ekor × Rp 42.000
    "total_biaya_operasional": 1500000,
    "keuntungan_bersih": 600000,
    "margin_keuntungan": 28.57,
    "roi": 40.0,
    "harga_pokok_produksi": 30000,      // ? per ekor (bukan per kg)
    "status_keuntungan": "Untung"
  }
}
```

**Frontend Action Required:**
- ?? **Hapus** tampilan "Berat Rata-Rata" dan "Total Berat"
- ?? Update label: "Harga Pasar Per Kg" ? "Harga Pasar Per Ekor"
- ?? Update label: "HPP Per Kg" ? "HPP Per Ekor"
- ?? Update card/detail view untuk hide field yang deprecated

---

#### **GET `/api/panen/analisis-keuntungan/periode`** - Analisis per Periode
**Response Structure Changes:**

```json
// ? SEBELUM (OLD)
{
  "success": true,
  "data": [
    {
      "panen_id": "guid",
      "jumlah_ayam": 50,
      "berat_rata_rata": 2.3,           // ? DEPRECATED
      "total_berat_kg": 115,            // ? DEPRECATED
      "harga_pasar_per_kg": 35000,      // ? DEPRECATED
      "pendapatan_kotor": 4025000
    }
  ]
}

// ? SESUDAH (NEW)
{
  "success": true,
  "data": [
    {
      "panen_id": "guid",
      "jumlah_ayam": 50,
      "harga_pasar_per_ekor": 42000,    // ? NEW
      "pendapatan_kotor": 2100000
    }
  ]
}
```

**Frontend Action Required:**
- ?? Update table columns (remove berat columns)
- ?? Update chart labels
- ?? Update export Excel/PDF templates

---

### 3?? **Dashboard API**

#### **GET `/api/dashboard/ringkasan-keuntungan`** - Ringkasan Keuntungan
**Response tetap sama, tapi nilai berubah:**

```json
{
  "periode": "01/01/2025 - 31/01/2025",
  "total_panen": 10,
  "total_pendapatan": 4200000,        // ?? Nilai lebih kecil dari sebelumnya
  "total_biaya": 3000000,
  "total_keuntungan": 1200000,
  "rata_rata_margin": 28.57,
  "rata_rata_roi": 40.0
}
```

**Frontend Action Required:**
- ?? **WARNING**: Nilai pendapatan akan tampak **turun drastis** setelah migration
- ?? Tambahkan **info banner** di dashboard:
  ```
  ?? Sistem perhitungan berubah dari per-kg ke per-ekor sejak [tanggal].
     Data sebelum tanggal tersebut menggunakan perhitungan lama.
  ```
- ?? Jika ada chart tren, tambahkan **vertical line marker** untuk tanggal perubahan

---

## ?? UI/UX Changes Required

### **1. Form Input Harga Pasar**

**SEBELUM:**
```html
<form>
  <label>Harga Per Kilogram (Rp/kg)</label>
  <input 
    type="number" 
    name="harga_per_kg"
    placeholder="Contoh: 35000"
    min="1"
    max="1000000"
  />
  <small>Harga dalam rupiah per kilogram ayam</small>
</form>
```

**SESUDAH:**
```html
<form>
  <label>Harga Per Ekor (Rp/ekor)</label>
  <input 
    type="number" 
    name="harga_per_ekor"
    placeholder="Contoh: 42000"
    min="10000"
    max="100000"
  />
  <small>Harga dalam rupiah per ekor ayam</small>
</form>
```

---

### **2. Estimasi Keuntungan Form**

**SEBELUM:**
```html
<form>
  <label>Jumlah Ayam (ekor)</label>
  <input type="number" name="total_ayam" />
  
  <label>Berat Rata-Rata (kg)</label> ? HAPUS
  <input type="number" name="berat_rata_rata" step="0.01" /> ? HAPUS
  
  <label>Tanggal Referensi</label>
  <input type="date" name="tanggal_referensi" />
</form>
```

**SESUDAH:**
```html
<form>
  <label>Jumlah Ayam (ekor)</label>
  <input type="number" name="total_ayam" />
  
  <label>Tanggal Referensi</label>
  <input type="date" name="tanggal_referensi" />
</form>
```

---

### **3. Detail Panen Card/Modal**

**SEBELUM:**
```html
<div class="panen-detail">
  <p>Jumlah Ayam: 50 ekor</p>
  <p>Berat Rata-Rata: 2.3 kg</p> ? HAPUS
  <p>Total Berat: 115 kg</p> ? HAPUS
  <p>Harga Pasar: Rp 35.000/kg</p> ? UBAH
  <p>Pendapatan: Rp 4.025.000</p>
  <p>HPP: Rp 21.739/kg</p> ? UBAH
</div>
```

**SESUDAH:**
```html
<div class="panen-detail">
  <p>Jumlah Ayam: 50 ekor</p>
  <p>Harga Pasar: Rp 42.000/ekor</p> ? UBAH
  <p>Pendapatan: Rp 2.100.000</p>
  <p>HPP: Rp 30.000/ekor</p> ? UBAH
</div>
```

---

### **4. Table Columns**

**SEBELUM:**
| Tanggal | Jumlah | Berat Rata | Total Berat | Harga/kg | Pendapatan |
|---------|--------|------------|-------------|----------|------------|
| 15/01   | 50     | 2.3 kg     | 115 kg      | 35.000   | 4.025.000  |

**SESUDAH:**
| Tanggal | Jumlah | Harga/Ekor | Pendapatan |
|---------|--------|------------|------------|
| 15/01   | 50     | 42.000     | 2.100.000  |

**Column Mapping:**
- ? Remove: `berat_rata_rata`
- ? Remove: `total_berat_kg`
- ? Update: `harga_pasar_per_kg` ? `harga_pasar_per_ekor`
- ? Keep: `jumlah_ayam`, `pendapatan_kotor`, `keuntungan_bersih`

---

## ? Migration Checklist

### **Phase 1: Backend Update (BE Team)**
- [ ] Update Model `HargaPasar` (HargaPerKg ? HargaPerEkor)
- [ ] Update DTOs (AnalisisKeuntunganDto, EstimasiKeuntunganDto)
- [ ] Update Service Logic (PanenService, HargaPasarService)
- [ ] Create Database Migration
- [ ] Run Migration: `dotnet ef database update`
- [ ] Update API Documentation

### **Phase 2: Frontend Update (FE Team)**

#### **2.1 Forms & Input**
- [ ] Update form "Create Harga Pasar"
  - [ ] Change field name: `harga_per_kg` ? `harga_per_ekor`
  - [ ] Update validation (min: 10000, max: 100000)
  - [ ] Update placeholder & helper text
- [ ] Update form "Estimasi Keuntungan"
  - [ ] Remove field `berat_rata_rata`
  - [ ] Update API call (remove parameter)
- [ ] Update form "Create Panen" (jika ada tampilan harga)

#### **2.2 Display & Tables**
- [ ] Update "List Harga Pasar" table
  - [ ] Column header: "Harga Per Kg" ? "Harga Per Ekor"
- [ ] Update "Analisis Keuntungan" detail view
  - [ ] Hide/remove `berat_rata_rata`
  - [ ] Hide/remove `total_berat_kg`
  - [ ] Update `harga_pasar_per_kg` ? `harga_pasar_per_ekor`
  - [ ] Update `harga_pokok_produksi` label (per kg ? per ekor)
- [ ] Update "List Panen" table
  - [ ] Remove columns: berat, total berat
  - [ ] Update harga column header
- [ ] Update "Dashboard" cards/stats
  - [ ] Review semua tampilan yang show harga

#### **2.3 API Integration**
- [ ] Update API service/repository
  ```javascript
  // ? OLD
  const estimasi = await api.get('/harga-pasar/estimasi-keuntungan', {
    params: { total_ayam, berat_rata_rata, tanggal }
  });
  
  // ? NEW
  const estimasi = await api.get('/harga-pasar/estimasi-keuntungan', {
    params: { total_ayam, tanggal }
  });
  ```
- [ ] Update response mapping
  ```javascript
  // ? OLD
  const hargaPerKg = response.data.harga_per_kg;
  
  // ? NEW
  const hargaPerEkor = response.data.harga_per_ekor;
  ```

#### **2.4 Validations**
- [ ] Update client-side validation
  ```javascript
  // ? OLD
  harga_per_kg: Yup.number()
    .min(1, 'Minimal Rp 1')
    .max(1000000, 'Maksimal Rp 1.000.000')
    .required('Harga harus diisi')
  
  // ? NEW
  harga_per_ekor: Yup.number()
    .min(10000, 'Minimal Rp 10.000')
    .max(100000, 'Maksimal Rp 100.000')
    .required('Harga harus diisi')
  ```

#### **2.5 Charts & Reports**
- [ ] Update chart labels
- [ ] Update Excel export templates
- [ ] Update PDF report templates
- [ ] Add migration info banner (if needed)

#### **2.6 Language/Translations**
- [ ] Update i18n files
  ```json
  // ? OLD
  {
    "harga_per_kg": "Harga Per Kg",
    "berat_rata_rata": "Berat Rata-Rata"
  }
  
  // ? NEW
  {
    "harga_per_ekor": "Harga Per Ekor"
  }
  ```

---

## ?? Testing Guide

### **Manual Testing Checklist**

#### **1. Harga Pasar Module**
- [ ] Create harga pasar baru dengan harga per ekor
- [ ] Edit harga pasar existing
- [ ] List harga pasar (verify column headers)
- [ ] Delete harga pasar

#### **2. Estimasi Keuntungan**
- [ ] Input form (verify no berat_rata_rata field)
- [ ] Submit form dan cek response
- [ ] Verify calculation: `total_ayam × harga_per_ekor`

#### **3. Panen Module**
- [ ] View detail panen (verify no berat fields displayed)
- [ ] View analisis keuntungan panen
- [ ] List panen dengan filtering
- [ ] Export panen ke Excel/PDF

#### **4. Dashboard**
- [ ] View dashboard cards
- [ ] View ringkasan keuntungan
- [ ] View chart (jika ada)

### **API Testing (Postman/Insomnia)**

**Test 1: Create Harga Pasar**
```http
POST /api/harga-pasar
Content-Type: application/json

{
  "harga_per_ekor": 42000,
  "tanggal_mulai": "2025-01-20",
  "wilayah": "Kupang"
}

Expected Response: 201 Created
```

**Test 2: Estimasi Keuntungan**
```http
GET /api/harga-pasar/estimasi-keuntungan?total_ayam=100&tanggal_referensi=2025-01-20

Expected Response:
{
  "success": true,
  "data": {
    "total_ayam": 100,
    "harga_per_ekor": 42000,
    "total_pendapatan": 4200000
  }
}
```

**Test 3: Analisis Keuntungan Panen**
```http
GET /api/panen/{id}/analisis-keuntungan

Expected Response:
{
  "success": true,
  "data": {
    "jumlah_ayam": 50,
    "harga_pasar_per_ekor": 42000,
    "pendapatan_kotor": 2100000,
    "harga_pokok_produksi": 30000
  }
}
```

---

## ?? Data Migration Impact

### **Contoh Perbandingan Data**

**Scenario: 100 ekor ayam dengan berat rata-rata 2.5 kg**

| Metrik | **Sistem Lama (Per Kg)** | **Sistem Baru (Per Ekor)** |
|--------|--------------------------|----------------------------|
| Total Berat | 250 kg | - (tidak relevan) |
| Harga | Rp 35.000/kg | Rp 42.000/ekor |
| **Pendapatan** | **Rp 8.750.000** | **Rp 4.200.000** |
| HPP | Rp 25.000/kg | Rp 30.000/ekor |

?? **PENTING**: Nilai pendapatan akan tampak **turun 52%** setelah migration!  
Ini **BUKAN BUG**, tapi perubahan cara perhitungan.

### **User Communication**
Tambahkan **notification/banner** di aplikasi:
```
?? PERUBAHAN SISTEM PERHITUNGAN
Sejak tanggal [DD/MM/YYYY], sistem harga berubah dari per-kilogram 
menjadi per-ekor. Data historis sebelum tanggal tersebut tetap 
menggunakan perhitungan lama. Untuk perbandingan yang akurat, 
gunakan filter tanggal setelah tanggal perubahan.
```

---

## ?? FAQ & Troubleshooting

### **Q: Kenapa pendapatan turun drastis setelah update?**
A: Bukan turun, tapi **cara hitung berubah**. Sebelumnya dihitung dari total berat (kg), sekarang dari jumlah ekor. Nilai riilnya berbeda karena basis perhitungannya berbeda.

### **Q: Bagaimana dengan data historis panen lama?**
A: Data lama tetap ada di database dengan field `BeratRataRata`. Backend masih menyimpan field ini, tapi frontend tidak perlu menampilkannya lagi.

### **Q: Apakah perlu migrasi data lama?**
A: **TIDAK** perlu migrasi data panen lama. Hanya data `HargaPasar` yang diubah field-nya. Data panen tetap utuh.

### **Q: Field `BeratRataRata` di model `Panen` dihapus?**
A: **TIDAK DIHAPUS**, hanya tidak ditampilkan di frontend. Field ini tetap ada untuk keperluan tracking/analytics internal.

### **Q: Apakah API backward compatible?**
A: **TIDAK**. Ini adalah breaking change. Frontend HARUS update sesuai guide ini.

---

## ?? Contact & Support

Jika ada pertanyaan atau issue saat implementasi:
- ?? **Bug Report**: Buat issue di GitHub dengan label `frontend-migration`
- ?? **Discussion**: Slack channel `#sim-ternak-ayam-frontend`
- ?? **Email**: dev-team@simternakayam.com

---

## ?? Related Documents
- [Backend Implementation Summary](./HargaPerEkor_BackendChanges.md)
- [API Documentation - Harga Pasar](../API_Docs/HargaPasar.md)
- [API Documentation - Panen](../API_Docs/Panen.md)
- [Database Migration Guide](./DatabaseMigration_HargaPerEkor.md)

---

**Last Updated**: 2025-01-20  
**Version**: 1.0.0  
**Status**: ? Ready for Implementation

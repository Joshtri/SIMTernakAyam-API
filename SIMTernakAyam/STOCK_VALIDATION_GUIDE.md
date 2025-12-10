# Panduan Validasi Stok - SIMTernakAyam

## ?? Tujuan
Panduan ini menjelaskan fitur validasi stok yang telah ditingkatkan untuk mencegah penggunaan vaksin dan pakan yang melebihi stok tersedia.

## ?? Fitur Baru

### 1. **Enhanced Response DTOs**

#### Vaksin Response DTO
```json
{
  "id": "guid",
  "namaVaksin": "string",
  "stok": 5,
  "stokAwal": 10,
  "stokTerpakai": 5,
  "stokTersisa": 5,
  "isStokCukup": true,
  "statusStok": "Aman",  // "Aman", "Menipis", "Kritis", "Habis"
  "bulan": 10,
  "tahun": 2025,
  "tipe": "Vaksin"
}
```

#### Pakan Response DTO
```json
{
  "id": "guid",
  "namaPakan": "string",
  "stokKg": 50.0,
  "stokAwalKg": 100.0,
  "stokTerpakaiKg": 50.0,
  "stokTersisaKg": 50.0,
  "isStokCukup": true,
  "statusStok": "Aman",  // "Aman", "Menipis", "Kritis", "Habis"
  "bulan": 10,
  "tahun": 2025
}
```

### 2. **New API Endpoints**

#### Vaksin Controller
- `GET /api/vaksins/with-usage-detail` - Semua vaksin dengan detail penggunaan
- `GET /api/vaksins/{id}/usage-detail` - Detail penggunaan vaksin tertentu
- `GET /api/vaksins/{id}/check-availability/{jumlahDibutuhkan}` - Cek ketersediaan stok

#### Pakan Controller
- `GET /api/pakans/with-usage-detail` - Semua pakan dengan detail penggunaan
- `GET /api/pakans/{id}/usage-detail` - Detail penggunaan pakan tertentu
- `GET /api/pakans/{id}/check-availability/{jumlahDibutuhkan}` - Cek ketersediaan stok

#### Biaya Controller
- `POST /api/biayas/validate-stock-before-create` - Validasi stok sebelum buat operasional
- `GET /api/biayas/stock-info` - Info lengkap stok vaksin dan pakan

#### Operasional Controller
- `POST /api/operasionals/validate-stock` - Validasi stok sebelum operasional

### 3. **Stock Availability Response**

Contoh response untuk cek ketersediaan stok:

```json
{
  "success": true,
  "message": "Berhasil mengecek ketersediaan stok",
  "data": {
    "vaksinId": "guid",
    "namaVaksin": "Medivac ND La Sota",
    "stokTersedia": 3,
    "jumlahDibutuhkan": 2,
    "isAvailable": true,
    "stokKurang": 0,
    "stokTerpakai": 2,
    "statusStok": "Menipis",
    "rekomendasi": "Stok akan menipis setelah penggunaan. Pertimbangkan untuk menambah stok."
  }
}
```

## ?? Status Stok

### Vaksin
- **Aman**: > 5 dosis
- **Menipis**: 3-5 dosis
- **Kritis**: 1-2 dosis
- **Habis**: 0 dosis

### Pakan
- **Aman**: > 50 kg
- **Menipis**: 11-50 kg
- **Kritis**: 1-10 kg
- **Habis**: 0 kg

## ?? Cara Penggunaan

### 1. **Sebelum Membuat Operasional**

```javascript
// Validasi stok sebelum operasional
const validateStock = async (vaksinId, pakanId, jumlah) => {
  const response = await fetch('/api/operasionals/validate-stock', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({
      vaksinId: vaksinId,
      pakanId: pakanId,
      jumlah: jumlah
    })
  });
  
  const result = await response.json();
  
  if (!result.success) {
    alert('Error: ' + result.message);
    return false;
  }
  
  // Tampilkan warning jika ada
  if (result.data.warnings && result.data.warnings.length > 0) {
    const confirmProceed = confirm('Ada peringatan stok. Lanjutkan?');
    return confirmProceed;
  }
  
  return true;
};
```

### 2. **Cek Detail Stok**

```javascript
// Ambil detail penggunaan vaksin
const getVaksinDetail = async (vaksinId) => {
  const response = await fetch(`/api/vaksins/${vaksinId}/usage-detail`);
  const result = await response.json();
  
  if (result.success) {
    console.log('Stok tersisa:', result.data.stokTersisa);
    console.log('Status:', result.data.statusStok);
  }
};

// Ambil detail penggunaan pakan
const getPakanDetail = async (pakanId) => {
  const response = await fetch(`/api/pakans/${pakanId}/usage-detail`);
  const result = await response.json();
  
  if (result.success) {
    console.log('Stok tersisa:', result.data.stokTersisaKg, 'kg');
    console.log('Status:', result.data.statusStok);
  }
};
```

### 3. **Tampilkan Info Stok di Frontend**

```javascript
// Ambil semua info stok
const getAllStockInfo = async () => {
  const response = await fetch('/api/biayas/stock-info');
  const result = await response.json();
  
  if (result.success) {
    // Tampilkan vaksin dengan warning
    result.data.vaksin.forEach(vaksin => {
      if (vaksin.statusStok === 'Kritis' || vaksin.statusStok === 'Menipis') {
        showWarning(`Vaksin ${vaksin.namaVaksin}: ${vaksin.statusStok} (${vaksin.stokTersisa} dosis)`);
      }
    });
    
    // Tampilkan pakan dengan warning
    result.data.pakan.forEach(pakan => {
      if (pakan.statusStok === 'Kritis' || pakan.statusStok === 'Menipis') {
        showWarning(`Pakan ${pakan.namaPakan}: ${pakan.statusStok} (${pakan.stokTersisaKg} kg)`);
      }
    });
  }
};
```

## ?? Validasi yang Sudah Ada

1. **Di OperasionalService**: Validasi stok sebelum create/update operasional
2. **Di StokService**: Validasi stok saat pengurangan langsung
3. **Di Repository**: Validasi periode stok (bulan/tahun)

## ?? Keuntungan Fitur Baru

1. **Pencegahan Overuse**: Tidak bisa menggunakan stok melebihi yang tersedia
2. **Real-time Monitoring**: Informasi stok real-time dengan detail penggunaan
3. **Smart Recommendations**: Sistem memberikan rekomendasi berdasarkan status stok
4. **Better UX**: Frontend bisa menampilkan warning dan konfirmasi sebelum operasional
5. **Audit Trail**: Tracking lengkap penggunaan stok per periode

## ?? Konfigurasi Threshold

Threshold dapat disesuaikan di service:

```csharp
// VaksinService.cs
private static string GetStatusStok(int stokTersisa)
{
    return stokTersisa switch
    {
        0 => "Habis",
        <= 2 => "Kritis",    // Ubah angka ini
        <= 5 => "Menipis",   // Ubah angka ini
        _ => "Aman"
    };
}

// PakanService.cs  
private static string GetStatusStok(decimal stokTersisa)
{
    return stokTersisa switch
    {
        0 => "Habis",
        <= 10 => "Kritis",   // Ubah angka ini
        <= 50 => "Menipis",  // Ubah angka ini
        _ => "Aman"
    };
}
```

## ?? Testing Endpoints

Gunakan file test HTTP berikut untuk testing:

```http
### Test Validasi Stok Operasional
POST https://localhost:7777/api/operasionals/validate-stock
Content-Type: application/json

{
  "vaksinId": "509b9562-7cee-4dd4-8ce9-d6a53569b0e5",
  "jumlah": 2
}

### Test Cek Ketersediaan Vaksin
GET https://localhost:7777/api/vaksins/509b9562-7cee-4dd4-8ce9-d6a53569b0e5/check-availability/2

### Test Cek Ketersediaan Pakan
GET https://localhost:7777/api/pakans/128c96b6-7df0-4388-b20f-a56292fac2db/check-availability/100

### Test Info Stok Lengkap
GET https://localhost:7777/api/biayas/stock-info
```

---

**Catatan**: Setelah implementasi ini, frontend perlu diupdate untuk menggunakan endpoint baru dan menampilkan informasi stok yang lebih detail kepada pengguna.
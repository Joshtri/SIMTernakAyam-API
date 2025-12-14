# ?? TROUBLESHOOTING: LAPORAN KEUNTUNGAN KOSONG

## ?? **MASALAH YANG DITEMUKAN**

Anda melaporkan bahwa laporan keuntungan menampilkan `totalPendapatan = 0` meski ada data panen:

```json
{
  "success": true,
  "message": "Laporan keuntungan bulan 12/2025 berhasil diambil",
  "data": {
    "tahun": 2025,
    "bulan": 12,
    "total": {
      "totalPanen": 3,
      "totalAyam": 2962,
      "totalBerat": 6219.20,
      "totalPendapatan": 0,      // ? KOSONG (HARUSNYA ADA NILAI)
      "rataRataBeratPerEkor": 2.0996623902768399729912221472
    },
    "detailHarian": [
      {
        "tanggal": "2025-12-10T00:00:00",
        "jumlahPanen": 3,
        "totalAyam": 2962,
        "totalBerat": 6219.20,
        "totalKeuntungan": 0,    // ? HARUSNYA ADA NILAI
        "hargaPerKg": 0,         // ? TIDAK ADA HARGA PASAR
        "detailPanen": [
          {
            "panenId": "d7f035eb-8d42-4486-a963-d7fb7ee17206",
            "jumlahAyam": 990,
            "totalBerat": 2079.00,
            "hargaPerKg": 0,
            "totalPendapatan": 0,
            "hargaPasarInfo": null  // ? TIDAK ADA INFO HARGA PASAR
          }
          // ... 2 panen lainnya dengan masalah sama
        ]
      }
    ],
    "hargaPasarBulanIni": [],    // ? ARRAY KOSONG
    "rataRataHargaPerKg": 0,
    "fluktusiHarga": {
      "hargaTerendah": 0,
      "hargaTertinggi": 0,
      "selisihHarga": 0,
      "persentaseFluktuasi": 0
    }
  }
}
```

**Data panen yang ada:**
- ? 3 panen pada tanggal 2025-12-10
- ? Total 2,962 ekor ayam
- ? Total berat 6,219.20 kg
- ? Tapi tidak ada harga pasar aktif untuk tanggal tersebut

## ?? **ROOT CAUSE ANALYSIS**

**MASALAH UTAMA:** Tidak ada **harga pasar yang aktif** untuk tanggal panen (2025-12-10).

### Logic API Keuntungan:
```csharp
// Di HargaPasarService.cs - GetLaporanKeuntunganBulananAsync()
foreach (var panen in panenHarian)
{
    // Cari harga pasar yang aktif pada tanggal panen
    var hargaPasar = await _hargaPasarRepository.GetHargaAktifByTanggalAsync(panen.TanggalPanen);
    
    // Jika tidak ada harga pasar, tetap tampilkan data tapi pendapatan = 0
    if (hargaPasar != null)
    {
        pendapatanPanen = totalBerat * hargaPasar.HargaPerKg;
        hargaPerKg = hargaPasar.HargaPerKg;
    }
    else
    {
        pendapatanPanen = 0;  // ? Pendapatan 0 karena tidak ada harga pasar
        hargaPerKg = 0;
    }
}
```

### Kriteria Pencarian Harga Pasar:
```csharp
// Di HargaPasarRepository.GetHargaAktifByTanggalAsync()
return await _context.HargaPasar
    .Where(h => h.IsAktif &&                           // ? Harus IsAktif = true
               h.TanggalMulai <= tanggal &&            // ? TanggalMulai <= 2025-12-10
               (h.TanggalBerakhir == null ||           // ? TanggalBerakhir = null ATAU
                h.TanggalBerakhir >= tanggal))         // ? TanggalBerakhir >= 2025-12-10
    .OrderByDescending(h => h.TanggalMulai)
    .FirstOrDefaultAsync();
```

**Kesimpulan:**
- Tidak ada record `HargaPasar` di database dengan:
  - `IsAktif = true`
  - `TanggalMulai <= 2025-12-10`
  - `TanggalBerakhir = null` atau `TanggalBerakhir >= 2025-12-10`

## ??? **SOLUSI YANG TELAH DIIMPLEMENTASI**

### 1. **? Enhanced Service Logic**
- Service sekarang **tetap menampilkan data panen** meskipun tidak ada harga pasar
- Keuntungan = 0 jika tidak ada harga pasar, tapi data panen tetap muncul
- Tambah **debug info** dalam response message

### 2. **? Debug Endpoints**
Tambahan endpoint untuk troubleshooting:
```http
GET /api/harga_pasar/debug/cek-harga-pasar?tanggal=2025-12-10
GET /api/harga_pasar/debug/cek-data-panen?startDate=2025-12-01&endDate=2025-12-31
```

### 3. **? Informative Error Messages**
Response sekarang include informasi debug:
```json
{
  "message": "Laporan keuntungan bulan 12/2025 berhasil dibuat. DEBUG: Ditemukan 3 panen dalam periode 2025-12-01 to 2025-12-31. PERHATIAN: 3 panen tidak memiliki harga pasar aktif: [Panen d7f035eb pada 2025-12-10, ...]"
}
```

## ?? **LANGKAH-LANGKAH PENYELESAIAN**

### **STEP 1: Diagnosis - Cek Harga Pasar yang Ada** 
```http
### 1.1 Cek harga pasar untuk tanggal tertentu
GET https://localhost:7195/api/harga_pasar/debug/cek-harga-pasar?tanggal=2025-12-10

### 1.2 Lihat semua harga pasar yang ada
GET https://localhost:7195/api/harga_pasar

### 1.3 Cek harga pasar terbaru yang aktif
GET https://localhost:7195/api/harga_pasar/terbaru
```

**Expected Output (jika ada masalah):**
```json
{
  "success": false,
  "message": "Tidak ada harga pasar aktif pada tanggal 2025-12-10",
  "data": {
    "tanggalDicek": "2025-12-10T00:00:00",
    "hargaPasarDitemukan": null,
    "rekomendasiSolusi": "? MASALAH: Tidak ada harga pasar aktif pada tanggal ini. Silakan buat harga pasar baru yang mencakup tanggal ini."
  }
}
```

### **STEP 2: Buat Harga Pasar Baru**

**PENTING:** Pastikan hanya ada **SATU** harga pasar dengan `IsAktif = true` pada satu waktu.

```http
### 2.1 RECOMMENDED: Nonaktifkan semua harga pasar lama (jika ada)
POST https://localhost:7195/api/harga_pasar/deactivate-all
Content-Type: application/json

### 2.2 Buat harga pasar baru untuk Desember 2025
POST https://localhost:7195/api/harga_pasar
Content-Type: application/json

{
  "hargaPerKg": 27000,
  "tanggalMulai": "2025-12-01T00:00:00Z",
  "tanggalBerakhir": null,
  "keterangan": "Harga pasar Desember 2025",
  "wilayah": "Jakarta",
  "isAktif": true
}
```

**Alternative:** Jika ingin set periode tertentu:
```json
{
  "hargaPerKg": 27000,
  "tanggalMulai": "2025-12-01T00:00:00Z",
  "tanggalBerakhir": "2025-12-31T23:59:59Z",  // Berlaku sampai akhir Desember
  "keterangan": "Harga pasar Desember 2025",
  "wilayah": "Jakarta",
  "isAktif": true
}
```

**Expected Response:**
```json
{
  "success": true,
  "message": "Harga pasar berhasil ditambahkan",
  "data": {
    "id": "new-guid-here",
    "hargaPerKg": 27000,
    "tanggalMulai": "2025-12-01T00:00:00Z",
    "tanggalBerakhir": null,
    "isAktif": true,
    "wilayah": "Jakarta"
  }
}
```

### **STEP 3: Verifikasi Fix**
```http
### 3.1 Test harga pasar untuk tanggal panen
GET https://localhost:7195/api/harga_pasar/debug/cek-harga-pasar?tanggal=2025-12-10

### 3.2 Test laporan keuntungan bulanan
GET https://localhost:7195/api/harga_pasar/laporan-keuntungan-bulanan?tahun=2025&bulan=12

### 3.3 Test keuntungan per panen
GET https://localhost:7195/api/harga_pasar/keuntungan-panen/d7f035eb-8d42-4486-a963-d7fb7ee17206
```

**Expected:** Data keuntungan terisi dengan lengkap

## ?? **HASIL SETELAH FIX**

Setelah menambahkan harga pasar yang sesuai, response akan menjadi:

```json
{
  "success": true,
  "message": "Laporan keuntungan bulan 12/2025 berhasil diambil",
  "data": {
    "tahun": 2025,
    "bulan": 12,
    "total": {
      "totalPanen": 3,
      "totalAyam": 2962,
      "totalBerat": 6219.20,
      "totalPendapatan": 167918400,  // ? Rp 167,918,400 (6219.20 kg × Rp 27,000)
      "rataRataBeratPerEkor": 2.10
    },
    "detailHarian": [
      {
        "tanggal": "2025-12-10T00:00:00",
        "jumlahPanen": 3,
        "totalAyam": 2962,
        "totalBerat": 6219.20,
        "totalKeuntungan": 167918400,  // ? TERISI
        "hargaPerKg": 27000,           // ? TERISI
        "detailPanen": [
          {
            "panenId": "d7f035eb-8d42-4486-a963-d7fb7ee17206",
            "tanggalPanen": "2025-12-10T00:00:00",
            "jumlahAyam": 990,
            "totalBerat": 2079.00,
            "beratRataRata": 2.10,
            "hargaPerKg": 27000,       // ? TERISI
            "totalPendapatan": 56133000, // ? TERISI (2079 × 27000)
            "namaKandang": "Kandang 1",
            "hargaPasarInfo": {        // ? TERISI
              "id": "new-guid-here",
              "hargaPerKg": 27000,
              "tanggalMulai": "2025-12-01T00:00:00",
              "tanggalBerakhir": null,
              "wilayah": "Jakarta",
              "keterangan": "Harga pasar Desember 2025"
            }
          },
          {
            "panenId": "c21e784c-c776-464a-a7a3-58211dcfe80d",
            "jumlahAyam": 981,
            "totalBerat": 2158.20,
            "hargaPerKg": 27000,
            "totalPendapatan": 58271400,  // ? TERISI (2158.20 × 27000)
            "namaKandang": "Kandang 2"
          },
          {
            "panenId": "3d9ba2e9-49bb-4544-8e9a-c92331324dc3",
            "jumlahAyam": 991,
            "totalBerat": 1982.00,
            "hargaPerKg": 27000,
            "totalPendapatan": 53514000,  // ? TERISI (1982 × 27000)
            "namaKandang": "Kandang 3"
          }
        ]
      }
    ],
    "hargaPasarBulanIni": [           // ? TERISI
      {
        "id": "new-guid-here",
        "hargaPerKg": 27000,
        "tanggalMulai": "2025-12-01T00:00:00",
        "tanggalBerakhir": null,
        "wilayah": "Jakarta",
        "keterangan": "Harga pasar Desember 2025"
      }
    ],
    "rataRataHargaPerKg": 27000,      // ? TERISI
    "fluktusiHarga": {
      "hargaTerendah": 27000,
      "hargaTertinggi": 27000,
      "selisihHarga": 0,
      "persentaseFluktuasi": 0
    }
  }
}
```

## ?? **TEST FILE**

Gunakan file test yang sudah disediakan:
- **File:** `SIMTernakAyam/Tests/HargaPasarKeuntunganTests.http`
- **Section:** `?? DEBUG ENDPOINTS - TROUBLESHOOTING`

## ? **QUICK FIX COMMANDS**

```http
### 1. DEBUG: Cek harga pasar untuk tanggal panen
GET https://localhost:7195/api/harga_pasar/debug/cek-harga-pasar?tanggal=2025-12-10

### 2. FIX: Nonaktifkan semua harga lama
POST https://localhost:7195/api/harga_pasar/deactivate-all
Content-Type: application/json

### 3. FIX: Buat harga pasar baru
POST https://localhost:7195/api/harga_pasar
Content-Type: application/json

{
  "hargaPerKg": 27000,
  "tanggalMulai": "2025-12-01T00:00:00Z",
  "tanggalBerakhir": null,
  "keterangan": "Harga pasar Desember 2025",
  "wilayah": "Jakarta",
  "isAktif": true
}

### 4. VERIFY: Test laporan keuntungan
GET https://localhost:7195/api/harga_pasar/laporan-keuntungan-bulanan?tahun=2025&bulan=12
```

## ?? **BEST PRACTICES**

### 1. **Manajemen Harga Pasar Aktif**

**Rule Penting:**
- ? **HANYA SATU** harga pasar dengan `IsAktif = true` pada satu waktu
- ? Gunakan `TanggalBerakhir = null` untuk harga berlaku terus hingga update berikutnya
- ? Jika ada perubahan harga, nonaktifkan yang lama sebelum aktivasi yang baru

### 2. **Validasi Sebelum Input Panen**

Sebelum input data panen, pastikan ada harga pasar aktif:

```http
GET /api/harga_pasar/terbaru
```

Jika response menunjukkan `data: null` atau tanggal tidak sesuai, tambahkan harga pasar dulu.

### 3. **Monitoring Harga Pasar**

Cek status harga pasar secara berkala:

```http
# Lihat semua harga pasar
GET /api/harga_pasar

# Response harus menunjukkan max 1 harga dengan IsAktif = true
```

## ?? **DEBUG CHECKLIST**

Jika laporan keuntungan masih kosong, cek:

- [ ] **Apakah ada harga pasar dengan `IsAktif = true`?**
  ```http
  GET /api/harga_pasar/terbaru
  ```
  Expected: Ada data dengan `isAktif: true`

- [ ] **Apakah periode harga pasar mencakup tanggal panen?**
  ```
  TanggalMulai <= TanggalPanen <= TanggalBerakhir (atau TanggalBerakhir = null)
  Contoh: TanggalMulai = 2025-12-01, TanggalPanen = 2025-12-10 ?
  ```

- [ ] **Apakah hanya ada SATU harga aktif?**
  ```
  Jika ada lebih dari 1, nonaktifkan yang tidak digunakan dengan:
  POST /api/harga_pasar/deactivate-all
  ```

- [ ] **Apakah data panen valid?**
  ```http
  GET /api/panen
  # Cek: JumlahEkorPanen > 0, BeratRataRata > 0, TotalBerat > 0
  ```

## ?? **CONTOH SKENARIO**

### Skenario 1: Harga Pasar Berlaku Permanen
```json
{
  "hargaPerKg": 27000,
  "tanggalMulai": "2025-01-01T00:00:00Z",
  "tanggalBerakhir": null,  // Berlaku terus sampai dinonaktifkan
  "keterangan": "Harga Standard 2025",
  "wilayah": "Indonesia",
  "isAktif": true
}
```

### Skenario 2: Harga Pasar Bulanan
```json
{
  "hargaPerKg": 27000,
  "tanggalMulai": "2025-12-01T00:00:00Z",
  "tanggalBerakhir": "2025-12-31T23:59:59Z",
  "keterangan": "Harga Desember 2025",
  "wilayah": "Jakarta",
  "isAktif": true
}
```

### Skenario 3: Multiple Harga untuk Different Wilayah
```json
// Harga untuk Jakarta
{
  "hargaPerKg": 27000,
  "tanggalMulai": "2025-12-01T00:00:00Z",
  "tanggalBerakhir": null,
  "wilayah": "Jakarta",
  "isAktif": true
}

// Harga untuk Bandung (BEDA wilayah)
{
  "hargaPerKg": 26000,
  "tanggalMulai": "2025-12-01T00:00:00Z",
  "tanggalBerakhir": null,
  "wilayah": "Bandung",
  "isAktif": true
}
```

## ?? **STATUS**
- ? **Root cause identified:** Tidak ada harga pasar aktif untuk tanggal panen
- ? **Enhanced service logic:** Tetap tampilkan data panen meskipun tidak ada harga  
- ? **Debug endpoints added:** `/debug/cek-harga-pasar`, `/debug/cek-data-panen`
- ? **Solution documented:** Langkah-langkah fix yang jelas
- ? **Action required:** Buat harga pasar untuk tanggal 2025-12-10

---

**Last Updated:** 2025-12-14
**Status:** ? Documented & Ready to Fix
**Estimated Fix Time:** 2 minutes (just add HargaPasar record)
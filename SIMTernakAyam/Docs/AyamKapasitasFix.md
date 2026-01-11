# Perbaikan Endpoint Kapasitas Kandang (FIXED v2)

## Masalah yang Diperbaiki

### 1. Format Parameter yang Rumit
**Sebelum:**
- Parameter: `tanggalMasuk` dengan tipe `DateTime?`
- Format: `2026-02-02T00:00:00.000Z` (ISO 8601 lengkap)
- Contoh: `?tanggalMasuk=2026-02-02T00:00:00.000Z`

**Sesudah:**
- Parameter: `periodeRencana` dengan tipe `string?`
- Format: `yyyy-MM` (tahun-bulan saja)
- Contoh: `?periodeRencana=2026-02`

### 2. Logika Penentuan Periode Ayam Sisa yang Salah (FIXED v2)

#### **? BUG v1 (Sudah Diperbaiki):**
```
- Input: periodeRencana=2026-02 (Februari 2026)
- Logika: Cek ayam yang tahun/bulan berbeda dari Februari 2026
- Problem: Ayam Januari 2026 (200 ekor) dianggap "sisa"
- Hasil: "PeriodeAyamSisa: Desember 2025" ? (SALAH!)
```

#### **? FIXED v2:**
```
- Input: periodeRencana=2026-02 (Februari 2026)
- Logika: Cek ayam yang TanggalMasuk < 1 Februari 2026
- Ayam Januari 2026 = TanggalMasuk < 1 Feb 2026 ? (Benar! Ini sisa)
- Hasil: "PeriodeAyamSisa: Januari 2026" ? (BENAR!)
```

### Penjelasan Logika Baru:

```csharp
// Input: periodeRencana = "2026-02"
// Parse menjadi: 2026-02-01 00:00:00 UTC

// Contoh Data Ayam:
// 1. Ayam A: TanggalMasuk = 2025-12-15 ? SISA ? (sebelum Feb 2026)
// 2. Ayam B: TanggalMasuk = 2026-01-20 ? SISA ? (sebelum Feb 2026)
// 3. Ayam C: TanggalMasuk = 2026-02-05 ? BUKAN SISA ? (periode yg sama)
// 4. Ayam D: TanggalMasuk = 2026-02-28 ? BUKAN SISA ? (periode yg sama)

// PeriodeAyamSisa akan ambil dari ayam yang paling BARU tapi masih sebelum periode rencana
// Dalam contoh: Ayam B (Januari 2026) adalah yang paling baru
// Result: "PeriodeAyamSisa: Januari 2026" ?
```

## Endpoint

```
GET /api/ayams/kandang/{kandangId}/kapasitas?periodeRencana={yyyy-MM}
```

### Parameter
- `kandangId` (path, required): GUID kandang
- `periodeRencana` (query, optional): Format `yyyy-MM` (contoh: `2026-02`)

### Response
```json
{
    "success": true,
    "message": "Berhasil mengambil informasi kapasitas kandang.",
    "data": {
        "kandangId": "fd082b84-41c8-45d4-8480-ddb918f74cca",
        "namaKandang": "Kandang 2",
        "kapasitasKandang": 1000,
        "totalAyamHidup": 200,
        "sisaAyamDariPeriodeSebelumnya": 200,
        "kapasitasTersedia": 800,
        "periodeAyamSisa": "Januari 2026",
        "adaSisaAyam": true,
        "persentasePengisian": 20.0
    }
}
```

## Contoh Penggunaan

### Skenario 1: Input Februari 2026, Ada Ayam Januari 2026
```http
GET /api/ayams/kandang/{id}/kapasitas?periodeRencana=2026-02
```

**Data Kandang:**
- Ayam di Januari 2026: 200 ekor (masih hidup)

**Response:**
```json
{
    "periodeAyamSisa": "Januari 2026",  // ? Benar!
    "sisaAyamDariPeriodeSebelumnya": 200,
    "adaSisaAyam": true
}
```

### Skenario 2: Input Maret 2026, Ada Ayam Februari 2026
```http
GET /api/ayams/kandang/{id}/kapasitas?periodeRencana=2026-03
```

**Data Kandang:**
- Ayam di Januari 2026: 50 ekor
- Ayam di Februari 2026: 150 ekor

**Response:**
```json
{
    "periodeAyamSisa": "Februari 2026",  // ? Ambil yang paling baru
    "sisaAyamDariPeriodeSebelumnya": 200,  // 50 + 150
    "adaSisaAyam": true
}
```

### Skenario 3: Input Februari 2026, Tidak Ada Ayam Sebelumnya
```http
GET /api/ayams/kandang/{id}/kapasitas?periodeRencana=2026-02
```

**Data Kandang:**
- Tidak ada ayam sama sekali

**Response:**
```json
{
    "periodeAyamSisa": null,
    "sisaAyamDariPeriodeSebelumnya": 0,
    "adaSisaAyam": false,
    "kapasitasTersedia": 1000
}
```

## Perubahan Kode

### Controller (AyamController.cs)
```csharp
public async Task<IActionResult> GetKapasitasKandang(
    Guid kandangId, 
    [FromQuery] string? periodeRencana = null)
{
    // Parse format yyyy-MM
    DateTime? tanggalMasukRencana = null;
    if (!string.IsNullOrWhiteSpace(periodeRencana))
    {
        if (DateTime.TryParseExact(periodeRencana + "-01", "yyyy-MM-dd", 
            CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
        {
            tanggalMasukRencana = parsedDate;
        }
        else
        {
            return Error("Format periode tidak valid. Gunakan format yyyy-MM (contoh: 2026-02).", 400);
        }
    }
    
    var kapasitas = await _ayamService.GetKapasitasKandangAsync(kandangId, tanggalMasukRencana);
    return Success(kapasitas, "Berhasil mengambil informasi kapasitas kandang.");
}
```

### Service (AyamService.cs) - FIXED v2
```csharp
if (tanggalMasukRencana.HasValue)
{
    // Buat tanggal awal periode rencana (tanggal 1 bulan tersebut)
    var periodeRencanaStart = new DateTime(
        tanggalMasukRencana.Value.Year, 
        tanggalMasukRencana.Value.Month, 
        1, 0, 0, 0, DateTimeKind.Utc);

    // ? Cek ayam yang TanggalMasuk SEBELUM periode rencana
    var ayamSebelumPeriodeRencana = ayamList
        .Where(a => a.TanggalMasuk < periodeRencanaStart)
        .OrderByDescending(a => a.TanggalMasuk) // Urutkan dari yang paling baru
        .ToList();

    if (ayamSebelumPeriodeRencana.Any())
    {
        // Hitung total ayam hidup
        foreach (var ayam in ayamSebelumPeriodeRencana)
        {
            var dipanen = panenData.ContainsKey(ayam.Id) ? panenData[ayam.Id] : 0;
            var mati = mortalitasData.ContainsKey(ayam.Id) ? mortalitasData[ayam.Id] : 0;
            var hidup = ayam.JumlahMasuk - dipanen - mati;
            if (hidup > 0)
            {
                sisaDariPeriodeSebelumnya += hidup;
            }
        }

        if (sisaDariPeriodeSebelumnya > 0)
        {
            adaSisaAyam = true;
            // ? Ambil periode dari ayam yang paling BARU (tapi masih sebelum periode rencana)
            var ayamTerbaru = ayamSebelumPeriodeRencana.FirstOrDefault();
            if (ayamTerbaru != null)
            {
                periodeAyamSisa = ayamTerbaru.TanggalMasuk.ToString("MMMM yyyy", new CultureInfo("id-ID"));
            }
        }
    }
}
```

## Testing
File test tersedia di: `SIMTernakAyam\Tests\AyamKapasitasTests.http`

### Test Cases
1. ? Get kapasitas tanpa parameter periode
2. ? Get kapasitas dengan periode Februari 2026 (ayam Januari = sisa)
3. ? Get kapasitas dengan periode Maret 2026 (ayam Jan & Feb = sisa)
4. ? Get kapasitas dengan periode Desember 2025 (tidak ada sisa)
5. ? Error handling untuk format invalid
6. ? Error handling untuk format invalid (string sembarang)

## Breaking Changes
?? **API Contract Changed**
- Parameter `tanggalMasuk` diganti menjadi `periodeRencana`
- Format input berubah dari `DateTime` ke `string` (yyyy-MM)
- Client/Frontend perlu update request mereka

## Migration Guide untuk Frontend
```javascript
// BEFORE
const params = {
    tanggalMasuk: '2026-02-02T00:00:00.000Z'
};

// AFTER
const params = {
    periodeRencana: '2026-02'  // Format: yyyy-MM
};
```

## Timeline Perbaikan
- **v1**: Fix format parameter dari DateTime ke yyyy-MM ?
- **v2**: Fix logika periode - hanya ayam sebelum periode rencana yang dianggap sisa ?

# ?? Quick Reference: Harga Per Ekor Migration

> **Breaking Change**: Sistem harga pasar berubah dari per-kilogram ke per-ekor

## ?? TL;DR (Too Long; Didn't Read)

| Aspek | Before | After |
|-------|--------|-------|
| **Harga Field** | `HargaPerKg` | `HargaPerEkor` |
| **Range Value** | Rp 1 - 1.000.000 | Rp 10.000 - 100.000 |
| **Calculation** | `Berat × Harga/kg` | `Ekor × Harga/ekor` |
| **Input Form** | 3 fields (Jumlah, Berat, Tanggal) | 2 fields (Jumlah, Tanggal) |
| **HPP** | Per kilogram | Per ekor |

---

## ?? Backend Quick Tasks

### 1?? Update Models & DTOs
```diff
- public decimal HargaPerKg { get; set; }
+ public decimal HargaPerEkor { get; set; }

- public decimal BeratRataRata { get; set; }  // REMOVE from DTO
- public decimal TotalBerat { get; set; }     // REMOVE from DTO
```

### 2?? Update Service Logic
```csharp
// OLD
var pendapatan = (jumlahEkor * beratRataRata) * hargaPerKg;

// NEW
var pendapatan = jumlahEkor * hargaPerEkor;
```

### 3?? Update Method Signature
```csharp
// OLD
Task<EstimasiKeuntunganDto> HitungKeuntunganAsync(int totalAyam, decimal beratRataRata, DateTime tanggal);

// NEW
Task<EstimasiKeuntunganDto> HitungKeuntunganAsync(int totalAyam, DateTime tanggal);
```

### 4?? Run Migration
```bash
dotnet ef migrations add ChangeHargaFromKgToEkor
dotnet ef database update
```

---

## ?? Frontend Quick Tasks

### 1?? Update API Calls
```javascript
// OLD
const params = { total_ayam, berat_rata_rata, tanggal };

// NEW
const params = { total_ayam, tanggal };  // Remove berat_rata_rata
```

### 2?? Update Response Mapping
```javascript
// OLD
const harga = response.data.harga_per_kg;
const totalBerat = response.data.total_berat;

// NEW
const harga = response.data.harga_per_ekor;
// No more total_berat
```

### 3?? Update Forms
```html
<!-- OLD -->
<input name="harga_per_kg" min="1" max="1000000" />
<input name="berat_rata_rata" step="0.01" /> <!-- REMOVE -->

<!-- NEW -->
<input name="harga_per_ekor" min="10000" max="100000" />
```

### 4?? Update Labels
```diff
- Harga Per Kg (Rp/kg)
+ Harga Per Ekor (Rp/ekor)

- Berat Rata-Rata (kg)        // REMOVE
- Total Berat (kg)             // REMOVE
```

---

## ?? API Changes Summary

| Endpoint | Method | Changed Parameters | Changed Response |
|----------|--------|-------------------|------------------|
| `/api/harga-pasar` | GET | - | `harga_per_kg` ? `harga_per_ekor` |
| `/api/harga-pasar` | POST | `harga_per_kg` ? `harga_per_ekor` | - |
| `/api/harga-pasar/estimasi-keuntungan` | GET | ? Remove `berat_rata_rata` | ? Remove `total_berat`, `berat_rata_rata` |
| `/api/panen/{id}/analisis-keuntungan` | GET | - | ? Remove `berat_rata_rata`, `total_berat_kg` |

---

## ?? Quick Tests

### Test 1: Create Harga Pasar
```http
POST /api/harga-pasar
{
  "harga_per_ekor": 42000,
  "tanggal_mulai": "2025-01-20"
}
```

### Test 2: Estimasi Keuntungan
```http
GET /api/harga-pasar/estimasi-keuntungan?total_ayam=100&tanggal_referensi=2025-01-20
```

Expected:
```json
{
  "total_ayam": 100,
  "harga_per_ekor": 42000,
  "total_pendapatan": 4200000
}
```

### Test 3: Analisis Keuntungan
```http
GET /api/panen/{id}/analisis-keuntungan
```

Expected:
```json
{
  "jumlah_ayam": 50,
  "harga_pasar_per_ekor": 42000,
  "pendapatan_kotor": 2100000,
  "harga_pokok_produksi": 30000
}
```

---

## ?? Common Issues & Solutions

### Issue 1: "Field 'berat_rata_rata' is required"
**Solution**: Remove parameter from API call

### Issue 2: "Property 'HargaPerKg' not found"
**Solution**: Update field name to `HargaPerEkor`

### Issue 3: "Pendapatan turun drastis"
**Solution**: Ini BUKAN bug! Cara perhitungan berubah. Update formula.

### Issue 4: "Validation failed: Harga must be between 1 and 1000000"
**Solution**: Update validation range to 10000-100000

---

## ?? Full Documentation

- **Frontend Guide**: [FrontendGuide_HargaPerEkor.md](./FrontendGuide_HargaPerEkor.md)
- **Backend Guide**: [BackendImplementation_HargaPerEkor.md](./BackendImplementation_HargaPerEkor.md)

---

## ? Checklist

### Backend
- [ ] Update `HargaPasar` model
- [ ] Update DTOs (Create, Update, Response, Estimasi, Analisis)
- [ ] Update `HargaPasarService.cs`
- [ ] Update `PanenService.cs`
- [ ] Update interface `IHargaPasarService`
- [ ] Update `ApplicationDbContext.cs`
- [ ] Create & run migration
- [ ] Test all endpoints

### Frontend
- [ ] Update form "Create Harga Pasar"
- [ ] Update form "Estimasi Keuntungan"
- [ ] Update table columns
- [ ] Update detail views
- [ ] Update API service/repository
- [ ] Update validations
- [ ] Update labels & translations
- [ ] Test all pages

---

**Last Updated**: 2025-01-20  
**Version**: 1.0.0  
**Status**: ? Ready for Implementation

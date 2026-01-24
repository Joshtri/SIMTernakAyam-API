# ?? Backend Implementation Guide: Harga Per Ekor

> **?? BREAKING CHANGE**: Perubahan sistem harga dari per kilogram ke per ekor

## ?? Table of Contents
- [Overview](#overview)
- [Files to Modify](#files-to-modify)
- [Step-by-Step Implementation](#step-by-step-implementation)
- [Database Migration](#database-migration)
- [Testing](#testing)

---

## ?? Overview

### Perubahan Konsep
| Aspek | Sebelum | Sesudah |
|-------|---------|---------|
| **Model Field** | `HargaPerKg` | `HargaPerEkor` |
| **Perhitungan** | `TotalBerat × HargaPerKg` | `JumlahEkor × HargaPerEkor` |
| **Input Required** | Jumlah + Berat Rata-Rata | Jumlah Ekor saja |
| **HPP** | Per kilogram | Per ekor |

---

## ?? Files to Modify

### **1. Models**
```
SIMTernakAyam/Models/HargaPasar.cs
```

### **2. DTOs**
```
SIMTernakAyam/DTOs/HargaPasar/CreateHargaPasarDto.cs
SIMTernakAyam/DTOs/HargaPasar/UpdateHargaPasarDto.cs
SIMTernakAyam/DTOs/HargaPasar/HargaPasarResponseDto.cs
SIMTernakAyam/DTOs/HargaPasar/EstimasiKeuntunganDto.cs
SIMTernakAyam/DTOs/Panen/AnalisisKeuntunganDto.cs
```

### **3. Services**
```
SIMTernakAyam/Services/HargaPasarService.cs
SIMTernakAyam/Services/PanenService.cs
SIMTernakAyam/Services/Interfaces/IHargaPasarService.cs
```

### **4. Database**
```
SIMTernakAyam/Data/ApplicationDbContext.cs
```

---

## ?? Step-by-Step Implementation

### **Step 1: Update Model `HargaPasar.cs`**

**File:** `SIMTernakAyam/Models/HargaPasar.cs`

```csharp
namespace SIMTernakAyam.Models
{
    public class HargaPasar : BaseModel
    {
        /// <summary>
        /// Harga per ekor ayam hidup (Rp/ekor)
        /// </summary>
        public decimal HargaPerEkor { get; set; }  // ? CHANGED from HargaPerKg
        
        public DateTime TanggalMulai { get; set; }
        public DateTime? TanggalBerakhir { get; set; }
        public string? Keterangan { get; set; }
        public bool IsAktif { get; set; } = true;
        public string? Wilayah { get; set; }
    }
}
```

---

### **Step 2: Update DTOs**

#### **2.1 CreateHargaPasarDto.cs**

```csharp
using System.ComponentModel.DataAnnotations;

namespace SIMTernakAyam.DTOs.HargaPasar
{
    public class CreateHargaPasarDto
    {
        [Required(ErrorMessage = "Harga per ekor harus diisi")]
        [Range(10000, 100000, ErrorMessage = "Harga per ekor harus antara Rp 10.000 sampai Rp 100.000")]
        public decimal HargaPerEkor { get; set; }  // ? CHANGED

        [Required(ErrorMessage = "Tanggal mulai harus diisi")]
        public DateTime TanggalMulai { get; set; }

        public DateTime? TanggalBerakhir { get; set; }

        [MaxLength(500, ErrorMessage = "Keterangan maksimal 500 karakter")]
        public string? Keterangan { get; set; }

        [MaxLength(100, ErrorMessage = "Wilayah maksimal 100 karakter")]
        public string? Wilayah { get; set; }
    }
}
```

#### **2.2 UpdateHargaPasarDto.cs**

```csharp
using System.ComponentModel.DataAnnotations;

namespace SIMTernakAyam.DTOs.HargaPasar
{
    public class UpdateHargaPasarDto
    {
        [Required(ErrorMessage = "Harga per ekor harus diisi")]
        [Range(10000, 100000, ErrorMessage = "Harga per ekor harus antara Rp 10.000 sampai Rp 100.000")]
        public decimal HargaPerEkor { get; set; }  // ? CHANGED

        [Required(ErrorMessage = "Tanggal mulai harus diisi")]
        public DateTime TanggalMulai { get; set; }

        public DateTime? TanggalBerakhir { get; set; }

        [MaxLength(500, ErrorMessage = "Keterangan maksimal 500 karakter")]
        public string? Keterangan { get; set; }

        [MaxLength(100, ErrorMessage = "Wilayah maksimal 100 karakter")]
        public string? Wilayah { get; set; }
    }
}
```

#### **2.3 HargaPasarResponseDto.cs**

```csharp
namespace SIMTernakAyam.DTOs.HargaPasar
{
    public class HargaPasarResponseDto
    {
        public Guid Id { get; set; }
        public decimal HargaPerEkor { get; set; }  // ? CHANGED
        public DateTime TanggalMulai { get; set; }
        public DateTime? TanggalBerakhir { get; set; }
        public string? Keterangan { get; set; }
        public bool IsAktif { get; set; }
        public string? Wilayah { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdateAt { get; set; }
    }
}
```

#### **2.4 EstimasiKeuntunganDto.cs**

```csharp
namespace SIMTernakAyam.DTOs.HargaPasar
{
    public class EstimasiKeuntunganDto
    {
        public int TotalAyam { get; set; }
        
        // ? REMOVED: public decimal BeratRataRata { get; set; }
        // ? REMOVED: public decimal TotalBerat { get; set; }
        
        public decimal HargaPerEkor { get; set; }  // ? CHANGED from HargaPerKg
        public decimal TotalPendapatan { get; set; }
        public DateTime TanggalReferensi { get; set; }
        public HargaPasarInfoDto HargaPasarInfo { get; set; } = null!;
    }

    public class HargaPasarInfoDto
    {
        public Guid Id { get; set; }
        public decimal HargaPerEkor { get; set; }  // ? CHANGED
        public DateTime TanggalMulai { get; set; }
        public DateTime? TanggalBerakhir { get; set; }
        public string? Wilayah { get; set; }
        public string? Keterangan { get; set; }
    }
}
```

#### **2.5 AnalisisKeuntunganDto.cs**

```csharp
namespace SIMTernakAyam.DTOs.Panen
{
    public class AnalisisKeuntunganDto
    {
        public Guid PanenId { get; set; }
        public DateTime TanggalPanen { get; set; }
        public int JumlahAyam { get; set; }
        
        // ? REMOVED: public decimal BeratRataRata { get; set; }
        // ? REMOVED: public decimal TotalBeratKg { get; set; }
        
        public decimal HargaPasarPerEkor { get; set; }  // ? CHANGED from HargaPasarPerKg
        public DateTime TanggalHargaPasar { get; set; }
        public string? WilayahHarga { get; set; }
        
        public decimal PendapatanKotor { get; set; }
        public decimal TotalBiayaOperasional { get; set; }
        public decimal KeuntunganBersih { get; set; }
        public decimal MarginKeuntungan { get; set; }
        
        public decimal BiayaPakan { get; set; }
        public decimal BiayaVaksin { get; set; }
        public decimal BiayaLainnya { get; set; }
        
        public string StatusKeuntungan { get; set; } = string.Empty;
        public decimal ROI { get; set; }
        public decimal HargaPokokProduksi { get; set; }  // ? Now per ekor (not per kg)
        
        public string PendapatanKotorFormatted => $"Rp {PendapatanKotor:N0}";
        public string TotalBiayaFormatted => $"Rp {TotalBiayaOperasional:N0}";
        public string KeuntunganBersihFormatted => $"Rp {KeuntunganBersih:N0}";
        public string MarginKeuntunganFormatted => $"{MarginKeuntungan:F2}%";
        public string ROIFormatted => $"{ROI:F2}%";
        public string HargaPokokProduksiFormatted => $"Rp {HargaPokokProduksi:N0}";
    }
}
```

---

### **Step 3: Update Services**

#### **3.1 IHargaPasarService.cs (Interface)**

```csharp
namespace SIMTernakAyam.Services.Interfaces
{
    public interface IHargaPasarService
    {
        // ... existing methods ...
        
        // ? UPDATED: Remove beratRataRata parameter
        Task<(bool Success, string Message, EstimasiKeuntunganDto? Data)> HitungKeuntunganAsync(
            int totalAyam,
            DateTime tanggalReferensi);  // ? REMOVED: decimal beratRataRata
    }
}
```

#### **3.2 HargaPasarService.cs**

**Method: HitungKeuntunganAsync**

```csharp
public async Task<(bool Success, string Message, EstimasiKeuntunganDto? Data)> HitungKeuntunganAsync(
    int totalAyam,
    DateTime tanggalReferensi)  // ? REMOVED: decimal beratRataRata
{
    try
    {
        var hargaPasar = await _hargaPasarRepository.GetHargaAktifByTanggalAsync(tanggalReferensi);
        if (hargaPasar == null)
        {
            return (false, $"Tidak ada harga pasar yang aktif pada tanggal {tanggalReferensi:dd/MM/yyyy}", null);
        }

        // ? NEW CALCULATION: Direct multiplication
        var totalPendapatan = totalAyam * hargaPasar.HargaPerEkor;

        var estimasi = new EstimasiKeuntunganDto
        {
            TotalAyam = totalAyam,
            HargaPerEkor = hargaPasar.HargaPerEkor,  // ? CHANGED
            TotalPendapatan = totalPendapatan,
            TanggalReferensi = tanggalReferensi,
            HargaPasarInfo = new HargaPasarInfoDto
            {
                Id = hargaPasar.Id,
                HargaPerEkor = hargaPasar.HargaPerEkor,  // ? CHANGED
                TanggalMulai = hargaPasar.TanggalMulai,
                TanggalBerakhir = hargaPasar.TanggalBerakhir,
                Wilayah = hargaPasar.Wilayah,
                Keterangan = hargaPasar.Keterangan
            }
        };

        return (true, "Estimasi keuntungan berhasil dihitung", estimasi);
    }
    catch (Exception ex)
    {
        return (false, $"Error: {ex.Message}", null);
    }
}
```

**Method: HitungKeuntunganDariPanenAsync**

```csharp
public async Task<(bool Success, string Message, KeuntunganPanenDto? Data)> HitungKeuntunganDariPanenAsync(Guid panenId)
{
    try
    {
        var panen = await _panenRepository.GetWithDetailsAsync(panenId);
        if (panen == null)
        {
            return (false, "Data panen tidak ditemukan", null);
        }

        var hargaPasar = await _hargaPasarRepository.GetHargaAktifByTanggalAsync(panen.TanggalPanen);
        if (hargaPasar == null)
        {
            return (false, $"Tidak ada harga pasar yang aktif pada tanggal panen {panen.TanggalPanen:dd/MM/yyyy}", null);
        }

        // ? NEW CALCULATION
        var totalPendapatan = panen.JumlahEkorPanen * hargaPasar.HargaPerEkor;

        var keuntunganPanen = new KeuntunganPanenDto
        {
            PanenId = panen.Id,
            TanggalPanen = panen.TanggalPanen,
            JumlahAyam = panen.JumlahEkorPanen,
            HargaPerEkor = hargaPasar.HargaPerEkor,  // ? CHANGED
            TotalPendapatan = totalPendapatan,
            NamaKandang = panen.Ayam?.Kandang?.NamaKandang ?? "N/A",
            HargaPasarInfo = new HargaPasarInfoDto
            {
                Id = hargaPasar.Id,
                HargaPerEkor = hargaPasar.HargaPerEkor,  // ? CHANGED
                TanggalMulai = hargaPasar.TanggalMulai,
                TanggalBerakhir = hargaPasar.TanggalBerakhir,
                Wilayah = hargaPasar.Wilayah,
                Keterangan = hargaPasar.Keterangan
            }
        };

        return (true, "Keuntungan panen berhasil dihitung", keuntunganPanen);
    }
    catch (Exception ex)
    {
        return (false, $"Error: {ex.Message}", null);
    }
}
```

#### **3.3 PanenService.cs**

**Method: GetAnalisisKeuntunganAsync**

```csharp
public async Task<AnalisisKeuntunganDto?> GetAnalisisKeuntunganAsync(Guid panenId)
{
    var panen = await _panenRepository.GetWithDetailsAsync(panenId);
    if (panen == null) return null;

    var hargaPasar = await _hargaPasarService.GetHargaAktifByTanggalAsync(panen.TanggalPanen);
    if (hargaPasar == null)
    {
        hargaPasar = await _hargaPasarService.GetHargaTerbaruAsync();
        if (hargaPasar == null) return null;
    }

    // ? NEW CALCULATION: Direct per-ekor calculation
    var pendapatanKotor = panen.JumlahEkorPanen * hargaPasar.HargaPerEkor;

    var ayam = panen.Ayam;
    var totalBiaya = await HitungTotalBiayaOperasionalAsync(
        ayam.Id, 
        ayam.TanggalMasuk, 
        panen.TanggalPanen, 
        panen.JumlahEkorPanen, 
        ayam.JumlahMasuk);

    var keuntunganBersih = pendapatanKotor - totalBiaya.Total;
    var marginKeuntungan = pendapatanKotor > 0 ? (keuntunganBersih / pendapatanKotor) * 100 : 0;
    var roi = totalBiaya.Total > 0 ? (keuntunganBersih / totalBiaya.Total) * 100 : 0;
    
    // ? NEW: HPP per ekor (not per kg)
    var hargaPokokProduksi = panen.JumlahEkorPanen > 0 ? totalBiaya.Total / panen.JumlahEkorPanen : 0;

    var statusKeuntungan = keuntunganBersih > 0 ? "Untung" : keuntunganBersih < 0 ? "Rugi" : "Impas";

    return new AnalisisKeuntunganDto
    {
        PanenId = panen.Id,
        TanggalPanen = panen.TanggalPanen,
        JumlahAyam = panen.JumlahEkorPanen,
        HargaPasarPerEkor = hargaPasar.HargaPerEkor,  // ? CHANGED
        TanggalHargaPasar = hargaPasar.TanggalMulai,
        WilayahHarga = hargaPasar.Wilayah,
        PendapatanKotor = pendapatanKotor,
        TotalBiayaOperasional = totalBiaya.Total,
        BiayaPakan = totalBiaya.BiayaPakan,
        BiayaVaksin = totalBiaya.BiayaVaksin,
        BiayaLainnya = totalBiaya.BiayaLainnya,
        KeuntunganBersih = keuntunganBersih,
        MarginKeuntungan = marginKeuntungan,
        ROI = roi,
        HargaPokokProduksi = hargaPokokProduksi,  // Now per ekor
        StatusKeuntungan = statusKeuntungan
    };
}
```

---

### **Step 4: Update Database Context**

**File:** `SIMTernakAyam/Data/ApplicationDbContext.cs`

```csharp
// Konfigurasi untuk HargaPasar
modelBuilder.Entity<HargaPasar>()
    .Property(h => h.HargaPerEkor)  // ? CHANGED from HargaPerKg
    .HasPrecision(18, 2)
    .IsRequired();

// ... rest of configuration remains the same ...
```

---

## ??? Database Migration

### **Step 1: Create Migration**

```bash
cd SIMTernakAyam
dotnet ef migrations add ChangeHargaFromKgToEkor
```

**Migration File Preview:**
```csharp
public partial class ChangeHargaFromKgToEkor : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Rename column
        migrationBuilder.RenameColumn(
            name: "HargaPerKg",
            table: "HargaPasar",
            newName: "HargaPerEkor");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.RenameColumn(
            name: "HargaPerEkor",
            table: "HargaPasar",
            newName: "HargaPerKg");
    }
}
```

### **Step 2: Update Database**

```bash
dotnet ef database update
```

Or use the batch file:
```bash
update_db.bat
```

### **Step 3: Verify Migration**

```sql
-- Check column name
SELECT COLUMN_NAME, DATA_TYPE 
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'HargaPasar';

-- Expected result: HargaPerEkor (not HargaPerKg)
```

---

## ?? Testing

### **Unit Tests**

Create test file: `SIMTernakAyam.Tests/Services/HargaPasarServiceTests.cs`

```csharp
[Fact]
public async Task HitungKeuntunganAsync_ShouldCalculateCorrectly()
{
    // Arrange
    var totalAyam = 100;
    var hargaPerEkor = 42000m;
    var tanggalReferensi = DateTime.UtcNow;
    
    // Mock harga pasar
    var hargaPasar = new HargaPasar 
    { 
        HargaPerEkor = hargaPerEkor,
        IsAktif = true 
    };
    
    _mockHargaPasarRepo
        .Setup(x => x.GetHargaAktifByTanggalAsync(tanggalReferensi))
        .ReturnsAsync(hargaPasar);
    
    // Act
    var result = await _service.HitungKeuntunganAsync(totalAyam, tanggalReferensi);
    
    // Assert
    Assert.True(result.Success);
    Assert.Equal(4200000m, result.Data.TotalPendapatan);  // 100 × 42000
    Assert.Equal(hargaPerEkor, result.Data.HargaPerEkor);
}
```

### **Integration Tests**

Create test file: `SIMTernakAyam/Tests/HargaPasarPerEkorTests.http`

```http
### Test 1: Create Harga Pasar Baru
POST {{baseUrl}}/api/harga-pasar
Content-Type: application/json
Authorization: Bearer {{token}}

{
  "harga_per_ekor": 42000,
  "tanggal_mulai": "2025-01-20T00:00:00Z",
  "wilayah": "Kupang",
  "keterangan": "Harga pasar per ekor - testing"
}

### Test 2: Get Estimasi Keuntungan (Without BeratRataRata)
GET {{baseUrl}}/api/harga-pasar/estimasi-keuntungan?total_ayam=100&tanggal_referensi=2025-01-20
Authorization: Bearer {{token}}

### Expected Response:
# {
#   "success": true,
#   "data": {
#     "total_ayam": 100,
#     "harga_per_ekor": 42000,
#     "total_pendapatan": 4200000
#   }
# }

### Test 3: Get Analisis Keuntungan Panen
GET {{baseUrl}}/api/panen/{{panenId}}/analisis-keuntungan
Authorization: Bearer {{token}}

### Expected Response:
# {
#   "success": true,
#   "data": {
#     "jumlah_ayam": 50,
#     "harga_pasar_per_ekor": 42000,
#     "pendapatan_kotor": 2100000,
#     "harga_pokok_produksi": 30000
#   }
# }
```

---

## ? Validation Checklist

### **Pre-Migration**
- [ ] Backup database
- [ ] Review all changes in DTOs
- [ ] Update all service methods
- [ ] Update interface signatures
- [ ] Test compilation (`dotnet build`)

### **Migration**
- [ ] Create migration: `dotnet ef migrations add ...`
- [ ] Review migration file
- [ ] Apply migration: `dotnet ef database update`
- [ ] Verify database schema change

### **Post-Migration**
- [ ] Run unit tests
- [ ] Run integration tests (`.http` files)
- [ ] Test all endpoints via Postman
- [ ] Verify API responses match new structure
- [ ] Check error handling

### **Documentation**
- [ ] Update API documentation (Swagger)
- [ ] Update frontend guide
- [ ] Update README
- [ ] Create migration notes

---

## ?? Rollback Plan

If something goes wrong:

### **Step 1: Rollback Database**
```bash
dotnet ef database update [PreviousMigrationName]
```

### **Step 2: Remove Migration**
```bash
dotnet ef migrations remove
```

### **Step 3: Restore Code**
```bash
git checkout HEAD -- .
```

---

## ?? Performance Considerations

### **Before (Per Kg)**
```
Calculation: BeratRataRata × JumlahEkor × HargaPerKg
Operations: 2 multiplications
```

### **After (Per Ekor)**
```
Calculation: JumlahEkor × HargaPerEkor
Operations: 1 multiplication
```

? **Result**: 50% reduction in arithmetic operations

---

## ?? Support & Questions

- **Technical Issues**: Open GitHub issue with label `backend-migration`
- **Code Review**: Tag `@backend-team` in PR
- **Urgent**: Contact Tech Lead

---

**Last Updated**: 2025-01-20  
**Author**: Backend Team  
**Version**: 1.0.0  
**Status**: ? Ready for Implementation

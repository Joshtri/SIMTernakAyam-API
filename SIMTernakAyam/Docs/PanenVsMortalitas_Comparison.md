# ?? Comparison: Panen vs Mortalitas - Manual Split Implementation

## Overview

Kedua fitur (Panen dan Mortalitas) sekarang memiliki **Manual Split** yang memungkinkan user menentukan distribusi antara ayam lama dan ayam baru.

---

## ?? Similarities (Persamaan)

| Feature | Panen | Mortalitas |
|---------|-------|-----------|
| **Manual Split Mode** | ? Available | ? Available |
| **Auto FIFO Mode** | ? Available | ? Deprecated (return error) |
| **Default Mode** | `manual-split` | `manual-split` |
| **API Structure** | Same pattern | Same pattern |
| **Validation Logic** | Same rules | Same rules |
| **Distribution Algorithm** | FIFO within group | FIFO within group |
| **Multiple Records Created** | ? Yes | ? Yes |

---

## ? Differences (Perbedaan)

### 1. **Auto FIFO Availability**

#### Panen
```http
POST /api/panens
{
  "mode": "auto-fifo",  // ? Still works
  ...
}
```
- ? Auto FIFO **masih berfungsi**
- Cocok untuk harvest pattern (oldest first)

#### Mortalitas
```http
POST /api/mortalitas
{
  "mode": "auto-fifo",  // ? Returns error
  ...
}
```
- ? Auto FIFO **deprecated**
- Mortalitas tidak predictable (bisa baru/lama yang mati)

---

### 2. **Additional Features**

| Feature | Panen | Mortalitas |
|---------|-------|-----------|
| **Foto Upload** | ? Not needed | ? Supported (`fotoMortalitas`) |
| **Analisis Keuntungan** | ? Available | ? Not available |
| **Notification** | ? No auto-notification | ? Auto-notification (HIGH priority) |

---

### 3. **DTO Comparison**

#### Panen
```csharp
public class CreatePanenWithModeDto
{
    public Guid KandangId { get; set; }
    public DateTime TanggalPanen { get; set; }
    public int JumlahEkorPanen { get; set; }
    public decimal BeratRataRata { get; set; }  // ? Required
    public string Mode { get; set; }
    public int? JumlahDariAyamLama { get; set; }
    public int? JumlahDariAyamBaru { get; set; }
}
```

#### Mortalitas
```csharp
public class CreateMortalitasDto
{
    public Guid KandangId { get; set; }
    public DateTime TanggalKematian { get; set; }
    public int JumlahKematian { get; set; }
    public string PenyebabKematian { get; set; }  // ? Required
    public string Mode { get; set; }
    public int? JumlahDariAyamLama { get; set; }
    public int? JumlahDariAyamBaru { get; set; }
    public string? FotoMortalitasBase64 { get; set; }  // ? Optional
    public string? FotoMortalitasFileName { get; set; }
}
```

---

## ?? API Endpoints Comparison

### Panen

```http
# Main endpoint with mode
POST /api/panens
{
  "mode": "manual-split",
  "jumlahDariAyamLama": 30,
  "jumlahDariAyamBaru": 20
}

# Auto FIFO (still available)
POST /api/panens/auto-fifo
{
  "jumlahEkorPanen": 50,
  "beratRataRata": 1.5
}

# Single record (original)
POST /api/panens/single
{
  "ayamId": "guid",
  "jumlahEkorPanen": 50
}
```

### Mortalitas

```http
# Main endpoint with mode
POST /api/mortalitas
{
  "mode": "manual-split",
  "jumlahDariAyamLama": 6,
  "jumlahDariAyamBaru": 4
}

# Auto FIFO (deprecated - returns error)
POST /api/mortalitas/auto-fifo
{
  "jumlahKematian": 10
}
# Response: 400 Error - "Method ini tidak menggunakan Auto FIFO lagi..."
```

---

## ?? Frontend Implementation Differences

### Panen Form
```tsx
<form>
  {/* Basic fields */}
  <Input label="Total Ekor" />
  <Input label="Berat Rata-rata (kg)" />  {/* ? Panen only */}
  
  {/* Mode selection */}
  <RadioGroup>
    <Radio value="auto-fifo">Auto FIFO</Radio>  {/* ? Available */}
    <Radio value="manual-split">Manual Split</Radio>
  </RadioGroup>
  
  {/* Manual split section */}
  {mode === 'manual-split' && (
    <div>
      <Slider label="Dari Ayam Lama" />
      <Display label="Dari Ayam Baru" />
    </div>
  )}
</form>
```

### Mortalitas Form
```tsx
<form>
  {/* Basic fields */}
  <Input label="Jumlah Kematian" />
  <Input label="Penyebab Kematian" />  {/* ? Mortalitas only */}
  <FileUpload label="Foto Bukti" />    {/* ? Mortalitas only */}
  
  {/* Mode selection */}
  <RadioGroup>
    <Radio value="auto-fifo" disabled>  {/* ? Deprecated */}
      Auto FIFO (Deprecated)
    </Radio>
    <Radio value="manual-split">Manual Split</Radio>
  </RadioGroup>
  
  {/* Manual split section (same as Panen) */}
  {mode === 'manual-split' && (
    <div>
      <Slider label="Dari Ayam Lama" />
      <Display label="Dari Ayam Baru" />
    </div>
  )}
</form>
```

---

## ?? Use Case Recommendations

### When to use Auto FIFO (Panen only)

? **Good for:**
- Standard harvest operations
- When following FIFO principle (oldest first)
- Quick data entry without detailed breakdown

? **Not good for:**
- Need to track specific ayam groups
- Complex distribution requirements

### When to use Manual Split (Both)

? **Good for:**
- Need accurate ayam group tracking
- Specific business requirements
- Data analysis by ayam periode
- Mortality tracking (required for Mortalitas)

? **Not good for:**
- Quick batch operations
- When exact distribution doesn't matter

---

## ?? Code Reusability

### Shared Logic
```typescript
// ? Can be shared between Panen & Mortalitas

// Validation
export const validateManualSplit = (form: any) => {
  const total = (form.jumlahDariAyamLama || 0) + (form.jumlahDariAyamBaru || 0);
  return total === form.jumlahTotal; // jumlahEkorPanen or jumlahKematian
};

// Stock info fetching
export const useAyamInfo = (kandangId: string) => {
  // Same implementation for both
};

// Distribution slider component
export const DistributionSlider = ({ ... }) => {
  // Can be reused
};
```

### Specific Logic
```typescript
// ? Cannot be shared (different fields)

// Panen specific
interface PanenForm {
  beratRataRata: number;  // Only for Panen
  // ...
}

// Mortalitas specific
interface MortalitasForm {
  penyebabKematian: string;      // Only for Mortalitas
  fotoMortalitasBase64?: string; // Only for Mortalitas
  // ...
}
```

---

## ?? Documentation Files

### Panen
- `Docs/PanenManualSplitFeature.md` - Full feature documentation
- `Docs/FrontendGuide_PanenManualSplit.md` - Frontend implementation guide
- `Tests/PanenManualSplitTests.http` - API test cases

### Mortalitas
- `Docs/MortalitasManualSplitFeature.md` - Full feature documentation
- `Docs/MortalitasAutoFifoDeprecated.md` - Auto FIFO deprecation notice
- `Tests/MortalitasManualSplitTests.http` - API test cases
- `Tests/MortalitasNoFifoTests.http` - No-FIFO validation tests

---

## ?? Key Takeaways

1. **Manual Split** - Fitur yang sama untuk Panen dan Mortalitas
2. **Auto FIFO** - Hanya tersedia untuk Panen, deprecated untuk Mortalitas
3. **Frontend** - Bisa reuse banyak komponen antara Panen dan Mortalitas
4. **Validation** - Logic yang sama untuk kedua fitur
5. **Documentation** - Lengkap untuk kedua fitur

---

## ?? Migration Path

### Existing Mortalitas Users
```
? Old Way (Auto FIFO)
POST /api/mortalitas/auto-fifo
{
  "jumlahKematian": 10
}

? New Way (Manual Split)
POST /api/mortalitas
{
  "mode": "manual-split",
  "jumlahKematian": 10,
  "jumlahDariAyamLama": 6,
  "jumlahDariAyamBaru": 4
}
```

### Existing Panen Users
```
? Old Way (Still works)
POST /api/panens/auto-fifo
{
  "jumlahEkorPanen": 50
}

? New Way (Recommended)
POST /api/panens
{
  "mode": "manual-split",
  "jumlahEkorPanen": 50,
  "jumlahDariAyamLama": 30,
  "jumlahDariAyamBaru": 20
}
```

---

**Last Updated**: 2024-01-15

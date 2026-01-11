# Fitur Mortalitas dengan Mode Auto FIFO dan Manual Split

## ?? Problem Statement

**Masalah Awal:**
- Sistem hanya support **Auto FIFO** - sistem otomatis pilih ayam terlama yang mati
- Tidak fleksibel: dalam realita, **ayam baru bisa mati lebih banyak** dari ayam lama
- Petugas tidak bisa input sesuai kondisi di lapangan

**Solusi:**
- Tambah mode **Manual Split** - user bisa tentukan berapa dari ayam lama dan berapa dari ayam baru
- Tetap support mode **Auto FIFO** untuk kemudahan

---

## ?? Fitur Baru

### 1. **Mode Auto FIFO** (Existing - Improved)
? Sistem otomatis distribute kematian ke ayam-ayam

**Cara Kerja:**
1. User input total jumlah kematian
2. Sistem otomatis pilih ayam yang mati berdasarkan FIFO
3. Prioritas: **Ayam sisa/lama** dulu (IsAyamSisa=true), baru **ayam baru**

**Request Body:**
```json
{
  "kandangId": "guid",
  "tanggalKematian": "2026-01-15T10:00:00.000Z",
  "jumlahKematian": 50,
  "penyebabKematian": "Penyakit Flu Burung",
  "mode": "auto-fifo"
}
```

---

### 2. **Mode Manual Split** (NEW! ?)
? User tentukan berapa dari ayam lama dan berapa dari ayam baru

**Cara Kerja:**
1. User input total jumlah kematian
2. User input **berapa dari ayam lama** (`jumlahDariAyamLama`)
3. User input **berapa dari ayam baru** (`jumlahDariAyamBaru`)
4. Sistem validate: total harus sama dengan `jumlahKematian`

**Request Body:**
```json
{
  "kandangId": "guid",
  "tanggalKematian": "2026-01-20T10:00:00.000Z",
  "jumlahKematian": 50,
  "penyebabKematian": "Penyakit Newcastle",
  "mode": "manual-split",
  "jumlahDariAyamLama": 30,    // 30 ekor dari ayam lama
  "jumlahDariAyamBaru": 20      // 20 ekor dari ayam baru
}
```

---

## ?? DTO Changes

### CreateMortalitasDto (Updated)

```csharp
public class CreateMortalitasDto
{
    [Required]
    public Guid KandangId { get; set; }  // ?? CHANGED: dari AyamId ke KandangId

    [Required]
    public DateTime TanggalKematian { get; set; }

    [Required]
    [Range(1, 1000000)]
    public int JumlahKematian { get; set; }

    [Required]
    [StringLength(200)]
    public string PenyebabKematian { get; set; }

    // ? NEW PROPERTIES
    [Required]
    public string Mode { get; set; } = "auto-fifo";  // "auto-fifo" or "manual-split"

    [Range(0, 1000000)]
    public int? JumlahDariAyamLama { get; set; }  // Optional, required if mode = manual-split

    [Range(0, 1000000)]
    public int? JumlahDariAyamBaru { get; set; }  // Optional, required if mode = manual-split

    public string? FotoMortalitasBase64 { get; set; }
    public string? FotoMortalitasFileName { get; set; }
}
```

---

## ??? Backend Changes

### 1. **New Method in IAyamService**
```csharp
Task<List<(Ayam Ayam, int JumlahHidup)>> GetAyamByPeriodeTypeAsync(Guid kandangId, bool isAyamLama);
```

**Purpose:**
- Get ayam berdasarkan tipe periode:
  - `isAyamLama = true` ? Ayam lama/sisa (IsAyamSisa=true)
  - `isAyamLama = false` ? Ayam baru (IsAyamSisa=false)
- Return tuple dengan jumlah hidup untuk setiap ayam

---

### 2. **New Method in IMortalitasService**
```csharp
Task<(bool Success, string Message, List<Mortalitas>? Data)> CreateMortalitasManualSplitAsync(
    Guid kandangId,
    DateTime tanggalKematian,
    int jumlahDariAyamLama,
    int jumlahDariAyamBaru,
    string penyebabKematian,
    IFormFile? fotoMortalitas = null);
```

**Logic:**
1. Validate total: `jumlahDariAyamLama + jumlahDariAyamBaru` harus valid
2. Get ayam lama dengan `GetAyamByPeriodeTypeAsync(kandangId, true)`
3. Get ayam baru dengan `GetAyamByPeriodeTypeAsync(kandangId, false)`
4. Validate tidak melebihi jumlah yang ada
5. Distribute kematian ke ayam-ayam (FIFO dalam grup)
6. Create multiple Mortalitas records

---

### 3. **Updated Controller Method**
```csharp
[HttpPost]
public async Task<IActionResult> Create([FromBody] CreateMortalitasDto dto)
{
    // Validate mode
    if (dto.Mode != "auto-fifo" && dto.Mode != "manual-split")
        return Error("Mode harus 'auto-fifo' atau 'manual-split'.");

    // Validate manual-split requirements
    if (dto.Mode == "manual-split")
    {
        if (!dto.JumlahDariAyamLama.HasValue || !dto.JumlahDariAyamBaru.HasValue)
            return Error("Mode 'manual-split' memerlukan JumlahDariAyamLama dan JumlahDariAyamBaru.");

        var totalManual = dto.JumlahDariAyamLama.Value + dto.JumlahDariAyamBaru.Value;
        if (totalManual != dto.JumlahKematian)
            return Error($"Total {totalManual} harus sama dengan JumlahKematian {dto.JumlahKematian}.");
    }

    // Call appropriate service method based on mode
    if (dto.Mode == "auto-fifo")
        result = await _mortalitasService.CreateMortalitasAutoFifoAsync(...);
    else
        result = await _mortalitasService.CreateMortalitasManualSplitAsync(...);

    return Created(result);
}
```

---

## ?? Use Cases

### Use Case 1: Auto FIFO (Simple)
**Kondisi:**
- Kandang punya ayam lama dan baru
- Petugas tidak tahu detail mana yang mati
- Ingin cepat input saja

**Input:**
```json
{
  "mode": "auto-fifo",
  "jumlahKematian": 50
}
```

**Output:**
- Sistem otomatis distribute ke ayam terlama dulu

---

### Use Case 2: Manual Split (Detailed)
**Kondisi:**
- Kandang punya 200 ayam lama + 800 ayam baru
- Hari ini mati 100 ekor
- Dari pengamatan petugas:
  - 70 ekor dari ayam lama (sudah lemah)
  - 30 ekor dari ayam baru

**Input:**
```json
{
  "mode": "manual-split",
  "jumlahKematian": 100,
  "jumlahDariAyamLama": 70,
  "jumlahDariAyamBaru": 30
}
```

**Output:**
- Create multiple Mortalitas records:
  - X records untuk ayam lama (total 70 ekor)
  - Y records untuk ayam baru (total 30 ekor)

---

### Use Case 3: Semua dari Ayam Lama
**Kondisi:**
- Hanya ayam lama yang mati (ayam baru sehat)

**Input:**
```json
{
  "mode": "manual-split",
  "jumlahKematian": 40,
  "jumlahDariAyamLama": 40,
  "jumlahDariAyamBaru": 0
}
```

---

### Use Case 4: Semua dari Ayam Baru
**Kondisi:**
- Ayam baru terserang penyakit

**Input:**
```json
{
  "mode": "manual-split",
  "jumlahKematian": 60,
  "jumlahDariAyamLama": 0,
  "jumlahDariAyamBaru": 60
}
```

---

## ? Validation Rules

### Mode: "auto-fifo"
- ? `jumlahKematian` > 0
- ? Tidak perlu `jumlahDariAyamLama` dan `jumlahDariAyamBaru`

### Mode: "manual-split"
- ? `jumlahDariAyamLama` dan `jumlahDariAyamBaru` **wajib diisi**
- ? `jumlahDariAyamLama` >= 0
- ? `jumlahDariAyamBaru` >= 0
- ? `jumlahDariAyamLama + jumlahDariAyamBaru = jumlahKematian`
- ? `jumlahDariAyamLama` <= total ayam lama yang hidup
- ? `jumlahDariAyamBaru` <= total ayam baru yang hidup

---

## ?? Response Example

```json
{
  "success": true,
  "message": "Berhasil membuat 3 record mortalitas dengan Manual Split. Ayam lama: 70 ekor, Ayam baru: 30 ekor. Total: 100 ekor.",
  "data": [
    {
      "id": "guid1",
      "ayamId": "guid-ayam-lama-1",
      "jumlahKematian": 50,
      "tanggalKematian": "2026-01-20T10:00:00Z",
      ...
    },
    {
      "id": "guid2",
      "ayamId": "guid-ayam-lama-2",
      "jumlahKematian": 20,
      ...
    },
    {
      "id": "guid3",
      "ayamId": "guid-ayam-baru-1",
      "jumlahKematian": 30,
      ...
    }
  ]
}
```

---

## ?? Frontend Implementation Guide

### Form UI Mockup

```
???????????????????????????????????????????????
?  INPUT MORTALITAS                           ?
???????????????????????????????????????????????
?                                             ?
?  Kandang: [Dropdown: Pilih Kandang]        ?
?                                             ?
?  Tanggal Kematian: [Date Picker]           ?
?                                             ?
?  Penyebab Kematian: [Text Input]           ?
?                                             ?
?  Total Jumlah Kematian: [Number Input]     ?
?                                             ?
?  Mode Input:                                ?
?  ? Auto FIFO (Sistem otomatis pilih)       ?
?  ? Manual Split (Tentukan sendiri)         ?
?                                             ?
?  ????????????????????????????????????????? ?
?  ? DETAIL SPLIT (jika Manual Split)     ? ?
?  ????????????????????????????????????????? ?
?  ?                                       ? ?
?  ? Dari Ayam Lama: [Number Input]       ? ?
?  ? (Max: 200 ekor tersedia)             ? ?
?  ?                                       ? ?
?  ? Dari Ayam Baru: [Number Input]       ? ?
?  ? (Max: 800 ekor tersedia)             ? ?
?  ?                                       ? ?
?  ? Total: 50 ekor ?                     ? ?
?  ????????????????????????????????????????? ?
?                                             ?
?  Foto Bukti: [Upload Image] (Optional)     ?
?                                             ?
?  [Cancel]  [Submit]                         ?
???????????????????????????????????????????????
```

### JavaScript/TypeScript Example

```javascript
// State
const [mode, setMode] = useState('auto-fifo');
const [jumlahKematian, setJumlahKematian] = useState(0);
const [jumlahDariAyamLama, setJumlahDariAyamLama] = useState(0);
const [jumlahDariAyamBaru, setJumlahDariAyamBaru] = useState(0);

// Validation
const validateManualSplit = () => {
  if (mode === 'manual-split') {
    const total = jumlahDariAyamLama + jumlahDariAyamBaru;
    if (total !== jumlahKematian) {
      alert(`Total split (${total}) harus sama dengan jumlah kematian (${jumlahKematian})`);
      return false;
    }
  }
  return true;
};

// Submit
const handleSubmit = async () => {
  if (!validateManualSplit()) return;

  const payload = {
    kandangId: selectedKandangId,
    tanggalKematian: tanggalKematian,
    jumlahKematian: jumlahKematian,
    penyebabKematian: penyebabKematian,
    mode: mode
  };

  if (mode === 'manual-split') {
    payload.jumlahDariAyamLama = jumlahDariAyamLama;
    payload.jumlahDariAyamBaru = jumlahDariAyamBaru;
  }

  const response = await fetch('/api/mortalitas', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(payload)
  });

  // Handle response...
};
```

---

## ?? Testing

Test file: `SIMTernakAyam/Tests/MortalitasManualSplitTests.http`

**Test Cases:**
1. ? Auto FIFO - Happy path
2. ? Manual Split - Split 50-50
3. ? Manual Split - Semua dari ayam lama
4. ? Manual Split - Semua dari ayam baru
5. ? Error: Total tidak cocok
6. ? Error: Missing split values
7. ? Error: Negative values
8. ? Error: Melebihi ayam yang ada
9. ? Error: Invalid mode
10. ? Manual Split dengan foto

---

## ?? Deployment Checklist

- [x] Update DTO (CreateMortalitasDto)
- [x] Add method GetAyamByPeriodeTypeAsync in IAyamService
- [x] Add method CreateMortalitasManualSplitAsync in IMortalitasService
- [x] Update Controller validation logic
- [x] Add dependency injection (IAyamService)
- [x] Create test file (.http)
- [x] Create documentation
- [ ] Update API documentation (Swagger)
- [ ] Update frontend form
- [ ] Test with real data
- [ ] User training

---

## ?? References

- **Test File**: `SIMTernakAyam/Tests/MortalitasManualSplitTests.http`
- **Controller**: `SIMTernakAyam/Controllers/MortalitasController.cs`
- **Service**: `SIMTernakAyam/Services/MortalitasService.cs`
- **DTO**: `SIMTernakAyam/DTOs/Mortalitas/CreateMortalitasDto.cs`

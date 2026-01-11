# ?? Feature: Manual Split untuk Panen

## ?? Overview

Fitur **Manual Split** untuk Panen memungkinkan user menentukan sendiri distribusi panen antara ayam lama (sisa periode sebelumnya) dan ayam baru (periode baru). Ini memberikan fleksibilitas yang sama seperti yang ada di Mortalitas.

---

## ? New Features

### 1. **Manual Split Mode** (RECOMMENDED)
User menentukan sendiri berapa jumlah panen dari:
- **Ayam Lama** (IsAyamSisa = true)
- **Ayam Baru** (IsAyamSisa = false)

### 2. **Auto FIFO Mode** (Still Available)
Sistem otomatis distribute panen ke ayam-ayam dengan prioritas FIFO (oldest first).

---

## ?? API Changes

### ? New Endpoint: POST /api/panens (with mode support)

```http
POST /api/panens
Content-Type: application/json

{
  "kandangId": "guid",
  "tanggalPanen": "2024-01-15T10:00:00Z",
  "jumlahEkorPanen": 50,
  "beratRataRata": 1.5,
  "mode": "manual-split",
  "jumlahDariAyamLama": 30,
  "jumlahDariAyamBaru": 20
}
```

**Response:**
```json
{
  "status": "success",
  "message": "Berhasil membuat 2 record panen dengan Manual Split. Ayam lama: 30 ekor, Ayam baru: 20 ekor. Total: 50 ekor.",
  "data": [
    {
      "id": "guid",
      "ayamId": "guid-ayam-lama",
      "jumlahEkorPanen": 30,
      "beratRataRata": 1.5,
      "totalBeratKg": 45.0,
      ...
    },
    {
      "id": "guid",
      "ayamId": "guid-ayam-baru",
      "jumlahEkorPanen": 20,
      "beratRataRata": 1.5,
      "totalBeratKg": 30.0,
      ...
    }
  ]
}
```

### ?? Existing Endpoints (Still Available)

#### 1. Auto FIFO
```http
POST /api/panens/auto-fifo
Content-Type: application/json

{
  "kandangId": "guid",
  "tanggalPanen": "2024-01-15T10:00:00Z",
  "jumlahEkorPanen": 50,
  "beratRataRata": 1.5
}
```

#### 2. Single Record (Original)
```http
POST /api/panens/single
Content-Type: application/json

{
  "ayamId": "guid-pilih-manual",
  "tanggalPanen": "2024-01-15T10:00:00Z",
  "jumlahEkorPanen": 50,
  "beratRataRata": 1.5
}
```

---

## ?? DTO Structure

### CreatePanenWithModeDto (NEW)
```csharp
public class CreatePanenWithModeDto
{
    public Guid KandangId { get; set; }
    public DateTime TanggalPanen { get; set; }
    public int JumlahEkorPanen { get; set; }
    public decimal BeratRataRata { get; set; }
    
    // Mode: "auto-fifo" or "manual-split"
    public string Mode { get; set; } = "manual-split";
    
    // Required if Mode = "manual-split"
    public int? JumlahDariAyamLama { get; set; }
    public int? JumlahDariAyamBaru { get; set; }
}
```

---

## ??? Service Layer

### IPanenService (Updated)
```csharp
Task<(bool Success, string Message, List<Panen>? Data)> CreatePanenManualSplitAsync(
    Guid kandangId,
    DateTime tanggalPanen,
    int jumlahDariAyamLama,
    int jumlahDariAyamBaru,
    decimal beratRataRata);
```

### Implementation Logic
1. Validate inputs (total, negative check, berat rata-rata range)
2. Get ayam lama list (IsAyamSisa = true) ordered by TanggalMasuk
3. Get ayam baru list (IsAyamSisa = false) ordered by TanggalMasuk
4. Validate stock availability for both groups
5. Distribute panen within each group using FIFO
6. Create multiple Panen records
7. Return list of created records

---

## ?? Frontend Implementation Guide

### 1. **Form UI Design**

```tsx
interface PanenForm {
  kandangId: string;
  tanggalPanen: Date;
  jumlahEkorPanen: number;
  beratRataRata: number;
  mode: 'auto-fifo' | 'manual-split';
  jumlahDariAyamLama?: number;
  jumlahDariAyamBaru?: number;
}

const PanenCreateForm = () => {
  const [form, setForm] = useState<PanenForm>({
    mode: 'manual-split', // Default mode
    // ...other fields
  });

  // Calculate ayam lama & baru info
  const { ayamLamaInfo, ayamBaruInfo } = useAyamInfo(form.kandangId);

  return (
    <form>
      {/* Basic fields */}
      <Input label="Kandang" value={form.kandangId} />
      <DatePicker label="Tanggal Panen" value={form.tanggalPanen} />
      <Input label="Total Ekor" value={form.jumlahEkorPanen} />
      <Input label="Berat Rata-rata (kg)" value={form.beratRataRata} />

      {/* Mode selector */}
      <RadioGroup value={form.mode} onChange={setMode}>
        <Radio value="auto-fifo">Auto FIFO</Radio>
        <Radio value="manual-split">Manual Split (Recommended)</Radio>
      </RadioGroup>

      {/* Manual Split Fields */}
      {form.mode === 'manual-split' && (
        <div className="manual-split-section">
          <h3>Distribusi Panen</h3>
          
          {/* Ayam Lama */}
          <div className="ayam-group">
            <label>Dari Ayam Lama</label>
            <p className="info">
              Tersedia: {ayamLamaInfo.jumlahHidup} ekor 
              (Masuk: {ayamLamaInfo.tanggalMasuk})
            </p>
            <Slider 
              min={0} 
              max={Math.min(form.jumlahEkorPanen, ayamLamaInfo.jumlahHidup)}
              value={form.jumlahDariAyamLama}
              onChange={(val) => {
                setForm({
                  ...form,
                  jumlahDariAyamLama: val,
                  jumlahDariAyamBaru: form.jumlahEkorPanen - val
                });
              }}
            />
            <span>{form.jumlahDariAyamLama || 0} ekor</span>
          </div>

          {/* Ayam Baru */}
          <div className="ayam-group">
            <label>Dari Ayam Baru</label>
            <p className="info">
              Tersedia: {ayamBaruInfo.jumlahHidup} ekor 
              (Masuk: {ayamBaruInfo.tanggalMasuk})
            </p>
            <span>{form.jumlahDariAyamBaru || 0} ekor</span>
          </div>

          {/* Total validation */}
          <div className="total-check">
            <strong>Total: {(form.jumlahDariAyamLama || 0) + (form.jumlahDariAyamBaru || 0)} ekor</strong>
            {isValidTotal() ? (
              <span className="valid">? Sesuai</span>
            ) : (
              <span className="invalid">? Harus sama dengan total ekor panen</span>
            )}
          </div>
        </div>
      )}

      <Button type="submit" disabled={!isValidForm()}>
        Simpan Panen
      </Button>
    </form>
  );
};
```

### 2. **API Call Example**

```typescript
// services/panenService.ts

export const createPanen = async (data: PanenForm) => {
  const response = await fetch('/api/panens', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`
    },
    body: JSON.stringify({
      kandangId: data.kandangId,
      tanggalPanen: data.tanggalPanen.toISOString(),
      jumlahEkorPanen: data.jumlahEkorPanen,
      beratRataRata: data.beratRataRata,
      mode: data.mode,
      jumlahDariAyamLama: data.jumlahDariAyamLama,
      jumlahDariAyamBaru: data.jumlahDariAyamBaru
    })
  });

  if (!response.ok) {
    const error = await response.json();
    throw new Error(error.message);
  }

  return await response.json();
};
```

### 3. **Helper Hook: useAyamInfo**

```typescript
// hooks/useAyamInfo.ts

export const useAyamInfo = (kandangId: string) => {
  const [ayamLamaInfo, setAyamLamaInfo] = useState(null);
  const [ayamBaruInfo, setAyamBaruInfo] = useState(null);

  useEffect(() => {
    if (!kandangId) return;

    // Fetch ayam info dari backend
    fetchAyamByKandang(kandangId).then(ayamList => {
      // Filter ayam lama & baru
      const ayamLama = ayamList.filter(a => a.isAyamSisa);
      const ayamBaru = ayamList.filter(a => !a.isAyamSisa);

      setAyamLamaInfo({
        jumlahHidup: ayamLama.reduce((sum, a) => sum + a.jumlahHidup, 0),
        tanggalMasuk: ayamLama[0]?.tanggalMasuk || null
      });

      setAyamBaruInfo({
        jumlahHidup: ayamBaru.reduce((sum, a) => sum + a.jumlahHidup, 0),
        tanggalMasuk: ayamBaru[0]?.tanggalMasuk || null
      });
    });
  }, [kandangId]);

  return { ayamLamaInfo, ayamBaruInfo };
};
```

### 4. **Validation Logic**

```typescript
const validateForm = (form: PanenForm) => {
  const errors: string[] = [];

  // Basic validation
  if (!form.kandangId) errors.push('Kandang wajib dipilih');
  if (form.jumlahEkorPanen <= 0) errors.push('Jumlah ekor harus lebih dari 0');
  if (form.beratRataRata < 0.01 || form.beratRataRata > 100) {
    errors.push('Berat rata-rata harus antara 0.01 - 100 kg');
  }

  // Manual split validation
  if (form.mode === 'manual-split') {
    const totalSplit = (form.jumlahDariAyamLama || 0) + (form.jumlahDariAyamBaru || 0);
    
    if (totalSplit !== form.jumlahEkorPanen) {
      errors.push(
        `Total split (${totalSplit}) harus sama dengan total ekor (${form.jumlahEkorPanen})`
      );
    }

    if ((form.jumlahDariAyamLama || 0) < 0 || (form.jumlahDariAyamBaru || 0) < 0) {
      errors.push('Jumlah tidak boleh negatif');
    }
  }

  return errors;
};
```

### 5. **UI Component Example (React + Tailwind)**

```tsx
<div className="space-y-6">
  {/* Header */}
  <div className="bg-blue-50 p-4 rounded-lg">
    <h3 className="font-semibold text-blue-900">Distribusi Panen Manual</h3>
    <p className="text-sm text-blue-700">
      Tentukan berapa ekor yang akan dipanen dari ayam lama dan ayam baru
    </p>
  </div>

  {/* Ayam Lama Section */}
  <div className="border rounded-lg p-4">
    <div className="flex justify-between items-start mb-3">
      <div>
        <h4 className="font-medium">Ayam Lama (Periode Sisa)</h4>
        <p className="text-sm text-gray-600">
          Masuk: {formatDate(ayamLamaInfo.tanggalMasuk)}
        </p>
      </div>
      <div className="text-right">
        <p className="text-sm text-gray-600">Tersedia</p>
        <p className="text-2xl font-bold text-green-600">
          {ayamLamaInfo.jumlahHidup} ekor
        </p>
      </div>
    </div>

    <div className="space-y-2">
      <input
        type="range"
        min={0}
        max={Math.min(jumlahEkorPanen, ayamLamaInfo.jumlahHidup)}
        value={jumlahDariAyamLama}
        onChange={handleSliderChange}
        className="w-full h-2 bg-gray-200 rounded-lg cursor-pointer"
      />
      <div className="flex justify-between text-sm text-gray-600">
        <span>0 ekor</span>
        <span className="font-bold text-blue-600">{jumlahDariAyamLama} ekor</span>
        <span>{Math.min(jumlahEkorPanen, ayamLamaInfo.jumlahHidup)} ekor</span>
      </div>
    </div>
  </div>

  {/* Ayam Baru Section */}
  <div className="border rounded-lg p-4">
    <div className="flex justify-between items-start mb-3">
      <div>
        <h4 className="font-medium">Ayam Baru (Periode Baru)</h4>
        <p className="text-sm text-gray-600">
          Masuk: {formatDate(ayamBaruInfo.tanggalMasuk)}
        </p>
      </div>
      <div className="text-right">
        <p className="text-sm text-gray-600">Tersedia</p>
        <p className="text-2xl font-bold text-green-600">
          {ayamBaruInfo.jumlahHidup} ekor
        </p>
      </div>
    </div>

    <div className="bg-gray-50 p-3 rounded">
      <p className="text-sm text-gray-600">Jumlah otomatis</p>
      <p className="text-2xl font-bold">{jumlahDariAyamBaru} ekor</p>
    </div>
  </div>

  {/* Total Summary */}
  <div className="bg-gradient-to-r from-blue-50 to-green-50 p-4 rounded-lg border-2 border-blue-200">
    <div className="flex justify-between items-center">
      <span className="font-medium">Total Panen</span>
      <div className="text-right">
        <p className="text-3xl font-bold text-blue-900">
          {jumlahDariAyamLama + jumlahDariAyamBaru} ekor
        </p>
        <p className="text-sm text-gray-600">
          Target: {jumlahEkorPanen} ekor
        </p>
      </div>
    </div>
    
    {isValidTotal ? (
      <div className="mt-2 flex items-center text-green-600">
        <CheckIcon className="w-5 h-5 mr-2" />
        <span className="font-medium">Distribusi valid</span>
      </div>
    ) : (
      <div className="mt-2 flex items-center text-red-600">
        <XIcon className="w-5 h-5 mr-2" />
        <span className="font-medium">Total harus sama dengan target</span>
      </div>
    )}
  </div>
</div>
```

---

## ?? Testing Examples

### Test Case 1: Manual Split Success
```http
POST /api/panens
{
  "kandangId": "{{kandangId}}",
  "tanggalPanen": "2024-01-15T10:00:00Z",
  "jumlahEkorPanen": 50,
  "beratRataRata": 1.5,
  "mode": "manual-split",
  "jumlahDariAyamLama": 30,
  "jumlahDariAyamBaru": 20
}

# Expected: Success (201)
```

### Test Case 2: Total Mismatch (Should Fail)
```http
POST /api/panens
{
  "kandangId": "{{kandangId}}",
  "tanggalPanen": "2024-01-15T10:00:00Z",
  "jumlahEkorPanen": 50,
  "beratRataRata": 1.5,
  "mode": "manual-split",
  "jumlahDariAyamLama": 25,
  "jumlahDariAyamBaru": 20
}

# Expected: Error 400
# "Total JumlahDariAyamLama (25) + JumlahDariAyamBaru (20) = 45 harus sama dengan JumlahEkorPanen (50)."
```

### Test Case 3: Exceed Stock (Should Fail)
```http
POST /api/panens
{
  "kandangId": "{{kandangId}}",
  "tanggalPanen": "2024-01-15T10:00:00Z",
  "jumlahEkorPanen": 999999,
  "beratRataRata": 1.5,
  "mode": "manual-split",
  "jumlahDariAyamLama": 500000,
  "jumlahDariAyamBaru": 499999
}

# Expected: Error 400
# "Jumlah panen dari ayam lama (500000) melebihi total ayam lama yang hidup (xxx)."
```

---

## ?? Comparison: Mortalitas vs Panen

| Feature | Mortalitas | Panen |
|---------|-----------|-------|
| Auto FIFO | ? Deprecated (return error) | ? Still available |
| Manual Split | ? Implemented | ? Implemented |
| Default Mode | `manual-split` | `manual-split` |
| Foto Upload | ? Supported | ? Not needed |
| Use Case | Random (bisa baru/lama) | Prefer old first |

---

## ?? Best Practices

### For Backend Developer
1. ? Always validate total split equals total panen
2. ? Check stock availability before creating records
3. ? Use UTC for datetime
4. ? Return detailed error messages
5. ? Create atomic transactions (all or nothing)

### For Frontend Developer
1. ? Show available stock for both ayam lama & baru
2. ? Use slider for better UX
3. ? Auto-calculate remaining when one changes
4. ? Show real-time validation feedback
5. ? Display tanggal masuk for context
6. ? Disable submit if validation fails
7. ? Show loading state during API call
8. ? Handle and display API errors gracefully

---

## ?? Troubleshooting

### Error: "Tidak ada ayam lama/sisa di kandang ini"
**Solution:** Pastikan ada ayam dengan `IsAyamSisa = true` di kandang tersebut.

### Error: "Total harus sama dengan jumlah ekor panen"
**Solution:** Pastikan `jumlahDariAyamLama + jumlahDariAyamBaru = jumlahEkorPanen`

### Error: "Jumlah panen melebihi total ayam yang hidup"
**Solution:** Cek stok tersedia dan kurangi jumlah panen.

---

## ?? Additional Resources

- Mortalitas Manual Split Documentation: `Docs/MortalitasManualSplitFeature.md`
- API Tests: `Tests/PanenManualSplitTests.http`
- Similar Implementation: `Services/MortalitasService.cs` (method `CreateMortalitasManualSplitAsync`)

---

## ?? Summary

Fitur Manual Split untuk Panen memberikan fleksibilitas yang sama seperti Mortalitas, memungkinkan user menentukan sendiri distribusi panen antara ayam lama dan baru. Ini sangat berguna untuk manajemen stok yang lebih akurat dan sesuai dengan kondisi real di lapangan.

**Mode yang Recommended**: `manual-split` ?

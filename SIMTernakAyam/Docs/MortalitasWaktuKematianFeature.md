# ?? Feature: Waktu Kematian pada Mortalitas

## ?? Tujuan
Menambahkan field **Waktu Kematian** (`WaktuKematian`) pada data mortalitas untuk mencatat jam dan menit ketika kematian ayam terjadi.

---

## ? Perubahan yang Dilakukan

### 1. **Model Mortalitas**
File: `Models/Mortalitas.cs`

```csharp
public class Mortalitas : BaseModel
{
    public Guid AyamId { get; set; }
    public Ayam Ayam { get; set; }
    
    public DateTime TanggalKematian { get; set; }
    
    /// <summary>
    /// ? BARU: Waktu kematian (jam:menit)
    /// Format: HH:mm (contoh: 14:30)
    /// </summary>
    public TimeOnly WaktuKematian { get; set; }
    
    public int JumlahKematian { get; set; }
    public string PenyebabKematian { get; set; }
    public string? FotoMortalitas { get; set; }
}
```

**Catatan:**
- Menggunakan tipe `TimeOnly` (tersedia di .NET 6+)
- Format: HH:mm (contoh: 14:30, 08:15, 23:45)
- Field ini **wajib diisi** (required)

---

### 2. **DTOs (Data Transfer Objects)**

#### a. CreateMortalitasDto
File: `DTOs/Mortalitas/CreateMortalitasDto.cs`

```csharp
[Required(ErrorMessage = "Waktu kematian wajib diisi.")]
public TimeOnly WaktuKematian { get; set; }
```

#### b. UpdateMortalitasDto
File: `DTOs/Mortalitas/UpdateMortalitasDto.cs`

```csharp
[Required(ErrorMessage = "Waktu kematian wajib diisi.")]
public TimeOnly WaktuKematian { get; set; }
```

#### c. MortalitasResponseDto
File: `DTOs/Mortalitas/MortalitasResponseDto.cs`

```csharp
/// <summary>
/// Waktu kematian (jam:menit)
/// Format: HH:mm (contoh: 14:30)
/// </summary>
public TimeOnly WaktuKematian { get; set; }
```

#### d. CreateMortalitasAutoFifoDto
File: `DTOs/Mortalitas/CreateMortalitasAutoFifoDto.cs`

```csharp
[Required(ErrorMessage = "Waktu kematian wajib diisi.")]
public TimeOnly WaktuKematian { get; set; }
```

---

### 3. **Service Layer**

#### MortalitasService
File: `Services/MortalitasService.cs`

**Method yang diupdate:**

```csharp
// Method Auto FIFO
public async Task<(bool Success, string Message, List<Mortalitas>? Data)> CreateMortalitasAutoFifoAsync(
    Guid kandangId,
    DateTime tanggalKematian,
    TimeOnly waktuKematian,  // ? BARU
    int jumlahKematian,
    string penyebabKematian,
    IFormFile? fotoMortalitas = null)

// Method Manual Split
public async Task<(bool Success, string Message, List<Mortalitas>? Data)> CreateMortalitasManualSplitAsync(
    Guid kandangId,
    DateTime tanggalKematian,
    TimeOnly waktuKematian,  // ? BARU
    int jumlahDariAyamLama,
    int jumlahDariAyamBaru,
    string penyebabKematian,
    IFormFile? fotoMortalitas = null)
```

---

### 4. **Controller Layer**

#### MortalitasController
File: `Controllers/MortalitasController.cs`

**Endpoints yang diupdate:**

```csharp
[HttpPost]
public async Task<IActionResult> Create([FromBody] CreateMortalitasDto dto)
{
    // Memanggil service dengan waktuKematian
    var result = await _mortalitasService.CreateMortalitasAutoFifoAsync(
        dto.KandangId,
        dto.TanggalKematian,
        dto.WaktuKematian,  // ? BARU
        dto.JumlahKematian,
        dto.PenyebabKematian,
        fotoFile);
}

[HttpPost("auto-fifo")]
public async Task<IActionResult> CreateAutoFifo([FromBody] CreateMortalitasAutoFifoDto dto)
{
    var result = await _mortalitasService.CreateMortalitasAutoFifoAsync(
        dto.KandangId,
        dto.TanggalKematian,
        dto.WaktuKematian,  // ? BARU
        dto.JumlahKematian,
        dto.PenyebabKematian,
        fotoFile);
}

[HttpPut("{id}")]
public async Task<IActionResult> Update(Guid id, [FromBody] UpdateMortalitasDto dto)
{
    var mortalitas = new Models.Mortalitas
    {
        Id = dto.Id,
        AyamId = dto.AyamId,
        TanggalKematian = dto.TanggalKematian,
        WaktuKematian = dto.WaktuKematian,  // ? BARU
        JumlahKematian = dto.JumlahKematian,
        PenyebabKematian = dto.PenyebabKematian
    };
}
```

---

### 5. **Interface Layer**

#### IMortalitasService
File: `Services/Interfaces/IMortalitasService.cs`

```csharp
Task<(bool Success, string Message, List<Mortalitas>? Data)> CreateMortalitasAutoFifoAsync(
    Guid kandangId,
    DateTime tanggalKematian,
    TimeOnly waktuKematian,  // ? BARU
    int jumlahKematian,
    string penyebabKematian,
    IFormFile? fotoMortalitas = null);

Task<(bool Success, string Message, List<Mortalitas>? Data)> CreateMortalitasManualSplitAsync(
    Guid kandangId,
    DateTime tanggalKematian,
    TimeOnly waktuKematian,  // ? BARU
    int jumlahDariAyamLama,
    int jumlahDariAyamBaru,
    string penyebabKematian,
    IFormFile? fotoMortalitas = null);
```

---

### 6. **Database Migration**

Migration: `20260123151640_AddWaktuKematianToMortalitas`

```sql
ALTER TABLE "Mortalitas" 
ADD "WaktuKematian" time without time zone NOT NULL DEFAULT TIME '00:00:00';
```

**Catatan:**
- Kolom baru ditambahkan dengan default value `00:00:00`
- Tipe data: `time without time zone` (PostgreSQL)
- Data existing akan memiliki waktu default `00:00:00`

---

## ?? Contoh Penggunaan API

### 1. Create Mortalitas (Auto FIFO Mode)

**Endpoint:** `POST /api/mortalitas`

**Request Body:**
```json
{
  "mode": "auto-fifo",
  "kandangId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "tanggalKematian": "2026-01-23T08:00:00Z",
  "waktuKematian": "14:30",
  "jumlahKematian": 5,
  "penyebabKematian": "Penyakit respiratory",
  "fotoMortalitasBase64": "data:image/jpeg;base64,/9j/4AAQ...",
  "fotoMortalitasFileName": "mortalitas_230126.jpg"
}
```

### 2. Create Mortalitas (Manual Split Mode)

**Endpoint:** `POST /api/mortalitas`

**Request Body:**
```json
{
  "mode": "manual-split",
  "kandangId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "tanggalKematian": "2026-01-23T08:00:00Z",
  "waktuKematian": "09:15",
  "jumlahKematian": 10,
  "jumlahDariAyamLama": 6,
  "jumlahDariAyamBaru": 4,
  "penyebabKematian": "Stress heat",
  "fotoMortalitasBase64": null,
  "fotoMortalitasFileName": null
}
```

### 3. Update Mortalitas

**Endpoint:** `PUT /api/mortalitas/{id}`

**Request Body:**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "ayamId": "7ba85f64-5717-4562-b3fc-2c963f66afa6",
  "tanggalKematian": "2026-01-23T08:00:00Z",
  "waktuKematian": "15:45",
  "jumlahKematian": 7,
  "penyebabKematian": "Newcastle Disease",
  "fotoMortalitasBase64": null,
  "fotoMortalitasFileName": null
}
```

### 4. Response Example

**GET /api/mortalitas/{id}**

```json
{
  "success": true,
  "message": "Berhasil mengambil detail data mortalitas.",
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "ayamId": "7ba85f64-5717-4562-b3fc-2c963f66afa6",
    "kandangId": "8ca85f64-5717-4562-b3fc-2c963f66afa6",
    "kandangNama": "Kandang A",
    "petugasId": "9da85f64-5717-4562-b3fc-2c963f66afa6",
    "petugasNama": "John Doe",
    "tanggalKematian": "2026-01-23T08:00:00Z",
    "waktuKematian": "14:30",
    "jumlahKematian": 5,
    "penyebabKematian": "Penyakit respiratory",
    "fotoMortalitas": "/uploads/mortalitas/abc123_mortalitas.jpg",
    "fotoMortalitasBase64": "data:image/jpeg;base64,/9j/4AAQ...",
    "jumlahAyamSebelumMati": 100,
    "jumlahAyamSesudahMati": 95,
    "persentaseMortalitas": 5.0,
    "kapasitasKandang": 500,
    "persentaseUtilisasiSebelum": 20.0,
    "persentaseUtilisasiSesudah": 19.0,
    "statusDampak": "High",
    "rekomendasi": "Tingkat mortalitas tinggi (5.0%). Perlu monitoring ketat...",
    "createdAt": "2026-01-23T08:00:00Z",
    "updateAt": "2026-01-23T08:00:00Z"
  }
}
```

---

## ?? Format Waktu

### Input Format (dari Frontend)
- **String format:** `"HH:mm"` (contoh: `"14:30"`, `"08:15"`, `"23:45"`)
- **JSON format:** `"waktuKematian": "14:30"`

### Output Format (dari Backend)
- **JSON response:** `"waktuKematian": "14:30"`
- **Display format:** `14:30` atau format sesuai locale

### Contoh Parsing di Frontend (JavaScript)
```javascript
// Parsing dari string
const waktuKematian = "14:30";

// Menampilkan dengan format
const [jam, menit] = waktuKematian.split(':');
console.log(`Kematian terjadi pada pukul ${jam}:${menit}`);
// Output: "Kematian terjadi pada pukul 14:30"
```

---

## ?? Testing

### Manual Testing dengan HTTP File

**File:** `Tests/MortalitasWaktuKematianTests.http`

```http
### 1. Create Mortalitas dengan Waktu Kematian (Auto FIFO)
POST {{baseUrl}}/api/mortalitas
Authorization: Bearer {{token}}
Content-Type: application/json

{
  "mode": "auto-fifo",
  "kandangId": "{{kandangId}}",
  "tanggalKematian": "2026-01-23T08:00:00Z",
  "waktuKematian": "14:30",
  "jumlahKematian": 5,
  "penyebabKematian": "Test dengan waktu kematian"
}

### 2. Create Mortalitas dengan Waktu Kematian (Manual Split)
POST {{baseUrl}}/api/mortalitas
Authorization: Bearer {{token}}
Content-Type: application/json

{
  "mode": "manual-split",
  "kandangId": "{{kandangId}}",
  "tanggalKematian": "2026-01-23T08:00:00Z",
  "waktuKematian": "09:15",
  "jumlahKematian": 10,
  "jumlahDariAyamLama": 6,
  "jumlahDariAyamBaru": 4,
  "penyebabKematian": "Test manual split dengan waktu"
}

### 3. Get Mortalitas by ID (verify waktu kematian)
GET {{baseUrl}}/api/mortalitas/{{mortalitasId}}
Authorization: Bearer {{token}}

### 4. Update Mortalitas dengan Waktu Kematian Baru
PUT {{baseUrl}}/api/mortalitas/{{mortalitasId}}
Authorization: Bearer {{token}}
Content-Type: application/json

{
  "id": "{{mortalitasId}}",
  "ayamId": "{{ayamId}}",
  "tanggalKematian": "2026-01-23T08:00:00Z",
  "waktuKematian": "15:45",
  "jumlahKematian": 7,
  "penyebabKematian": "Updated dengan waktu baru"
}
```

---

## ?? Validasi

### Backend Validation
1. ? `WaktuKematian` wajib diisi (Required)
2. ? Format harus valid `TimeOnly` (HH:mm)
3. ? Tidak boleh null atau empty

### Frontend Validation (Rekomendasi)
```javascript
// Validasi format waktu
function validateWaktuKematian(waktu) {
    const timeRegex = /^([0-1]?[0-9]|2[0-3]):[0-5][0-9]$/;
    
    if (!waktu || waktu.trim() === '') {
        return { valid: false, message: 'Waktu kematian wajib diisi' };
    }
    
    if (!timeRegex.test(waktu)) {
        return { valid: false, message: 'Format waktu tidak valid. Gunakan format HH:mm (contoh: 14:30)' };
    }
    
    return { valid: true, message: 'Valid' };
}

// Contoh penggunaan
const validation = validateWaktuKematian("14:30");
if (!validation.valid) {
    alert(validation.message);
}
```

---

## ?? UI/UX Recommendations

### Form Input
```html
<!-- HTML5 Time Input -->
<label for="waktuKematian">Waktu Kematian *</label>
<input 
    type="time" 
    id="waktuKematian" 
    name="waktuKematian"
    required
    placeholder="HH:mm"
/>

<!-- Atau dengan custom time picker component -->
<TimePicker 
    label="Waktu Kematian *"
    format="HH:mm"
    value={waktuKematian}
    onChange={handleWaktuChange}
    required
/>
```

### Display Format
```javascript
// Display waktu kematian di tabel atau card
function formatWaktuKematian(waktu) {
    return `Pukul ${waktu} WIB`;
}

// Contoh: "Pukul 14:30 WIB"
```

---

## ?? Migration Status

? **Migration berhasil dijalankan**
- Migration file: `20260123151640_AddWaktuKematianToMortalitas.cs`
- Database column: `WaktuKematian` (type: `time without time zone`)
- Default value untuk data existing: `00:00:00`

---

## ?? Catatan Penting

1. **Data Existing:** 
   - Data mortalitas yang sudah ada akan memiliki `WaktuKematian` default `00:00:00`
   - Perlu diupdate manual jika diperlukan

2. **TimeZone:**
   - `WaktuKematian` adalah waktu lokal (tidak ada timezone info)
   - `TanggalKematian` tetap menggunakan UTC timezone
   - Kombinasi keduanya memberikan informasi lengkap tentang kapan kematian terjadi

3. **Validation:**
   - Waktu harus dalam format 24 jam (00:00 - 23:59)
   - Field wajib diisi pada create dan update

4. **Performance:**
   - Index tidak ditambahkan pada `WaktuKematian` karena bukan field yang sering di-query
   - Jika diperlukan filter berdasarkan waktu, pertimbangkan untuk menambahkan index

---

## ? Checklist Implementasi

- [x] Update Model `Mortalitas`
- [x] Update `CreateMortalitasDto`
- [x] Update `UpdateMortalitasDto`
- [x] Update `MortalitasResponseDto`
- [x] Update `CreateMortalitasAutoFifoDto`
- [x] Update `IMortalitasService` interface
- [x] Update `MortalitasService` implementation
- [x] Update `MortalitasController`
- [x] Create dan apply database migration
- [x] Build verification
- [x] Create documentation

---

## ?? Next Steps (Opsional)

1. **Analisis Waktu Kematian:**
   - Tambahkan analytics untuk melihat pola waktu kematian
   - Identifikasi jam-jam rawan kematian ayam

2. **Reporting:**
   - Include waktu kematian dalam laporan mortalitas
   - Chart distribusi waktu kematian (heatmap per jam)

3. **Alert System:**
   - Notifikasi jika ada cluster kematian pada jam tertentu
   - Early warning untuk mencegah kematian massal

---

## ?? Support

Jika ada pertanyaan atau issue terkait implementasi ini, silakan hubungi tim development.

---

**Dokumentasi dibuat:** 23 Januari 2026  
**Terakhir diupdate:** 23 Januari 2026  
**Versi:** 1.0

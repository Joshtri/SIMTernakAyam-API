# Kandang Asisten with Ayam Sisa Feature

## Overview
Fitur ini menambahkan kemampuan untuk melihat informasi **ayam sisa** (ayam yang ditandai sebagai sisa periode sebelumnya) pada endpoint Kandang Asisten.

## Problem Statement
Dalam sistem peternakan, sering terjadi kondisi dimana ada ayam yang tersisa dari periode sebelumnya yang belum dipanen. Penting bagi asisten kandang untuk mengetahui informasi ini agar dapat:
1. Memantau status ayam sisa di kandang yang mereka kelola
2. Merencanakan panen atau pengelolaan ayam sisa
3. Mengetahui alasan mengapa ayam tersebut menjadi sisa

## Features Added

### 1. New DTO: `KandangAsistenWithAyamSisaDto`
DTO baru yang extends informasi kandang asisten dengan data ayam sisa.

**Properties:**
```csharp
public class KandangAsistenWithAyamSisaDto
{
    // Standard KandangAsisten fields
    public Guid Id { get; set; }
    public Guid KandangId { get; set; }
    public string? KandangNama { get; set; }
    public Guid AsistenId { get; set; }
    public string? AsistenNama { get; set; }
    public string? AsistenEmail { get; set; }
    public string? AsistenNoWA { get; set; }
    public string? Catatan { get; set; }
    public bool IsAktif { get; set; }
    
    // Ayam Sisa Information
    public List<AyamSisaInfo>? AyamSisaList { get; set; }
    public int TotalAyamSisa { get; set; }
}

public class AyamSisaInfo
{
    public Guid Id { get; set; }
    public DateTime? TanggalMasuk { get; set; }
    public int JumlahMasukAwal { get; set; }
    public string? AlasanSisa { get; set; }
    public DateTime? TanggalDitandaiSisa { get; set; }
    public bool IsAyamSisa { get; set; }
    public int UmurAyam { get; set; } // Dalam hari
}
```

### 2. New Repository Method
Added to `IAyamRepository` and `AyamRepository`:

```csharp
/// <summary>
/// Get ayam sisa (IsAyamSisa = true) by kandangId
/// </summary>
Task<IEnumerable<Ayam>> GetAyamSisaByKandangIdAsync(Guid kandangId);
```

**Implementation:**
- Filter ayam where `IsAyamSisa = true`
- Include Kandang and User details
- Order by `TanggalDitandaiSisa` (desc) then `TanggalMasuk` (desc)

### 3. New Service Method
Added to `IKandangAsistenService` and `KandangAsistenService`:

```csharp
/// <summary>
/// Mendapatkan asisten dengan informasi ayam sisa di kandang
/// </summary>
Task<IEnumerable<Models.Ayam>> GetAyamSisaByKandangAsync(Guid kandangId);
```

### 4. New API Endpoint

#### Endpoint Details
```
GET /api/kandang-asistens/by-kandang/{kandangId}/with-ayam-sisa
```

**Description:** Get all asistens for a specific kandang WITH ayam sisa information

**Parameters:**
- `kandangId` (path parameter, required): GUID of the kandang

**Response Example:**
```json
{
  "success": true,
  "message": "Data asisten kandang dengan informasi ayam sisa berhasil diambil",
  "data": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440000",
      "kandangId": "fd082b84-41c8-45d4-8480-ddb918f74cca",
      "kandangNama": "Kandang A",
      "asistenId": "660e8400-e29b-41d4-a716-446655440000",
      "asistenNama": "John Doe",
      "asistenEmail": "john@example.com",
      "asistenNoWA": "081234567890",
      "catatan": "Asisten shift pagi",
      "isAktif": true,
      "createdAt": "2024-01-01T00:00:00Z",
      "updateAt": "2024-01-01T00:00:00Z",
      "ayamSisaList": [
        {
          "id": "770e8400-e29b-41d4-a716-446655440000",
          "tanggalMasuk": "2024-01-01T00:00:00Z",
          "jumlahMasukAwal": 1000,
          "alasanSisa": "Belum mencapai target panen",
          "tanggalDitandaiSisa": "2024-02-01T00:00:00Z",
          "isAyamSisa": true,
          "umurAyam": 45
        },
        {
          "id": "880e8400-e29b-41d4-a716-446655440000",
          "tanggalMasuk": "2024-01-15T00:00:00Z",
          "jumlahMasukAwal": 500,
          "alasanSisa": "Ayam kurang sehat untuk dipanen",
          "tanggalDitandaiSisa": "2024-02-15T00:00:00Z",
          "isAyamSisa": true,
          "umurAyam": 30
        }
      ],
      "totalAyamSisa": 1500
    }
  ]
}
```

## API Comparison

### Original Endpoint (Still Available)
```
GET /api/kandang-asistens/by-kandang/{kandangId}
```
Returns: Standard KandangAsisten information without ayam sisa details

### New Enhanced Endpoint
```
GET /api/kandang-asistens/by-kandang/{kandangId}/with-ayam-sisa
```
Returns: KandangAsisten information WITH ayam sisa details

## Use Cases

### 1. Dashboard Asisten Kandang
Asisten dapat melihat ayam sisa yang perlu dikelola:
```
GET /api/kandang-asistens/by-kandang/{kandangId}/with-ayam-sisa
```

### 2. Monitoring Ayam Sisa
Management dapat melihat berapa banyak ayam sisa per kandang dan alasannya

### 3. Perencanaan Panen
Tim dapat merencanakan panen ayam sisa berdasarkan umur dan jumlahnya

## Database Fields Used

### From `Ayam` Model:
- `IsAyamSisa` (bool): Flag untuk menandai ayam sebagai sisa
- `AlasanSisa` (string?): Alasan kenapa ayam menjadi sisa
- `TanggalDitandaiSisa` (DateTime?): Kapan ayam ditandai sebagai sisa
- `TanggalMasuk` (DateTime?): Tanggal ayam masuk pertama kali
- `JumlahMasuk` (int): Jumlah ayam yang masuk

## Testing

Test file: `SIMTernakAyam\Tests\KandangAsistenAyamSisaTests.http`

**Test Cases:**
1. Get kandang asisten dengan ayam sisa
2. Get kandang asisten tanpa ayam sisa (empty list)
3. Compare response dengan endpoint original

## Benefits

1. ? **Visibility**: Asisten dapat melihat ayam sisa yang harus dikelola
2. ? **Planning**: Memudahkan perencanaan panen atau pengelolaan ayam sisa
3. ? **Traceability**: Mengetahui alasan dan kapan ayam ditandai sebagai sisa
4. ? **Age Tracking**: Mengetahui umur ayam sisa dalam hari
5. ? **Backward Compatible**: Endpoint lama masih berfungsi normal

## Implementation Notes

- Endpoint baru menggunakan suffix `/with-ayam-sisa` untuk membedakan dengan endpoint original
- Ayam sisa diurutkan berdasarkan `TanggalDitandaiSisa` (terbaru dulu), kemudian `TanggalMasuk`
- `UmurAyam` dihitung otomatis berdasarkan `DateTime.UtcNow - TanggalMasuk`
- Response tetap menggunakan format standard dengan `success`, `message`, dan `data`

## Future Enhancements

1. Filter ayam sisa berdasarkan umur (e.g., > 30 hari)
2. Alert jika ayam sisa sudah terlalu lama (e.g., > 60 hari)
3. Statistik ayam sisa per bulan
4. Export laporan ayam sisa

# Fitur Ayam Sisa dari Periode Sebelumnya - Kandang Detail API

## ?? Problem Statement

Dalam sistem peternakan ayam, sering terjadi kondisi dimana:
- Ada **beberapa batch ayam** yang masuk ke kandang pada waktu berbeda
- Batch lama belum habis dipanen, lalu batch baru sudah masuk
- **Ayam dari periode/batch lama yang masih tersisa** perlu dipantau secara khusus
- Petugas perlu tahu berapa banyak ayam lama yang masih ada dan berapa umurnya

## ? Solution

Menambahkan informasi **Ayam Sisa dari Periode Sebelumnya** di endpoint `/api/kandangs/{kandangId}`.

### Definisi "Ayam Sisa Periode Sebelumnya"
**Ayam Sisa** adalah ayam dari batch/periode LAMA (bukan batch terbaru) yang:
1. **Masih ada sisanya** (`SisaHidup > 0`)
2. **Bukan dari batch terbaru** (batch terbaru dianggap periode aktif)
3. Belum habis dipanen atau mati

### Contoh Skenario

**Kandang A memiliki 3 batch ayam:**

| Batch | Tanggal Masuk | Jumlah Masuk | Dipanen | Mati | Sisa Hidup | Status |
|-------|---------------|--------------|---------|------|------------|--------|
| 1     | 01/01/2024    | 1000         | 700     | 100  | **200**    | ?? Ayam Sisa (90 hari) |
| 2     | 15/02/2024    | 800          | 500     | 50   | **250**    | ?? Ayam Sisa (60 hari) |
| 3     | 01/04/2024    | 1000         | 0       | 20   | **980**    | ? Periode Aktif |

**Hasil di `ayamSisaList`:**
- Batch 1: 200 ekor (umur 90 hari) ? **Perlu Perhatian** ??
- Batch 2: 250 ekor (umur 60 hari) ? **Perlu Perhatian** ??
- Batch 3: **TIDAK TAMPIL** (batch terbaru/periode aktif)

**Total Ayam Sisa:** 450 ekor

## ?? Technical Implementation

### 1. Updated DTO: `KandangDetailDto`

```csharp
public class KandangDetailDto
{
    // ...existing properties...
    
    // ? NEW: Informasi Ayam Sisa
    public List<AyamSisaDetailDto> AyamSisaList { get; set; } = new();
    public int TotalAyamSisa { get; set; }
    
    // ...existing properties...
}

public class AyamSisaDetailDto
{
    public Guid Id { get; set; }
    public DateTime TanggalMasuk { get; set; }
    public int JumlahMasukAwal { get; set; }
    public int SisaHidup { get; set; }
    public string? AlasanSisa { get; set; }
    public DateTime? TanggalDitandaiSisa { get; set; }
    public int UmurAyam { get; set; } // Umur dalam hari
    public bool PerluPerhatian { get; set; } // True jika umur > 60 hari
}
```

### 2. Logic di Controller

```csharp
// Get all ayam di kandang (ordered by TanggalMasuk DESC = terbaru dulu)
var ayamList = await _context.Ayams
    .Where(a => a.KandangId == id)
    .OrderByDescending(a => a.TanggalMasuk)
    .ToListAsync();

var ayamSisaDetails = new List<AyamSisaDetailDto>();

// Jika ada lebih dari 1 batch, batch selain yang terbaru = "ayam sisa periode sebelumnya"
if (ayamList.Count > 1)
{
    // Skip batch terbaru (index 0)
    var ayamPeriodeSebelumnya = ayamList.Skip(1).ToList();
    
    foreach (var ayamLama in ayamPeriodeSebelumnya)
    {
        var totalPanen = await _context.Panens
            .Where(p => p.AyamId == ayamLama.Id)
            .SumAsync(p => p.JumlahEkorPanen);
        
        var totalMortalitas = await _context.Mortalitas
            .Where(m => m.AyamId == ayamLama.Id)
            .SumAsync(m => m.JumlahKematian);
        
        var sisaHidup = Math.Max(0, ayamLama.JumlahMasuk - totalPanen - totalMortalitas);
        
        // Hanya tampilkan jika masih ada sisa hidup
        if (sisaHidup > 0)
        {
            var umurAyam = (DateTime.UtcNow - ayamLama.TanggalMasuk).Days;
            
            ayamSisaDetails.Add(new AyamSisaDetailDto
            {
                Id = ayamLama.Id,
                TanggalMasuk = ayamLama.TanggalMasuk,
                JumlahMasukAwal = ayamLama.JumlahMasuk,
                SisaHidup = sisaHidup,
                AlasanSisa = ayamLama.AlasanSisa ?? $"Sisa dari periode {ayamLama.TanggalMasuk:dd/MM/yyyy}",
                TanggalDitandaiSisa = ayamLama.TanggalDitandaiSisa,
                UmurAyam = umurAyam,
                PerluPerhatian = umurAyam > 60 // Alert jika > 60 hari
            });
        }
    }
}
```

## ?? API Details

### Endpoint
```
GET /api/kandangs/{kandangId}
```

### Response Example

```json
{
  "success": true,
  "message": "Berhasil mengambil detail kandang dengan history lengkap.",
  "data": {
    "id": "fd082b84-41c8-45d4-8480-ddb918f74cca",
    "namaKandang": "Kandang A",
    "kapasitas": 1000,
    "lokasi": "Blok A",
    "petugasId": "guid",
    "petugasNama": "John Doe",
    
    "jumlahAyamTerisi": 500,
    "kapasitasTersedia": 500,
    "persentaseTerisi": 50.00,
    "statusKapasitas": "Tersedia",
    
    "totalAyamMasuk": 2000,
    "totalPanen": 1200,
    "totalMortalitas": 300,
    "totalOperasional": 45,
    
    "ayamSisaList": [
      {
        "id": "ayam-guid-1",
        "tanggalMasuk": "2024-01-01T00:00:00Z",
        "jumlahMasukAwal": 1000,
        "sisaHidup": 200,
        "alasanSisa": "Sisa dari periode 01/01/2024",
        "tanggalDitandaiSisa": null,
        "umurAyam": 90,
        "perluPerhatian": true
      },
      {
        "id": "ayam-guid-2",
        "tanggalMasuk": "2024-02-01T00:00:00Z",
        "jumlahMasukAwal": 800,
        "sisaHidup": 250,
        "alasanSisa": "Sisa dari periode 01/02/2024",
        "tanggalDitandaiSisa": null,
        "umurAyam": 60,
        "perluPerhatian": false
      }
    ],
    "totalAyamSisa": 450,
    
    "historyAyamMasuk": [...],
    "historyPanen": [...],
    "historyMortalitas": [...],
    "historyOperasional": [...]
  }
}
```

## ?? Frontend Implementation Guide

### TypeScript Interface

```typescript
interface AyamSisaDetail {
  id: string;
  tanggalMasuk: string;
  jumlahMasukAwal: number;
  sisaHidup: number;
  alasanSisa: string | null;
  tanggalDitandaiSisa: string | null;
  umurAyam: number; // dalam hari
  perluPerhatian: boolean; // true jika umur > 60 hari
}

interface KandangDetail {
  // ...existing fields...
  
  // Ayam Sisa Info
  ayamSisaList: AyamSisaDetail[];
  totalAyamSisa: number;
  
  // ...existing fields...
}
```

### React Component Example

```tsx
import { useState, useEffect } from 'react';
import axios from 'axios';

const KandangDetailPage = ({ kandangId }: { kandangId: string }) => {
  const [kandang, setKandang] = useState<KandangDetail | null>(null);
  
  useEffect(() => {
    loadKandangDetail();
  }, [kandangId]);
  
  const loadKandangDetail = async () => {
    const response = await axios.get(
      `https://localhost:7195/api/kandangs/${kandangId}`
    );
    
    if (response.data.success) {
      setKandang(response.data.data);
    }
  };
  
  return (
    <div className="kandang-detail">
      <h1>{kandang?.namaKandang}</h1>
      
      {/* Ayam Sisa Section */}
      <div className="ayam-sisa-section">
        <h2>Ayam Sisa dari Periode Sebelumnya</h2>
        <p className="total">Total: {kandang?.totalAyamSisa || 0} ekor</p>
        
        {kandang?.ayamSisaList && kandang.ayamSisaList.length > 0 ? (
          <table>
            <thead>
              <tr>
                <th>Tanggal Masuk</th>
                <th>Jumlah Awal</th>
                <th>Sisa Hidup</th>
                <th>Umur (Hari)</th>
                <th>Status</th>
              </tr>
            </thead>
            <tbody>
              {kandang.ayamSisaList.map((ayam) => (
                <tr key={ayam.id} className={ayam.perluPerhatian ? 'warning' : ''}>
                  <td>{new Date(ayam.tanggalMasuk).toLocaleDateString('id-ID')}</td>
                  <td>{ayam.jumlahMasukAwal}</td>
                  <td>{ayam.sisaHidup}</td>
                  <td>{ayam.umurAyam}</td>
                  <td>
                    {ayam.perluPerhatian ? (
                      <span className="badge badge-warning">?? Perlu Perhatian</span>
                    ) : (
                      <span className="badge badge-info">Normal</span>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        ) : (
          <p>? Tidak ada ayam sisa dari periode sebelumnya</p>
        )}
      </div>
    </div>
  );
};
```

### CSS Styling Example

```css
.ayam-sisa-section {
  background: #fff3cd;
  padding: 20px;
  border-radius: 8px;
  margin: 20px 0;
  border-left: 4px solid #ffc107;
}

.ayam-sisa-section h2 {
  color: #856404;
  margin-bottom: 10px;
}

.ayam-sisa-section .total {
  font-size: 18px;
  font-weight: bold;
  color: #856404;
  margin-bottom: 15px;
}

.ayam-sisa-section table tr.warning {
  background-color: #ffebee;
  font-weight: bold;
}

.badge {
  padding: 5px 10px;
  border-radius: 4px;
  font-size: 12px;
}

.badge-warning {
  background-color: #ff9800;
  color: white;
}

.badge-info {
  background-color: #2196F3;
  color: white;
}
```

## ?? Use Cases

### 1. **Dashboard Petugas Kandang**
```
Menampilkan alert:
"?? Ada 450 ekor ayam sisa dari periode sebelumnya yang perlu segera dipanen!"
```

### 2. **Perencanaan Panen**
```
Tim dapat melihat:
- Batch mana yang masih ada sisa
- Umur ayam sisa berapa hari
- Prioritas panen (yang umurnya > 60 hari dipanen dulu)
```

### 3. **Monitoring Kesehatan**
```
Jika ayam sisa terlalu lama (> 90 hari):
- Kemungkinan ada masalah kesehatan
- Perlu pemeriksaan dokter hewan
- Atau market tidak menerima ayam tua
```

### 4. **Optimasi Kapasitas Kandang**
```
Jika ayam sisa banyak:
- Jangan tambah batch baru dulu
- Fokus habiskan ayam sisa terlebih dahulu
- Hindari overcrowding kandang
```

## ?? Alert Rules

| Umur Ayam | Status | Tindakan |
|-----------|--------|----------|
| 0-30 hari | ?? Normal | Monitor biasa |
| 31-60 hari | ?? Perhatian | Rencanakan panen |
| 61-90 hari | ?? Urgent | Segera panen |
| > 90 hari | ?? Kritis | Panen ASAP / Cek kesehatan |

## ?? Business Benefits

1. ? **Visibility**: Petugas tahu persis ada berapa ayam sisa periode lama
2. ? **Planning**: Memudahkan perencanaan panen prioritas
3. ? **Efficiency**: Hindari ayam terlalu lama di kandang (kualitas menurun)
4. ? **Cost Saving**: Kurangi biaya pakan untuk ayam yang sudah siap panen
5. ? **Quality Control**: Ayam terlalu tua kualitasnya menurun, harga jual lebih rendah

## ?? Important Notes

### ?? Yang TIDAK Termasuk Ayam Sisa:
1. **Batch terbaru** (periode aktif saat ini)
2. Ayam yang `SisaHidup = 0` (sudah habis)

### ? Yang Termasuk Ayam Sisa:
1. Batch LAMA (bukan terbaru)
2. `SisaHidup > 0`
3. Dari periode/tanggal masuk sebelumnya

### ?? Example Logic:

**Kandang memiliki 3 batch:**
- Batch C (01/04/2024) ? Terbaru ? **TIDAK MASUK ayamSisaList**
- Batch B (15/02/2024) ? Lama, sisa 250 ? **MASUK ayamSisaList** ?
- Batch A (01/01/2024) ? Lama, sisa 200 ? **MASUK ayamSisaList** ?

## ?? Testing

Test file: `SIMTernakAyam\Tests\KandangAyamSisaPeriodeSebelumnya.http`

**Test Scenarios:**
1. Kandang dengan beberapa batch (ada ayam sisa)
2. Kandang dengan 1 batch saja (tidak ada ayam sisa)
3. Kandang dengan batch lama tapi sudah habis dipanen (tidak ada ayam sisa)

## ?? Related Documentation

- `KandangDetailDto.cs`: DTO definitions
- `KandangController.cs`: Implementation logic
- Test file untuk endpoint testing

---

**Last Updated:** 2024  
**Feature Status:** ? Implemented & Tested  
**API Version:** 1.0

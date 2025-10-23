# 🔌 New API Endpoints - Backend Updates

> Quick reference untuk semua API endpoints baru yang ditambahkan ke backend.

---

## 1️⃣ Kandang Asisten API

**Base URL:** `/api/kandang-asistens`

### GET - Get All Assistants
```
GET /api/kandang-asistens
Authorization: Bearer {token}
Roles: All authenticated users

Response: ApiResponse<List<KandangAsistenResponseDto>>
```

### GET - Get Assistant by ID
```
GET /api/kandang-asistens/{id}
Authorization: Bearer {token}
Roles: All authenticated users

Response: ApiResponse<KandangAsistenResponseDto>
```

### GET - Get Assistants by Kandang
```
GET /api/kandang-asistens/by-kandang/{kandangId}
Authorization: Bearer {token}
Roles: All authenticated users

Response: ApiResponse<List<KandangAsistenResponseDto>>
```

### GET - Get Active Assistants by Kandang
```
GET /api/kandang-asistens/by-kandang/{kandangId}/active
Authorization: Bearer {token}
Roles: All authenticated users

Response: ApiResponse<List<KandangAsistenResponseDto>>
```

### GET - Get Kandang Assignments by Asisten
```
GET /api/kandang-asistens/by-asisten/{asistenId}
Authorization: Bearer {token}
Roles: All authenticated users

Response: ApiResponse<List<KandangAsistenResponseDto>>
```

### POST - Create Assistant Assignment
```
POST /api/kandang-asistens
Authorization: Bearer {token}
Roles: All authenticated users
Content-Type: application/json

Request Body:
{
  "kandangId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "asistenId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "catatan": "Asisten pengganti sementara",  // optional
  "isAktif": true
}

Response: ApiResponse<KandangAsistenResponseDto>
```

### PUT - Update Assistant Assignment
```
PUT /api/kandang-asistens/{id}
Authorization: Bearer {token}
Roles: All authenticated users
Content-Type: application/json

Request Body:
{
  "catatan": "Updated catatan",  // optional
  "isAktif": false              // optional
}

Response: ApiResponse<KandangAsistenResponseDto>
```

### DELETE - Delete Assistant Assignment
```
DELETE /api/kandang-asistens/{id}
Authorization: Bearer {token}
Roles: All authenticated users

Response: ApiResponse<object>
```

---

## 2️⃣ Vaksin & Vitamin API (Updated)

**Base URL:** `/api/vaksins`

### GET - Get by Type (NEW ENDPOINT)
```
GET /api/vaksins/by-type/{type}
Authorization: Bearer {token}
Roles: All authenticated users

Parameters:
- type: "Vaksin" or "Vitamin"

Example:
GET /api/vaksins/by-type/Vaksin
GET /api/vaksins/by-type/Vitamin

Response: ApiResponse<List<VaksinResponseDto>>
```

### POST - Create Vaksin/Vitamin (UPDATED)
```
POST /api/vaksins
Authorization: Bearer {token}
Roles: All authenticated users
Content-Type: application/json

Request Body (UPDATED - added tipe field):
{
  "namaVaksin": "ND Vaccine",
  "jenis": "Live",
  "stok": 100,
  "satuan": "Dosis",
  "hargaPerSatuan": 5000,
  "tipe": 0  // 0 = Vaksin, 1 = Vitamin (NEW FIELD)
}

Response: ApiResponse<VaksinResponseDto>
```

### PUT - Update Vaksin/Vitamin (UPDATED)
```
PUT /api/vaksins/{id}
Authorization: Bearer {token}
Roles: All authenticated users
Content-Type: application/json

Request Body (UPDATED - added tipe field):
{
  "namaVaksin": "ND Vaccine Updated",
  "jenis": "Live",
  "stok": 150,
  "satuan": "Dosis",
  "hargaPerSatuan": 5500,
  "tipe": 0  // 0 = Vaksin, 1 = Vitamin (NEW FIELD)
}

Response: ApiResponse<VaksinResponseDto>
```

**Response DTO (UPDATED):**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "namaVaksin": "ND Vaccine",
  "jenis": "Live",
  "stok": 100,
  "satuan": "Dosis",
  "hargaPerSatuan": 5000,
  "tipe": 0,  // NEW FIELD: 0 = Vaksin, 1 = Vitamin
  "createdAt": "2025-01-20T10:00:00Z",
  "updatedAt": "2025-01-20T10:00:00Z"
}
```

---

## 3️⃣ Biaya Operasional Bulanan API

**Base URL:** `/api/biayas`

### GET - Rekap Biaya Bulanan (NEW ENDPOINT)
```
GET /api/biayas/rekap-bulanan/{bulan}/{tahun}
Authorization: Bearer {token}
Roles: All authenticated users

Parameters:
- bulan: 1-12 (January = 1, December = 12)
- tahun: Year (e.g., 2025)

Example:
GET /api/biayas/rekap-bulanan/1/2025  // January 2025
GET /api/biayas/rekap-bulanan/12/2024 // December 2024

Response: ApiResponse<RekapBiayaBulananDto>
```

**Response Example:**
```json
{
  "success": true,
  "message": "Berhasil mengambil rekap biaya bulanan.",
  "data": {
    "bulan": 1,
    "tahun": 2025,
    "totalBiaya": 5000000,
    "detailPerKandang": [
      {
        "kandangId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "kandangNama": "Kandang A",
        "totalBiaya": 2000000,
        "detailBiaya": [
          {
            "jenisBiaya": "Listrik",
            "jumlah": 1000000,
            "tanggal": "2025-01-05T00:00:00Z",
            "keterangan": "Tagihan bulan Januari",
            "catatan": "Pembayaran tepat waktu"
          },
          {
            "jenisBiaya": "Air",
            "jumlah": 1000000,
            "tanggal": "2025-01-05T00:00:00Z",
            "keterangan": "Tagihan bulan Januari",
            "catatan": null
          }
        ]
      },
      {
        "kandangId": null,
        "kandangNama": "Biaya Umum",
        "totalBiaya": 3000000,
        "detailBiaya": [...]
      }
    ]
  }
}
```

### POST - Create Biaya (UPDATED)
```
POST /api/biayas
Authorization: Bearer {token}
Roles: All authenticated users
Content-Type: application/json

Request Body (UPDATED - added new fields):
{
  "jenisBiaya": "Listrik",
  "jumlah": 1000000,
  "petugasId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "tanggal": "2025-01-05T00:00:00Z",
  "keterangan": "Tagihan listrik Januari 2025",
  "kandangId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",  // NEW (optional)
  "catatan": "Pembayaran tepat waktu",                  // NEW (optional)
  "bulan": 1,                                            // NEW (optional)
  "tahun": 2025                                          // NEW (optional)
}

Response: ApiResponse<BiayaResponseDto>
```

### PUT - Update Biaya (UPDATED)
```
PUT /api/biayas/{id}
Authorization: Bearer {token}
Roles: All authenticated users
Content-Type: application/json

Request Body (UPDATED - added new fields):
{
  "jenisBiaya": "Listrik",
  "jumlah": 1200000,
  "petugasId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "tanggal": "2025-01-05T00:00:00Z",
  "keterangan": "Tagihan listrik Januari 2025 (Updated)",
  "kandangId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",  // NEW (optional)
  "catatan": "Sudah dibayar",                            // NEW (optional)
  "bulan": 1,                                            // NEW (optional)
  "tahun": 2025                                          // NEW (optional)
}

Response: ApiResponse<BiayaResponseDto>
```

---

## 4️⃣ Laporan Operasional PDF API

**Base URL:** `/api/laporan`

### GET - Download Laporan Operasional PDF (NEW ENDPOINT)
```
GET /api/laporan/operasional/pdf/{kandangId}?startDate={date}&endDate={date}
Authorization: Bearer {token}
Roles: Operator, Pemilik ONLY

Parameters:
- kandangId: UUID (required, in path)
- startDate: DateTime (optional, query string, format: yyyy-MM-dd)
- endDate: DateTime (optional, query string, format: yyyy-MM-dd)

Example:
GET /api/laporan/operasional/pdf/3fa85f64-5717-4562-b3fc-2c963f66afa6
GET /api/laporan/operasional/pdf/3fa85f64-5717-4562-b3fc-2c963f66afa6?startDate=2025-01-01&endDate=2025-01-31

Response: File (application/pdf)
Filename: Laporan_Operasional_20250101_20250131.pdf
```

**Frontend Implementation Example:**
```javascript
// JavaScript/TypeScript
async function downloadLaporanOperasionalPdf(kandangId, startDate, endDate) {
  const params = new URLSearchParams();
  if (startDate) params.append('startDate', startDate);
  if (endDate) params.append('endDate', endDate);

  const response = await fetch(
    `/api/laporan/operasional/pdf/${kandangId}?${params.toString()}`,
    {
      headers: {
        'Authorization': `Bearer ${token}`
      }
    }
  );

  const blob = await response.blob();
  const url = window.URL.createObjectURL(blob);
  const a = document.createElement('a');
  a.href = url;
  a.download = `Laporan_Operasional_${kandangId}.pdf`;
  a.click();
}
```

---

## 5️⃣ Laporan Kesehatan PDF API

**Base URL:** `/api/laporan`

### GET - Download Laporan Kesehatan PDF (NEW ENDPOINT)
```
GET /api/laporan/kesehatan/pdf/{kandangId}?startDate={date}&endDate={date}
Authorization: Bearer {token}
Roles: Operator, Pemilik ONLY

Parameters:
- kandangId: UUID (required, in path)
- startDate: DateTime (optional, query string, format: yyyy-MM-dd)
- endDate: DateTime (optional, query string, format: yyyy-MM-dd)

Example:
GET /api/laporan/kesehatan/pdf/3fa85f64-5717-4562-b3fc-2c963f66afa6
GET /api/laporan/kesehatan/pdf/3fa85f64-5717-4562-b3fc-2c963f66afa6?startDate=2025-01-01&endDate=2025-01-31

Response: File (application/pdf)
Filename: Laporan_Kesehatan_Ayam_20250101_20250131.pdf
```

**Frontend Implementation Example:**
```javascript
// JavaScript/TypeScript
async function downloadLaporanKesehatanPdf(kandangId, startDate, endDate) {
  const params = new URLSearchParams();
  if (startDate) params.append('startDate', startDate);
  if (endDate) params.append('endDate', endDate);

  const response = await fetch(
    `/api/laporan/kesehatan/pdf/${kandangId}?${params.toString()}`,
    {
      headers: {
        'Authorization': `Bearer ${token}`
      }
    }
  );

  const blob = await response.blob();
  const url = window.URL.createObjectURL(blob);
  const a = document.createElement('a');
  a.href = url;
  a.download = `Laporan_Kesehatan_${kandangId}.pdf`;
  a.click();
}
```

---

## 📊 Summary of Changes

### New Endpoints (Total: 11)
1. **Kandang Asisten**: 8 endpoints (full CRUD)
2. **Vaksin by Type**: 1 endpoint (filter)
3. **Rekap Biaya Bulanan**: 1 endpoint (monthly summary)
4. **Laporan Operasional PDF**: 1 endpoint (download)
5. **Laporan Kesehatan PDF**: 1 endpoint (download)

### Modified Endpoints (Total: 2)
1. **POST /api/vaksins** - Added `tipe` field
2. **PUT /api/vaksins/{id}** - Added `tipe` field
3. **POST /api/biayas** - Added `kandangId`, `catatan`, `bulan`, `tahun` fields
4. **PUT /api/biayas/{id}** - Added `kandangId`, `catatan`, `bulan`, `tahun` fields

---

## 🔐 Authorization Requirements

| Endpoint | Roles Required |
|----------|---------------|
| `/api/kandang-asistens/*` | All authenticated users |
| `/api/vaksins/*` | All authenticated users |
| `/api/biayas/*` | All authenticated users |
| `/api/laporan/operasional/pdf/*` | **Operator, Pemilik only** |
| `/api/laporan/kesehatan/pdf/*` | **Operator, Pemilik only** |

**Note:** Petugas role CANNOT access PDF report endpoints.

---

## 🧪 Testing Endpoints

### Using cURL:

```bash
# Get vaksin by type
curl -X GET "https://yourapi.com/api/vaksins/by-type/Vaksin" \
  -H "Authorization: Bearer YOUR_TOKEN"

# Get rekap biaya bulanan
curl -X GET "https://yourapi.com/api/biayas/rekap-bulanan/1/2025" \
  -H "Authorization: Bearer YOUR_TOKEN"

# Download PDF operasional
curl -X GET "https://yourapi.com/api/laporan/operasional/pdf/KANDANG_ID?startDate=2025-01-01&endDate=2025-01-31" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  --output laporan_operasional.pdf

# Create kandang asisten
curl -X POST "https://yourapi.com/api/kandang-asistens" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "kandangId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "asistenId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "catatan": "Test asisten",
    "isAktif": true
  }'
```

---

## 📝 Notes

- All endpoints return standard `ApiResponse<T>` format (except PDF downloads)
- PDF endpoints return binary file stream
- Date parameters use ISO 8601 format: `yyyy-MM-dd`
- All IDs are UUIDs (GUID format)
- Authorization header required for all endpoints: `Bearer {token}`

---

**Last Updated:** January 2025
**Backend Version:** Latest (with all 5 features)

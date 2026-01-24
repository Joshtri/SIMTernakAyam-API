# Dokumentasi Frontend: Fitur Kandang Isolasi & Relokasi Ayam

## Daftar Isi
1. [Overview](#1-overview)
2. [Perubahan API yang Ada](#2-perubahan-api-yang-ada)
3. [API Baru - Relokasi](#3-api-baru---relokasi)
4. [TypeScript Interfaces](#4-typescript-interfaces)
5. [Komponen UI yang Perlu Dibuat](#5-komponen-ui-yang-perlu-dibuat)
6. [Alur Penggunaan](#6-alur-penggunaan)
7. [Contoh Implementasi](#7-contoh-implementasi)

---

## 1. Overview

### Apa itu Fitur Kandang Isolasi?

Fitur ini memungkinkan petugas untuk:
- Membuat kandang khusus bertipe "Isolasi" untuk ayam sakit
- Memindahkan (relokasi) ayam sakit dari kandang normal ke kandang isolasi
- Memindahkan ayam yang sudah pulih kembali ke kandang normal
- Tracking riwayat pemindahan ayam beserta catatan treatment

### Konsep Utama

```
┌─────────────────────────────────────────────────────────────┐
│                      ALUR RELOKASI                          │
└─────────────────────────────────────────────────────────────┘

  Kandang Normal                         Kandang Isolasi
  ┌─────────────┐                        ┌─────────────┐
  │ Batch: 1000 │  ──── SAKIT ────►      │ Batch: 5    │
  │ ekor        │       (5 ekor)         │ ekor        │
  │             │                        │             │
  │ Sisa: 995   │  ◄──── PULIH ────      │ Sisa: 2     │
  │ ekor        │       (3 ekor)         │ ekor        │
  └─────────────┘                        └─────────────┘
```

### Perubahan Stock Calculation

**Formula LAMA:**
```
SisaAyamHidup = JumlahMasuk - Mortalitas - Panen
```

**Formula BARU:**
```
SisaAyamHidup = JumlahMasuk - Mortalitas - Panen - Direlokasi
```

---

## 2. Perubahan API yang Ada

### 2.1. GET /api/kandangs - Response Berubah

**Response LAMA:**
```json
{
  "id": "guid",
  "namaKandang": "Kandang A",
  "kapasitas": 1000,
  "lokasi": "Blok A",
  "petugasId": "guid",
  "petugasNama": "John Doe",
  "jumlahAyamTerisi": 500,
  "kapasitasTersedia": 500,
  "persentaseTerisi": 50.00,
  "isKandangPenuh": false,
  "statusKapasitas": "Tersedia",
  "createdAt": "2026-01-01T00:00:00Z",
  "updateAt": "2026-01-01T00:00:00Z"
}
```

**Response BARU (+ 2 field):**
```json
{
  "id": "guid",
  "namaKandang": "Kandang A",
  "kapasitas": 1000,
  "lokasi": "Blok A",
  "petugasId": "guid",
  "petugasNama": "John Doe",
  "tipeKandang": "Normal",        // ⭐ BARU: "Normal" | "Isolasi"
  "tipeKandangNama": "Normal",    // ⭐ BARU: string display
  "jumlahAyamTerisi": 500,
  "kapasitasTersedia": 500,
  "persentaseTerisi": 50.00,
  "isKandangPenuh": false,
  "statusKapasitas": "Tersedia",
  "createdAt": "2026-01-01T00:00:00Z",
  "updateAt": "2026-01-01T00:00:00Z"
}
```

### 2.2. POST /api/kandangs - Payload Berubah

**Payload LAMA:**
```json
{
  "namaKandang": "Kandang A",
  "kapasitas": 1000,
  "lokasi": "Blok A",
  "petugasId": "guid"
}
```

**Payload BARU (+ 1 field opsional):**
```json
{
  "namaKandang": "Kandang Isolasi A",
  "kapasitas": 100,
  "lokasi": "Blok Isolasi",
  "petugasId": "guid",
  "tipeKandang": "Isolasi"    // ⭐ BARU: "Normal" (default) | "Isolasi"
}
```

> **Note:** Jika `tipeKandang` tidak diisi, default = "Normal"

### 2.3. PUT /api/kandangs/{id} - Payload Berubah

**Payload BARU:**
```json
{
  "id": "guid",
  "namaKandang": "Kandang Isolasi A",
  "kapasitas": 100,
  "lokasi": "Blok Isolasi",
  "petugasId": "guid",
  "tipeKandang": "Isolasi"    // ⭐ BARU
}
```

### 2.4. GET /api/ayams - Response Berubah

**Response LAMA:**
```json
{
  "id": "guid",
  "kandangId": "guid",
  "kandangNama": "Kandang A",
  "petugasKandangNama": "John Doe",
  "tanggalMasuk": "2026-01-01T00:00:00Z",
  "jumlahMasuk": 1000,
  "jumlahSudahDipanen": 200,
  "jumlahMortalitas": 50,
  "sisaAyamHidup": 750,
  "persentaseSurvival": 75.00,
  "persentaseDipanen": 20.00,
  "persentaseMortalitas": 5.00,
  "bisaDipanen": true,
  "perluPerhatianKesehatan": false,
  "createdAt": "2026-01-01T00:00:00Z",
  "updateAt": "2026-01-01T00:00:00Z"
}
```

**Response BARU (+ 1 field):**
```json
{
  "id": "guid",
  "kandangId": "guid",
  "kandangNama": "Kandang A",
  "petugasKandangNama": "John Doe",
  "tanggalMasuk": "2026-01-01T00:00:00Z",
  "jumlahMasuk": 1000,
  "jumlahSudahDipanen": 200,
  "jumlahMortalitas": 50,
  "jumlahDirelokasi": 10,       // ⭐ BARU: jumlah dipindahkan ke kandang lain
  "sisaAyamHidup": 740,         // ⭐ BERUBAH: sekarang dikurangi relokasi juga
  "persentaseSurvival": 74.00,
  "persentaseDipanen": 20.00,
  "persentaseMortalitas": 5.00,
  "bisaDipanen": true,
  "perluPerhatianKesehatan": false,
  "createdAt": "2026-01-01T00:00:00Z",
  "updateAt": "2026-01-01T00:00:00Z"
}
```

### 2.5. GET /api/ayams/{id} - Response Berubah

Sama seperti di atas, ada tambahan field `jumlahDirelokasi`.

---

## 3. API Baru - Relokasi

### 3.1. GET /api/relokasi

Mengambil semua data relokasi.

**Query Parameters (opsional):**
| Parameter | Type | Description |
|-----------|------|-------------|
| search | string | Cari berdasarkan nama kandang, petugas, catatan |
| kandangId | guid | Filter by kandang (asal atau tujuan) |

**Request:**
```http
GET /api/relokasi?search=isolasi&kandangId=xxx
Authorization: Bearer {token}
```

**Response:**
```json
{
  "success": true,
  "message": "Berhasil mengambil 5 data relokasi.",
  "data": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "kandangAsalId": "guid-kandang-normal",
      "kandangAsalNama": "Kandang A",
      "kandangAsalTipe": "Normal",
      "kandangTujuanId": "guid-kandang-isolasi",
      "kandangTujuanNama": "Kandang Isolasi A",
      "kandangTujuanTipe": "Isolasi",
      "ayamAsalId": "guid-batch-ayam",
      "ayamAsalJumlahMasuk": 1000,
      "ayamAsalTanggalMasuk": "2026-01-01T00:00:00Z",
      "ayamTujuanId": "guid-batch-baru-di-isolasi",
      "jumlahEkor": 5,
      "tanggalRelokasi": "2026-01-23T08:00:00Z",
      "alasanRelokasi": "Sakit",
      "alasanRelokasiNama": "Sakit",
      "statusRelokasi": "Selesai",
      "statusRelokasiNama": "Selesai",
      "catatan": "Gejala flu burung, diberi obat Amoxicillin",
      "petugasId": "guid-petugas",
      "petugasNama": "John Doe",
      "createdAt": "2026-01-23T08:00:00Z",
      "updateAt": "2026-01-23T08:00:00Z"
    }
  ],
  "statusCode": 200,
  "timestamp": "2026-01-23T08:30:00Z"
}
```

### 3.2. GET /api/relokasi/{id}

Mengambil detail relokasi berdasarkan ID.

**Request:**
```http
GET /api/relokasi/3fa85f64-5717-4562-b3fc-2c963f66afa6
Authorization: Bearer {token}
```

**Response:**
```json
{
  "success": true,
  "message": "Berhasil mengambil data relokasi.",
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "kandangAsalId": "guid-kandang-normal",
    "kandangAsalNama": "Kandang A",
    "kandangAsalTipe": "Normal",
    "kandangTujuanId": "guid-kandang-isolasi",
    "kandangTujuanNama": "Kandang Isolasi A",
    "kandangTujuanTipe": "Isolasi",
    "ayamAsalId": "guid-batch-ayam",
    "ayamAsalJumlahMasuk": 1000,
    "ayamAsalTanggalMasuk": "2026-01-01T00:00:00Z",
    "ayamTujuanId": "guid-batch-baru",
    "jumlahEkor": 5,
    "tanggalRelokasi": "2026-01-23T08:00:00Z",
    "alasanRelokasi": "Sakit",
    "alasanRelokasiNama": "Sakit",
    "statusRelokasi": "Selesai",
    "statusRelokasiNama": "Selesai",
    "catatan": "Gejala flu burung, diberi obat Amoxicillin",
    "petugasId": "guid-petugas",
    "petugasNama": "John Doe",
    "createdAt": "2026-01-23T08:00:00Z",
    "updateAt": "2026-01-23T08:00:00Z"
  },
  "statusCode": 200,
  "timestamp": "2026-01-23T08:30:00Z"
}
```

### 3.3. GET /api/relokasi/kandang/{kandangId}

Mengambil semua relokasi untuk kandang tertentu (baik sebagai asal maupun tujuan).

**Request:**
```http
GET /api/relokasi/kandang/guid-kandang-isolasi
Authorization: Bearer {token}
```

**Response:** Sama format dengan GET /api/relokasi

### 3.4. POST /api/relokasi ⭐ PENTING

Membuat relokasi baru (memindahkan ayam).

**Request:**
```http
POST /api/relokasi
Content-Type: application/json
Authorization: Bearer {token}
```

**Payload:**
```json
{
  "kandangAsalId": "guid-kandang-normal",
  "kandangTujuanId": "guid-kandang-isolasi",
  "ayamAsalId": "guid-batch-ayam-yang-dipindahkan",
  "jumlahEkor": 5,
  "tanggalRelokasi": "2026-01-23T08:00:00Z",
  "alasanRelokasi": "Sakit",
  "catatan": "Gejala flu burung, diberi obat Amoxicillin 3x sehari"
}
```

**Payload Fields:**

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| kandangAsalId | guid | Ya | ID kandang asal |
| kandangTujuanId | guid | Ya | ID kandang tujuan |
| ayamAsalId | guid | Ya | ID batch ayam yang dipindahkan |
| jumlahEkor | int | Ya | Jumlah ekor (min: 1, max: 100000) |
| tanggalRelokasi | datetime | Ya | Tanggal relokasi |
| alasanRelokasi | string | Ya | "Sakit", "Karantina", "Pulih", "Lainnya" |
| catatan | string | Tidak | Catatan/treatment (max: 1000 char) |

**Response Success (201 Created):**
```json
{
  "success": true,
  "message": "Relokasi berhasil dibuat.",
  "data": {
    "id": "guid-relokasi-baru",
    "kandangAsalId": "guid-kandang-normal",
    "kandangAsalNama": "Kandang A",
    "kandangAsalTipe": "Normal",
    "kandangTujuanId": "guid-kandang-isolasi",
    "kandangTujuanNama": "Kandang Isolasi A",
    "kandangTujuanTipe": "Isolasi",
    "ayamAsalId": "guid-batch-ayam",
    "ayamAsalJumlahMasuk": 1000,
    "ayamAsalTanggalMasuk": "2026-01-01T00:00:00Z",
    "ayamTujuanId": "guid-batch-baru-otomatis-dibuat",
    "jumlahEkor": 5,
    "tanggalRelokasi": "2026-01-23T08:00:00Z",
    "alasanRelokasi": "Sakit",
    "alasanRelokasiNama": "Sakit",
    "statusRelokasi": "Selesai",
    "statusRelokasiNama": "Selesai",
    "catatan": "Gejala flu burung, diberi obat Amoxicillin 3x sehari",
    "petugasId": "guid-petugas-login",
    "petugasNama": "John Doe",
    "createdAt": "2026-01-23T08:00:00Z",
    "updateAt": "2026-01-23T08:00:00Z"
  },
  "statusCode": 201,
  "timestamp": "2026-01-23T08:00:00Z"
}
```

**Response Error - Stok Tidak Cukup (400):**
```json
{
  "success": false,
  "message": "Stok ayam tidak mencukupi. Tersedia: 50 ekor, diminta: 100 ekor.",
  "data": null,
  "statusCode": 400,
  "timestamp": "2026-01-23T08:00:00Z"
}
```

**Response Error - Kapasitas Tidak Cukup (400):**
```json
{
  "success": false,
  "message": "Kapasitas kandang tujuan tidak mencukupi. Tersedia: 10 ekor, diminta: 50 ekor.",
  "data": null,
  "statusCode": 400,
  "timestamp": "2026-01-23T08:00:00Z"
}
```

**Response Error - Kandang Sama (400):**
```json
{
  "success": false,
  "message": "Kandang asal dan tujuan tidak boleh sama.",
  "data": null,
  "statusCode": 400,
  "timestamp": "2026-01-23T08:00:00Z"
}
```

**Response Error - Batch Tidak Di Kandang Asal (400):**
```json
{
  "success": false,
  "message": "Batch ayam tidak berada di kandang asal yang dipilih.",
  "data": null,
  "statusCode": 400,
  "timestamp": "2026-01-23T08:00:00Z"
}
```

### 3.5. PUT /api/relokasi/{id}

Update relokasi (hanya catatan dan status yang bisa diupdate).

**Request:**
```http
PUT /api/relokasi/guid-relokasi
Content-Type: application/json
Authorization: Bearer {token}
```

**Payload:**
```json
{
  "id": "guid-relokasi",
  "statusRelokasi": "Selesai",
  "catatan": "Update: Kondisi ayam membaik setelah 3 hari treatment"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Data relokasi berhasil diupdate.",
  "data": { ... },
  "statusCode": 200,
  "timestamp": "2026-01-23T10:00:00Z"
}
```

### 3.6. PUT /api/relokasi/{id}/batal

Batalkan relokasi (ubah status jadi "Dibatalkan").

> **Note:** Pembatalan TIDAK otomatis mengembalikan stok. Ini hanya menandai bahwa relokasi dibatalkan.

**Request:**
```http
PUT /api/relokasi/guid-relokasi/batal
Authorization: Bearer {token}
```

**Response:**
```json
{
  "success": true,
  "message": "Relokasi berhasil dibatalkan. Catatan: Stok tidak otomatis dikembalikan, silakan lakukan penyesuaian manual jika diperlukan.",
  "data": null,
  "statusCode": 200,
  "timestamp": "2026-01-23T10:00:00Z"
}
```

### 3.7. DELETE /api/relokasi/{id}

Hapus relokasi (hard delete).

**Request:**
```http
DELETE /api/relokasi/guid-relokasi
Authorization: Bearer {token}
```

**Response:**
```json
{
  "success": true,
  "message": "RelokasiAyam berhasil dihapus.",
  "data": null,
  "statusCode": 200,
  "timestamp": "2026-01-23T10:00:00Z"
}
```

---

## 4. TypeScript Interfaces

Copy-paste interfaces ini ke project frontend:

```typescript
// ============================================================
// ENUMS
// ============================================================

export type TipeKandang = "Normal" | "Isolasi";

export type AlasanRelokasi = "Sakit" | "Karantina" | "Pulih" | "Lainnya";

export type StatusRelokasi = "Pending" | "Selesai" | "Dibatalkan";

// ============================================================
// KANDANG (UPDATED)
// ============================================================

export interface KandangResponse {
  id: string;
  namaKandang: string;
  kapasitas: number;
  lokasi: string;
  petugasId: string;
  petugasNama: string | null;
  tipeKandang: TipeKandang;      // ⭐ BARU
  tipeKandangNama: string;        // ⭐ BARU
  jumlahAyamTerisi: number;
  kapasitasTersedia: number;
  persentaseTerisi: number;
  isKandangPenuh: boolean;
  statusKapasitas: string;
  createdAt: string;
  updateAt: string;
}

export interface CreateKandangDto {
  namaKandang: string;
  kapasitas: number;
  lokasi: string;
  petugasId: string;
  tipeKandang?: TipeKandang;      // ⭐ BARU (optional, default: Normal)
}

export interface UpdateKandangDto {
  id: string;
  namaKandang: string;
  kapasitas: number;
  lokasi: string;
  petugasId: string;
  tipeKandang: TipeKandang;       // ⭐ BARU
}

// ============================================================
// AYAM (UPDATED)
// ============================================================

export interface AyamResponse {
  id: string;
  kandangId: string;
  kandangNama: string | null;
  petugasKandangNama: string | null;
  tanggalMasuk: string;
  jumlahMasuk: number;
  jumlahSudahDipanen: number;
  jumlahMortalitas: number;
  jumlahDirelokasi: number;       // ⭐ BARU
  sisaAyamHidup: number;
  persentaseSurvival: number;
  persentaseDipanen: number;
  persentaseMortalitas: number;
  bisaDipanen: boolean;
  perluPerhatianKesehatan: boolean;
  createdAt: string;
  updateAt: string;
}

// ============================================================
// RELOKASI (BARU)
// ============================================================

export interface RelokasiResponse {
  id: string;
  kandangAsalId: string;
  kandangAsalNama: string;
  kandangAsalTipe: string;
  kandangTujuanId: string;
  kandangTujuanNama: string;
  kandangTujuanTipe: string;
  ayamAsalId: string;
  ayamAsalJumlahMasuk: number;
  ayamAsalTanggalMasuk: string;
  ayamTujuanId: string | null;
  jumlahEkor: number;
  tanggalRelokasi: string;
  alasanRelokasi: AlasanRelokasi;
  alasanRelokasiNama: string;
  statusRelokasi: StatusRelokasi;
  statusRelokasiNama: string;
  catatan: string | null;
  petugasId: string;
  petugasNama: string;
  createdAt: string;
  updateAt: string;
}

export interface CreateRelokasiDto {
  kandangAsalId: string;
  kandangTujuanId: string;
  ayamAsalId: string;
  jumlahEkor: number;
  tanggalRelokasi: string;
  alasanRelokasi: AlasanRelokasi;
  catatan?: string;
}

export interface UpdateRelokasiDto {
  id: string;
  statusRelokasi?: StatusRelokasi;
  catatan?: string;
}

// ============================================================
// API RESPONSE WRAPPER
// ============================================================

export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T;
  errors?: Record<string, string[]>;
  statusCode: number;
  timestamp: string;
}
```

---

## 5. Komponen UI yang Perlu Dibuat

### 5.1. Update Form Kandang

**Tambahkan dropdown/select untuk Tipe Kandang:**

```tsx
// Contoh React/Next.js
<FormField>
  <Label>Tipe Kandang</Label>
  <Select
    value={formData.tipeKandang}
    onChange={(e) => setFormData({...formData, tipeKandang: e.target.value})}
  >
    <option value="Normal">Normal (Produksi)</option>
    <option value="Isolasi">Isolasi (Ayam Sakit)</option>
  </Select>
</FormField>
```

### 5.2. Update List Kandang

**Tampilkan badge/label untuk tipe kandang:**

```tsx
// Contoh tampilan
<KandangCard>
  <h3>Kandang A</h3>
  <Badge color={kandang.tipeKandang === "Isolasi" ? "red" : "green"}>
    {kandang.tipeKandangNama}
  </Badge>
  <p>Kapasitas: {kandang.kapasitas}</p>
  <p>Terisi: {kandang.jumlahAyamTerisi}</p>
</KandangCard>
```

### 5.3. Update Detail Ayam

**Tampilkan jumlah yang direlokasi:**

```tsx
<StockInfo>
  <Row>
    <Label>Jumlah Masuk:</Label>
    <Value>{ayam.jumlahMasuk} ekor</Value>
  </Row>
  <Row>
    <Label>Dipanen:</Label>
    <Value>{ayam.jumlahSudahDipanen} ekor</Value>
  </Row>
  <Row>
    <Label>Mortalitas:</Label>
    <Value>{ayam.jumlahMortalitas} ekor</Value>
  </Row>
  <Row>
    <Label>Direlokasi:</Label>           {/* ⭐ BARU */}
    <Value>{ayam.jumlahDirelokasi} ekor</Value>
  </Row>
  <Divider />
  <Row>
    <Label>Sisa Hidup:</Label>
    <Value strong>{ayam.sisaAyamHidup} ekor</Value>
  </Row>
</StockInfo>
```

### 5.4. Halaman Relokasi (BARU)

**Komponen yang perlu dibuat:**

1. **RelokasiList** - Tabel list relokasi
2. **RelokasiDetail** - Detail relokasi
3. **RelokasiForm** - Form create relokasi
4. **RelokasiFilter** - Filter by kandang, status, alasan

### 5.5. Form Relokasi (BARU)

```tsx
interface RelokasiFormProps {
  onSubmit: (data: CreateRelokasiDto) => void;
  kandangList: KandangResponse[];
}

function RelokasiForm({ onSubmit, kandangList }: RelokasiFormProps) {
  const [formData, setFormData] = useState<CreateRelokasiDto>({
    kandangAsalId: "",
    kandangTujuanId: "",
    ayamAsalId: "",
    jumlahEkor: 1,
    tanggalRelokasi: new Date().toISOString(),
    alasanRelokasi: "Sakit",
    catatan: ""
  });

  const [ayamList, setAyamList] = useState<AyamResponse[]>([]);

  // Fetch ayam list when kandangAsalId changes
  useEffect(() => {
    if (formData.kandangAsalId) {
      fetchAyamByKandang(formData.kandangAsalId).then(setAyamList);
    }
  }, [formData.kandangAsalId]);

  return (
    <Form onSubmit={() => onSubmit(formData)}>
      {/* Kandang Asal */}
      <FormField>
        <Label>Kandang Asal *</Label>
        <Select
          value={formData.kandangAsalId}
          onChange={(e) => setFormData({...formData, kandangAsalId: e.target.value})}
        >
          <option value="">Pilih Kandang Asal</option>
          {kandangList.map(k => (
            <option key={k.id} value={k.id}>
              {k.namaKandang} ({k.tipeKandangNama})
            </option>
          ))}
        </Select>
      </FormField>

      {/* Kandang Tujuan */}
      <FormField>
        <Label>Kandang Tujuan *</Label>
        <Select
          value={formData.kandangTujuanId}
          onChange={(e) => setFormData({...formData, kandangTujuanId: e.target.value})}
        >
          <option value="">Pilih Kandang Tujuan</option>
          {kandangList
            .filter(k => k.id !== formData.kandangAsalId) // Exclude kandang asal
            .map(k => (
              <option key={k.id} value={k.id}>
                {k.namaKandang} ({k.tipeKandangNama}) - Tersedia: {k.kapasitasTersedia}
              </option>
            ))}
        </Select>
      </FormField>

      {/* Batch Ayam */}
      <FormField>
        <Label>Batch Ayam *</Label>
        <Select
          value={formData.ayamAsalId}
          onChange={(e) => setFormData({...formData, ayamAsalId: e.target.value})}
          disabled={!formData.kandangAsalId}
        >
          <option value="">Pilih Batch Ayam</option>
          {ayamList
            .filter(a => a.sisaAyamHidup > 0) // Only show batches with remaining stock
            .map(a => (
              <option key={a.id} value={a.id}>
                Masuk: {new Date(a.tanggalMasuk).toLocaleDateString()} -
                Sisa: {a.sisaAyamHidup} ekor
              </option>
            ))}
        </Select>
      </FormField>

      {/* Jumlah Ekor */}
      <FormField>
        <Label>Jumlah Ekor *</Label>
        <Input
          type="number"
          min={1}
          max={selectedAyam?.sisaAyamHidup || 100000}
          value={formData.jumlahEkor}
          onChange={(e) => setFormData({...formData, jumlahEkor: parseInt(e.target.value)})}
        />
        {selectedAyam && (
          <HelperText>Maksimal: {selectedAyam.sisaAyamHidup} ekor</HelperText>
        )}
      </FormField>

      {/* Tanggal Relokasi */}
      <FormField>
        <Label>Tanggal Relokasi *</Label>
        <Input
          type="datetime-local"
          value={formData.tanggalRelokasi.slice(0, 16)}
          onChange={(e) => setFormData({...formData, tanggalRelokasi: new Date(e.target.value).toISOString()})}
        />
      </FormField>

      {/* Alasan Relokasi */}
      <FormField>
        <Label>Alasan Relokasi *</Label>
        <Select
          value={formData.alasanRelokasi}
          onChange={(e) => setFormData({...formData, alasanRelokasi: e.target.value as AlasanRelokasi})}
        >
          <option value="Sakit">Sakit - Pindah ke Isolasi</option>
          <option value="Karantina">Karantina - Pencegahan</option>
          <option value="Pulih">Pulih - Kembali ke Normal</option>
          <option value="Lainnya">Lainnya</option>
        </Select>
      </FormField>

      {/* Catatan */}
      <FormField>
        <Label>Catatan / Treatment</Label>
        <Textarea
          value={formData.catatan || ""}
          onChange={(e) => setFormData({...formData, catatan: e.target.value})}
          placeholder="Contoh: Gejala flu burung, diberi obat Amoxicillin 3x sehari"
          maxLength={1000}
        />
      </FormField>

      <Button type="submit">Simpan Relokasi</Button>
    </Form>
  );
}
```

### 5.6. Tabel Relokasi (BARU)

```tsx
function RelokasiTable({ data }: { data: RelokasiResponse[] }) {
  return (
    <Table>
      <thead>
        <tr>
          <th>Tanggal</th>
          <th>Dari</th>
          <th>Ke</th>
          <th>Jumlah</th>
          <th>Alasan</th>
          <th>Status</th>
          <th>Petugas</th>
          <th>Aksi</th>
        </tr>
      </thead>
      <tbody>
        {data.map(r => (
          <tr key={r.id}>
            <td>{new Date(r.tanggalRelokasi).toLocaleDateString()}</td>
            <td>
              {r.kandangAsalNama}
              <Badge small>{r.kandangAsalTipe}</Badge>
            </td>
            <td>
              {r.kandangTujuanNama}
              <Badge small>{r.kandangTujuanTipe}</Badge>
            </td>
            <td>{r.jumlahEkor} ekor</td>
            <td>
              <Badge color={getAlasanColor(r.alasanRelokasi)}>
                {r.alasanRelokasiNama}
              </Badge>
            </td>
            <td>
              <Badge color={getStatusColor(r.statusRelokasi)}>
                {r.statusRelokasiNama}
              </Badge>
            </td>
            <td>{r.petugasNama}</td>
            <td>
              <Button onClick={() => viewDetail(r.id)}>Detail</Button>
              {r.statusRelokasi !== "Dibatalkan" && (
                <Button onClick={() => batalkan(r.id)}>Batalkan</Button>
              )}
            </td>
          </tr>
        ))}
      </tbody>
    </Table>
  );
}

function getAlasanColor(alasan: AlasanRelokasi): string {
  switch (alasan) {
    case "Sakit": return "red";
    case "Karantina": return "orange";
    case "Pulih": return "green";
    default: return "gray";
  }
}

function getStatusColor(status: StatusRelokasi): string {
  switch (status) {
    case "Selesai": return "green";
    case "Pending": return "yellow";
    case "Dibatalkan": return "red";
    default: return "gray";
  }
}
```

---

## 6. Alur Penggunaan

### Skenario 1: Ayam Sakit Dipindahkan ke Isolasi

```
1. Petugas melihat ada 5 ayam yang sakit di Kandang A

2. Petugas membuka halaman "Relokasi" > "Tambah Relokasi"

3. Petugas mengisi form:
   - Kandang Asal: Kandang A (Normal)
   - Kandang Tujuan: Kandang Isolasi A (Isolasi)
   - Batch Ayam: Batch masuk 1 Jan 2026 (sisa 500 ekor)
   - Jumlah: 5 ekor
   - Alasan: Sakit
   - Catatan: "Gejala flu burung, diberi obat Amoxicillin"

4. Sistem melakukan:
   - Validasi stok batch asal (500 >= 5 ✓)
   - Validasi kapasitas kandang tujuan (tersedia >= 5 ✓)
   - Mengurangi stok batch asal: 500 - 5 = 495 ekor
   - Membuat batch baru di kandang isolasi: 5 ekor
   - Menyimpan record relokasi

5. Hasil:
   - Kandang A: Batch asal sekarang sisa 495 ekor
   - Kandang Isolasi A: Ada batch baru dengan 5 ekor
   - Relokasi tercatat dengan status "Selesai"
```

### Skenario 2: Ayam Pulih Dikembalikan ke Normal

```
1. Setelah 5 hari, 3 dari 5 ayam sudah pulih

2. Petugas membuka halaman "Relokasi" > "Tambah Relokasi"

3. Petugas mengisi form:
   - Kandang Asal: Kandang Isolasi A (Isolasi)
   - Kandang Tujuan: Kandang B (Normal) atau Kandang A
   - Batch Ayam: Batch 5 ekor yang di isolasi
   - Jumlah: 3 ekor
   - Alasan: Pulih
   - Catatan: "Kondisi sudah sehat, sudah selesai masa karantina"

4. Hasil:
   - Kandang Isolasi: Batch sekarang sisa 2 ekor
   - Kandang B: Ada batch baru dengan 3 ekor
   - Relokasi tercatat dengan alasan "Pulih"
```

### Skenario 3: Ayam Mati di Isolasi

```
1. 2 ayam di isolasi mati

2. Petugas TIDAK perlu relokasi, cukup input Mortalitas seperti biasa:
   - Buka halaman Mortalitas > Tambah
   - Pilih batch ayam di kandang isolasi
   - Input: 2 ekor mati, penyebab: flu burung

3. Hasil:
   - Batch di isolasi: 5 - 3 (pulih) - 2 (mati) = 0 ekor
```

---

## 7. Contoh Implementasi

### 7.1. Service API (React/Next.js)

```typescript
// services/relokasiService.ts

import axios from 'axios';
import { ApiResponse, RelokasiResponse, CreateRelokasiDto, UpdateRelokasiDto } from '@/types';

const API_URL = process.env.NEXT_PUBLIC_API_URL;

export const relokasiService = {
  // Get all relokasi
  getAll: async (search?: string, kandangId?: string): Promise<RelokasiResponse[]> => {
    const params = new URLSearchParams();
    if (search) params.append('search', search);
    if (kandangId) params.append('kandangId', kandangId);

    const response = await axios.get<ApiResponse<RelokasiResponse[]>>(
      `${API_URL}/api/relokasi?${params.toString()}`
    );
    return response.data.data;
  },

  // Get by ID
  getById: async (id: string): Promise<RelokasiResponse> => {
    const response = await axios.get<ApiResponse<RelokasiResponse>>(
      `${API_URL}/api/relokasi/${id}`
    );
    return response.data.data;
  },

  // Get by kandang
  getByKandang: async (kandangId: string): Promise<RelokasiResponse[]> => {
    const response = await axios.get<ApiResponse<RelokasiResponse[]>>(
      `${API_URL}/api/relokasi/kandang/${kandangId}`
    );
    return response.data.data;
  },

  // Create
  create: async (data: CreateRelokasiDto): Promise<RelokasiResponse> => {
    const response = await axios.post<ApiResponse<RelokasiResponse>>(
      `${API_URL}/api/relokasi`,
      data
    );
    return response.data.data;
  },

  // Update
  update: async (id: string, data: UpdateRelokasiDto): Promise<RelokasiResponse> => {
    const response = await axios.put<ApiResponse<RelokasiResponse>>(
      `${API_URL}/api/relokasi/${id}`,
      data
    );
    return response.data.data;
  },

  // Batalkan
  batalkan: async (id: string): Promise<void> => {
    await axios.put(`${API_URL}/api/relokasi/${id}/batal`);
  },

  // Delete
  delete: async (id: string): Promise<void> => {
    await axios.delete(`${API_URL}/api/relokasi/${id}`);
  }
};
```

### 7.2. Update Kandang Service

```typescript
// services/kandangService.ts - UPDATE

export interface CreateKandangDto {
  namaKandang: string;
  kapasitas: number;
  lokasi: string;
  petugasId: string;
  tipeKandang?: "Normal" | "Isolasi";  // ⭐ TAMBAHKAN INI
}

// ... rest of the service
```

### 7.3. React Hook Example

```typescript
// hooks/useRelokasi.ts

import { useState, useEffect } from 'react';
import { relokasiService } from '@/services/relokasiService';
import { RelokasiResponse, CreateRelokasiDto } from '@/types';

export function useRelokasi(kandangId?: string) {
  const [data, setData] = useState<RelokasiResponse[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fetchData = async () => {
    setLoading(true);
    setError(null);
    try {
      const result = kandangId
        ? await relokasiService.getByKandang(kandangId)
        : await relokasiService.getAll();
      setData(result);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Terjadi kesalahan');
    } finally {
      setLoading(false);
    }
  };

  const createRelokasi = async (dto: CreateRelokasiDto) => {
    setLoading(true);
    setError(null);
    try {
      const result = await relokasiService.create(dto);
      await fetchData(); // Refresh data
      return result;
    } catch (err: any) {
      const message = err.response?.data?.message || 'Gagal membuat relokasi';
      setError(message);
      throw new Error(message);
    } finally {
      setLoading(false);
    }
  };

  const batalkanRelokasi = async (id: string) => {
    setLoading(true);
    try {
      await relokasiService.batalkan(id);
      await fetchData();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Gagal membatalkan relokasi');
      throw err;
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchData();
  }, [kandangId]);

  return {
    data,
    loading,
    error,
    refetch: fetchData,
    createRelokasi,
    batalkanRelokasi
  };
}
```

---

## Checklist Frontend

- [ ] Update interface/type untuk Kandang (tambah tipeKandang)
- [ ] Update interface/type untuk Ayam (tambah jumlahDirelokasi)
- [ ] Buat interface/type untuk Relokasi
- [ ] Update form Create Kandang (tambah dropdown tipeKandang)
- [ ] Update form Edit Kandang (tambah dropdown tipeKandang)
- [ ] Update tampilan list Kandang (tampilkan badge tipe)
- [ ] Update tampilan detail Ayam (tampilkan jumlahDirelokasi)
- [ ] Buat service API untuk Relokasi
- [ ] Buat halaman List Relokasi
- [ ] Buat halaman Detail Relokasi
- [ ] Buat form Create Relokasi
- [ ] Buat tombol Batalkan Relokasi
- [ ] Tambahkan menu navigasi ke halaman Relokasi
- [ ] Handle error responses dari API

---

## Pertanyaan?

Jika ada pertanyaan terkait implementasi frontend, silakan tanyakan ke tim backend.

**Kontak:**
- Backend Developer: [nama]
- API Documentation: `/swagger` (development only)

# ?? API HARGA PASAR - ANALISIS KEUNTUNGAN

## ?? **ENDPOINT BARU YANG TELAH DITAMBAHKAN**

### 1. **?? Estimasi Keuntungan (Calculator)**
```http
GET /api/harga_pasar/hitung-keuntungan?totalAyam={jumlah}&beratRataRata={berat}&tanggalPanen={tanggal}
```
**Response:**
```json
{
  "success": true,
  "message": "Estimasi keuntungan berhasil dihitung",
  "data": {
    "totalAyam": 1000,
    "beratRataRata": 1.5,
    "totalBerat": 1500.0,
    "hargaPerKg": 26000,
    "totalPendapatan": 39000000,
    "tanggalReferensi": "2024-12-14T00:00:00",
    "hargaPasarInfo": {
      "id": "guid-here",
      "hargaPerKg": 26000,
      "tanggalMulai": "2024-12-01T00:00:00",
      "tanggalBerakhir": null,
      "wilayah": "Jakarta",
      "keterangan": "Harga Desember 2024"
    }
  }
}
```

### 2. **?? Keuntungan dari Data Panen**
```http
GET /api/harga_pasar/keuntungan-panen/{panenId}
```
**Response:**
```json
{
  "success": true,
  "message": "Keuntungan panen berhasil dihitung",
  "data": {
    "panenId": "guid-here",
    "tanggalPanen": "2024-12-01T00:00:00",
    "jumlahAyam": 250,
    "totalBerat": 375.0,
    "beratRataRata": 1.5,
    "hargaPerKg": 26000,
    "totalPendapatan": 9750000,
    "namaKandang": "Kandang A1",
    "hargaPasarInfo": { /* detail harga pasar */ }
  }
}
```

### 3. **?? Laporan Keuntungan Bulanan**
```http
GET /api/harga_pasar/laporan-keuntungan-bulanan?tahun=2024&bulan=12
```
**Response:**
```json
{
  "success": true,
  "message": "Laporan keuntungan bulan 12/2024 berhasil diambil",
  "data": {
    "tahun": 2024,
    "bulan": 12,
    "namaBulan": "Desember",
    "periode": {
      "tanggalMulai": "2024-12-01T00:00:00",
      "tanggalAkhir": "2024-12-31T00:00:00",
      "jumlahHari": 31
    },
    "total": {
      "totalPanen": 15,
      "totalAyam": 5000,
      "totalBerat": 7500.0,
      "totalPendapatan": 195000000,
      "rataRataBeratPerEkor": 1.5
    },
    "detailHarian": [
      {
        "tanggal": "2024-12-01T00:00:00",
        "jumlahPanen": 2,
        "totalAyam": 500,
        "totalBerat": 750.0,
        "totalKeuntungan": 19500000,
        "hargaPerKg": 26000,
        "detailPanen": [/* array detail panen */]
      }
    ],
    "perbandinganBulanSebelumnya": {
      "bulanSebelumnya": "11/2024",
      "totalKeuntunganSebelumnya": 180000000,
      "selisihKeuntungan": 15000000,
      "persentasePerubahan": 8.33,
      "statusPerubahan": "Naik"
    },
    "hargaPasarBulanIni": [/* array harga pasar yang digunakan */],
    "rataRataHargaPerKg": 26000,
    "fluktusiHarga": {
      "hargaTerendah": 25000,
      "hargaTertinggi": 27000,
      "selisihHarga": 2000,
      "persentaseFluktuasi": 8.0
    }
  }
}
```

### 4. **?? Laporan Keuntungan Mingguan**
```http
GET /api/harga_pasar/laporan-keuntungan-mingguan?tahun=2024&mingguKe=50
```
**Response:** (Similar structure to monthly, but for weekly periods)

### 5. **?? Ringkasan Keuntungan Tahunan**
```http
GET /api/harga_pasar/ringkasan-keuntungan-tahunan?tahun=2024
```
**Response:**
```json
{
  "success": true,
  "message": "Ringkasan keuntungan tahun 2024 berhasil diambil",
  "data": {
    "tahun": 2024,
    "totalTahunan": {
      "totalPanen": 180,
      "totalAyam": 50000,
      "totalBerat": 75000.0,
      "totalPendapatan": 1950000000,
      "rataRataBeratPerEkor": 1.5
    },
    "breakdownBulanan": [
      {
        "bulan": 1,
        "namaBulan": "Januari",
        "totalKeuntungan": 160000000,
        "totalAyam": 4000,
        "totalBerat": 6000.0,
        "jumlahHariPanen": 15,
        "rataRataHargaPerKg": 25000
      }
      /* ... data untuk 12 bulan */
    ],
    "bulanTerbaik": {
      "bulan": 12,
      "namaBulan": "Desember",
      "totalKeuntungan": 195000000,
      "totalAyam": 5000,
      "totalBerat": 7500.0,
      "jumlahHariPanen": 20,
      "rataRataHargaPerKg": 26000
    },
    "bulanTerburuk": { /* bulan dengan keuntungan terendah */ },
    "rataRataKeuntunganPerBulan": 162500000,
    "trendTahunan": "Naik",
    "fluktusiHargaTahunan": {
      "hargaTerendah": 23000,
      "hargaTertinggi": 28000,
      "selisihHarga": 5000,
      "persentaseFluktuasi": 21.74
    }
  }
}
```

## ?? **FITUR FRONTEND YANG BISA DIIMPLEMENTASI**

### 1. **Dashboard Keuntungan**
```jsx
// Profit Summary Cards
<div className="profit-dashboard">
  <Card title="Keuntungan Bulan Ini">
    <h2>Rp 195.000.000</h2>
    <Badge variant="success">+8.33% vs bulan lalu</Badge>
  </Card>
  
  <Card title="Total Panen Bulan Ini">
    <h2>15 kali panen</h2>
    <p>5,000 ekor ayam</p>
  </Card>
  
  <Card title="Harga Pasar Saat Ini">
    <h2>Rp 26.000/kg</h2>
    <p>Berlaku sejak 1 Des 2024</p>
  </Card>
</div>
```

### 2. **Profit Calculator Widget**
```jsx
<div className="profit-calculator">
  <h3>?? Kalkulator Keuntungan</h3>
  <input type="number" placeholder="Jumlah Ayam" value={totalAyam} />
  <input type="number" placeholder="Berat Rata-rata (kg)" value={beratRataRata} />
  <input type="date" placeholder="Tanggal Panen" value={tanggalPanen} />
  
  <button onClick={hitungKeuntungan}>Hitung Estimasi</button>
  
  {result && (
    <div className="result-card">
      <h4>Estimasi Keuntungan</h4>
      <p>Total Berat: {result.totalBerat} kg</p>
      <p>Harga Pasar: Rp {result.hargaPerKg:N0}/kg</p>
      <h3>Total Pendapatan: Rp {result.totalPendapatan:N0}</h3>
    </div>
  )}
</div>
```

### 3. **Monthly Profit Charts**
```jsx
// Chart keuntungan bulanan
<div className="monthly-charts">
  <BarChart 
    data={laporanBulanan.detailHarian}
    xKey="tanggal"
    yKey="totalKeuntungan"
    title="Keuntungan Harian dalam Bulan"
  />
  
  <LineChart
    data={ringkasanTahunan.breakdownBulanan}
    xKey="namaBulan"
    yKey="totalKeuntungan"
    title="Trend Keuntungan Sepanjang Tahun"
  />
</div>
```

### 4. **Enhanced Panen List**
```jsx
// Tabel panen dengan kolom keuntungan
<table className="panen-table">
  <thead>
    <tr>
      <th>Tanggal</th>
      <th>Kandang</th>
      <th>Jumlah Ayam</th>
      <th>Total Berat</th>
      <th>Harga Pasar</th>
      <th>?? Keuntungan</th> {/* KOLOM BARU */}
    </tr>
  </thead>
  <tbody>
    {panenList.map(panen => (
      <tr key={panen.id}>
        <td>{formatDate(panen.tanggalPanen)}</td>
        <td>{panen.namaKandang}</td>
        <td>{panen.jumlahAyam} ekor</td>
        <td>{panen.totalBerat} kg</td>
        <td>Rp {panen.hargaPerKg:N0}</td>
        <td className="profit-cell">
          <strong>Rp {panen.totalPendapatan:N0}</strong>
        </td>
      </tr>
    ))}
  </tbody>
</table>
```

### 5. **Profit Reports Page**
```jsx
// Halaman dedicated untuk laporan keuntungan
<div className="profit-reports">
  <div className="report-filters">
    <select value={reportType}>
      <option value="monthly">Bulanan</option>
      <option value="weekly">Mingguan</option>
      <option value="yearly">Tahunan</option>
    </select>
    
    <input type="month" value={selectedMonth} />
    <button onClick={generateReport}>Generate Report</button>
  </div>
  
  <div className="report-content">
    {reportType === 'monthly' && <MonthlyReport data={monthlyData} />}
    {reportType === 'yearly' && <YearlyReport data={yearlyData} />}
  </div>
</div>
```

## ?? **SUMMARY - ENDPOINT BARU**

| No | Endpoint | Method | Fungsi | Response Key Data |
|----|----------|--------|--------|-------------------|
| 1 | `/hitung-keuntungan` | GET | Calculator keuntungan simulasi | `totalPendapatan`, `hargaPerKg` |
| 2 | `/keuntungan-panen/{id}` | GET | Keuntungan dari panen specific | `totalPendapatan`, `namaKandang` |
| 3 | `/laporan-keuntungan-bulanan` | GET | Laporan detail per bulan | `detailHarian`, `perbandinganBulanSebelumnya` |
| 4 | `/laporan-keuntungan-mingguan` | GET | Laporan detail per minggu | `detailHarian`, `periode` |
| 5 | `/ringkasan-keuntungan-tahunan` | GET | Summary tahunan + breakdown bulanan | `breakdownBulanan`, `bulanTerbaik`, `trendTahunan` |

## ?? **NEXT STEPS UNTUK FRONTEND**

1. **High Priority**: Implement Profit Calculator widget di dashboard
2. **Medium Priority**: Add keuntungan column di Panen list
3. **Medium Priority**: Create Monthly/Weekly profit charts
4. **Low Priority**: Build dedicated Profit Reports page
5. **Future**: Add profit predictions & trend analysis

Semua endpoint sudah siap dan tested! ??
# Frontend Guide - Kandang Asisten Ayam Sisa Feature

## Overview
Guide ini untuk frontend developer yang ingin mengimplementasikan fitur menampilkan ayam sisa pada halaman Kandang Asisten.

## API Endpoints

### 1. Get Kandang Asisten (Original - Tanpa Ayam Sisa)
```http
GET /api/kandang-asistens/by-kandang/{kandangId}
```

**Response:**
```json
{
  "success": true,
  "message": "Data asisten kandang berhasil diambil",
  "data": [
    {
      "id": "guid",
      "kandangId": "guid",
      "kandangNama": "Kandang A",
      "asistenId": "guid",
      "asistenNama": "John Doe",
      "asistenEmail": "john@example.com",
      "asistenNoWA": "081234567890",
      "catatan": "Asisten shift pagi",
      "isAktif": true,
      "createdAt": "2024-01-01T00:00:00Z",
      "updateAt": "2024-01-01T00:00:00Z"
    }
  ]
}
```

### 2. Get Kandang Asisten WITH Ayam Sisa (NEW) ?
```http
GET /api/kandang-asistens/by-kandang/{kandangId}/with-ayam-sisa
```

**Response:**
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

## TypeScript/JavaScript Types

### TypeScript Interface
```typescript
// Ayam Sisa Info
interface AyamSisaInfo {
  id: string;
  tanggalMasuk: string; // ISO 8601 date string
  jumlahMasukAwal: number;
  alasanSisa: string | null;
  tanggalDitandaiSisa: string | null; // ISO 8601 date string
  isAyamSisa: boolean;
  umurAyam: number; // Age in days
}

// Kandang Asisten Response (Standard)
interface KandangAsistenResponse {
  id: string;
  kandangId: string;
  kandangNama: string | null;
  asistenId: string;
  asistenNama: string | null;
  asistenEmail: string | null;
  asistenNoWA: string | null;
  catatan: string | null;
  isAktif: boolean;
  createdAt: string; // ISO 8601 date string
  updateAt: string; // ISO 8601 date string
}

// Kandang Asisten with Ayam Sisa (Extended)
interface KandangAsistenWithAyamSisaResponse extends KandangAsistenResponse {
  ayamSisaList: AyamSisaInfo[] | null;
  totalAyamSisa: number;
}

// API Response Wrapper
interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T;
}
```

## Frontend Implementation Examples

### Example 1: React/Next.js Component

```tsx
import { useState, useEffect } from 'react';
import axios from 'axios';

interface AyamSisaInfo {
  id: string;
  tanggalMasuk: string;
  jumlahMasukAwal: number;
  alasanSisa: string | null;
  tanggalDitandaiSisa: string | null;
  isAyamSisa: boolean;
  umurAyam: number;
}

interface KandangAsistenWithAyamSisa {
  id: string;
  kandangId: string;
  kandangNama: string | null;
  asistenId: string;
  asistenNama: string | null;
  asistenEmail: string | null;
  asistenNoWA: string | null;
  catatan: string | null;
  isAktif: boolean;
  createdAt: string;
  updateAt: string;
  ayamSisaList: AyamSisaInfo[] | null;
  totalAyamSisa: number;
}

const KandangAsistenPage = ({ kandangId }: { kandangId: string }) => {
  const [data, setData] = useState<KandangAsistenWithAyamSisa[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    fetchKandangAsistenWithAyamSisa();
  }, [kandangId]);

  const fetchKandangAsistenWithAyamSisa = async () => {
    try {
      setLoading(true);
      const response = await axios.get(
        `https://localhost:7195/api/kandang-asistens/by-kandang/${kandangId}/with-ayam-sisa`
      );
      
      if (response.data.success) {
        setData(response.data.data);
      } else {
        setError(response.data.message);
      }
    } catch (err) {
      setError('Failed to fetch data');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  if (loading) return <div>Loading...</div>;
  if (error) return <div>Error: {error}</div>;

  return (
    <div className="kandang-asisten-container">
      <h1>Kandang Asisten & Ayam Sisa</h1>
      
      {data.map((asisten) => (
        <div key={asisten.id} className="asisten-card">
          <h2>{asisten.asistenNama}</h2>
          <p>Kandang: {asisten.kandangNama}</p>
          <p>Status: {asisten.isAktif ? 'Aktif' : 'Tidak Aktif'}</p>
          <p>Contact: {asisten.asistenNoWA}</p>
          
          {/* Display Ayam Sisa Section */}
          <div className="ayam-sisa-section">
            <h3>Ayam Sisa</h3>
            <p>Total: {asisten.totalAyamSisa} ekor</p>
            
            {asisten.ayamSisaList && asisten.ayamSisaList.length > 0 ? (
              <table>
                <thead>
                  <tr>
                    <th>Tanggal Masuk</th>
                    <th>Jumlah</th>
                    <th>Umur (Hari)</th>
                    <th>Alasan Sisa</th>
                  </tr>
                </thead>
                <tbody>
                  {asisten.ayamSisaList.map((ayam) => (
                    <tr key={ayam.id}>
                      <td>{new Date(ayam.tanggalMasuk).toLocaleDateString()}</td>
                      <td>{ayam.jumlahMasukAwal}</td>
                      <td>{ayam.umurAyam}</td>
                      <td>{ayam.alasanSisa || 'N/A'}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            ) : (
              <p>Tidak ada ayam sisa</p>
            )}
          </div>
        </div>
      ))}
    </div>
  );
};

export default KandangAsistenPage;
```

### Example 2: Vanilla JavaScript (Fetch API)

```javascript
async function loadKandangAsistenWithAyamSisa(kandangId) {
  const loader = document.getElementById('loader');
  const container = document.getElementById('asisten-container');
  
  try {
    loader.style.display = 'block';
    
    const response = await fetch(
      `https://localhost:7195/api/kandang-asistens/by-kandang/${kandangId}/with-ayam-sisa`
    );
    
    const result = await response.json();
    
    if (!result.success) {
      throw new Error(result.message);
    }
    
    renderAsistenList(result.data, container);
  } catch (error) {
    console.error('Error loading data:', error);
    container.innerHTML = `<div class="error">Error: ${error.message}</div>`;
  } finally {
    loader.style.display = 'none';
  }
}

function renderAsistenList(asistens, container) {
  container.innerHTML = '';
  
  asistens.forEach(asisten => {
    const card = document.createElement('div');
    card.className = 'asisten-card';
    
    card.innerHTML = `
      <h2>${asisten.asistenNama}</h2>
      <p><strong>Kandang:</strong> ${asisten.kandangNama}</p>
      <p><strong>Status:</strong> ${asisten.isAktif ? 'Aktif' : 'Tidak Aktif'}</p>
      <p><strong>Contact:</strong> ${asisten.asistenNoWA || 'N/A'}</p>
      
      <div class="ayam-sisa-section">
        <h3>Ayam Sisa (${asisten.totalAyamSisa} ekor)</h3>
        ${renderAyamSisaTable(asisten.ayamSisaList)}
      </div>
    `;
    
    container.appendChild(card);
  });
}

function renderAyamSisaTable(ayamSisaList) {
  if (!ayamSisaList || ayamSisaList.length === 0) {
    return '<p>Tidak ada ayam sisa</p>';
  }
  
  const rows = ayamSisaList.map(ayam => `
    <tr>
      <td>${new Date(ayam.tanggalMasuk).toLocaleDateString('id-ID')}</td>
      <td>${ayam.jumlahMasukAwal}</td>
      <td>${ayam.umurAyam}</td>
      <td>${ayam.alasanSisa || 'N/A'}</td>
    </tr>
  `).join('');
  
  return `
    <table>
      <thead>
        <tr>
          <th>Tanggal Masuk</th>
          <th>Jumlah</th>
          <th>Umur (Hari)</th>
          <th>Alasan Sisa</th>
        </tr>
      </thead>
      <tbody>
        ${rows}
      </tbody>
    </table>
  `;
}

// Usage
document.addEventListener('DOMContentLoaded', () => {
  const kandangId = 'fd082b84-41c8-45d4-8480-ddb918f74cca';
  loadKandangAsistenWithAyamSisa(kandangId);
});
```

### Example 3: Vue.js Component

```vue
<template>
  <div class="kandang-asisten-page">
    <h1>Kandang Asisten & Ayam Sisa</h1>
    
    <div v-if="loading" class="loading">Loading...</div>
    <div v-else-if="error" class="error">{{ error }}</div>
    
    <div v-else class="asisten-list">
      <div 
        v-for="asisten in data" 
        :key="asisten.id" 
        class="asisten-card"
      >
        <h2>{{ asisten.asistenNama }}</h2>
        <p><strong>Kandang:</strong> {{ asisten.kandangNama }}</p>
        <p><strong>Status:</strong> 
          <span :class="asisten.isAktif ? 'badge-success' : 'badge-inactive'">
            {{ asisten.isAktif ? 'Aktif' : 'Tidak Aktif' }}
          </span>
        </p>
        
        <div class="ayam-sisa-section">
          <h3>Ayam Sisa ({{ asisten.totalAyamSisa }} ekor)</h3>
          
          <div v-if="asisten.ayamSisaList && asisten.ayamSisaList.length > 0">
            <table class="ayam-sisa-table">
              <thead>
                <tr>
                  <th>Tanggal Masuk</th>
                  <th>Jumlah</th>
                  <th>Umur (Hari)</th>
                  <th>Alasan Sisa</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="ayam in asisten.ayamSisaList" :key="ayam.id">
                  <td>{{ formatDate(ayam.tanggalMasuk) }}</td>
                  <td>{{ ayam.jumlahMasukAwal }}</td>
                  <td>{{ ayam.umurAyam }}</td>
                  <td>{{ ayam.alasanSisa || 'N/A' }}</td>
                </tr>
              </tbody>
            </table>
          </div>
          <p v-else>Tidak ada ayam sisa</p>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import axios from 'axios';

export default {
  name: 'KandangAsistenPage',
  props: {
    kandangId: {
      type: String,
      required: true
    }
  },
  data() {
    return {
      data: [],
      loading: false,
      error: null
    };
  },
  mounted() {
    this.fetchData();
  },
  methods: {
    async fetchData() {
      try {
        this.loading = true;
        this.error = null;
        
        const response = await axios.get(
          `https://localhost:7195/api/kandang-asistens/by-kandang/${this.kandangId}/with-ayam-sisa`
        );
        
        if (response.data.success) {
          this.data = response.data.data;
        } else {
          this.error = response.data.message;
        }
      } catch (err) {
        this.error = 'Gagal memuat data';
        console.error(err);
      } finally {
        this.loading = false;
      }
    },
    formatDate(dateString) {
      return new Date(dateString).toLocaleDateString('id-ID');
    }
  }
};
</script>

<style scoped>
.asisten-card {
  border: 1px solid #ddd;
  padding: 20px;
  margin-bottom: 20px;
  border-radius: 8px;
}

.ayam-sisa-section {
  margin-top: 20px;
  padding: 15px;
  background-color: #f9f9f9;
  border-radius: 5px;
}

.ayam-sisa-table {
  width: 100%;
  border-collapse: collapse;
}

.ayam-sisa-table th,
.ayam-sisa-table td {
  padding: 10px;
  text-align: left;
  border-bottom: 1px solid #ddd;
}

.badge-success {
  color: green;
  font-weight: bold;
}

.badge-inactive {
  color: gray;
}
</style>
```

## UI/UX Recommendations

### 1. Visual Indicators
```
?? Ayam Sisa < 30 hari (Normal)
?? Ayam Sisa 30-60 hari (Perlu Perhatian)
?? Ayam Sisa > 60 hari (Urgent - Segera Panen)
```

### 2. Display Format
- Show `totalAyamSisa` prominently as badge/chip
- Use color coding based on age (`umurAyam`)
- Display `alasanSisa` as tooltip or expandable section
- Format dates consistently (use Indonesian locale)

### 3. Actions
- Button to view full ayam details (link to `/ayam/{id}`)
- Button to create panen for ayam sisa
- Filter/sort by age, quantity, or reason

## Best Practices

1. **Error Handling**: Always wrap API calls in try-catch
2. **Loading States**: Show loading indicators during fetch
3. **Empty States**: Handle cases where `ayamSisaList` is null or empty
4. **Date Formatting**: Use `Intl.DateTimeFormat` or library like `date-fns`
5. **Performance**: Consider pagination if ayam sisa list is large
6. **Accessibility**: Use semantic HTML and ARIA labels

## Common Pitfalls

? **Don't:**
- Forget to check `ayamSisaList` for null
- Assume `alasanSisa` is always present
- Display raw ISO dates without formatting

? **Do:**
- Always check API response `success` field
- Handle null/empty states gracefully
- Format dates for Indonesian locale
- Show meaningful error messages

## Testing Checklist

- [ ] Test with kandang that has ayam sisa
- [ ] Test with kandang that has NO ayam sisa
- [ ] Test with invalid kandangId (should show error)
- [ ] Test loading state display
- [ ] Test date formatting
- [ ] Test responsive design on mobile

## Support & Documentation

- Backend Documentation: `SIMTernakAyam\Docs\KandangAsistenAyamSisaFeature.md`
- API Tests: `SIMTernakAyam\Tests\KandangAsistenAyamSisaTests.http`
- Backend Support: Contact backend team for API issues

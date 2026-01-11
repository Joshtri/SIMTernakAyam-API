# ?? Frontend Implementation Guide: Panen Manual Split

## ?? Quick Overview

Fitur **Manual Split** untuk Panen memungkinkan user menentukan distribusi panen antara:
- **Ayam Lama** (sisa periode sebelumnya)
- **Ayam Baru** (periode baru)

---

## ?? What Frontend Needs to Build

### 1. **Form with Mode Selection**

User dapat memilih antara 2 mode:
- `auto-fifo`: Sistem otomatis (masih available)
- `manual-split`: User tentukan sendiri (RECOMMENDED)

### 2. **Slider/Input untuk Distribusi**

Ketika mode `manual-split` dipilih, tampilkan:
- Slider untuk `jumlahDariAyamLama`
- Auto-calculate `jumlahDariAyamBaru` berdasarkan total

### 3. **Real-time Validation**

- Total harus sama dengan `jumlahEkorPanen`
- Tidak boleh melebihi stok yang tersedia
- Tidak boleh negatif

### 4. **Stock Information Display**

Tampilkan info stok untuk kedua grup ayam:
- Jumlah hidup
- Tanggal masuk
- Umur ayam (optional)

---

## ?? API Endpoint

### POST /api/panens

**Request Body:**
```json
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

**Response (Success):**
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

**Response (Error):**
```json
{
  "status": "error",
  "message": "Total JumlahDariAyamLama (25) + JumlahDariAyamBaru (20) = 45 harus sama dengan JumlahEkorPanen (50).",
  "data": null
}
```

---

## ?? Code Examples

### 1. TypeScript Interface

```typescript
// types/panen.ts

export interface PanenForm {
  kandangId: string;
  tanggalPanen: Date;
  jumlahEkorPanen: number;
  beratRataRata: number;
  mode: 'auto-fifo' | 'manual-split';
  jumlahDariAyamLama?: number;
  jumlahDariAyamBaru?: number;
}

export interface AyamInfo {
  jumlahHidup: number;
  tanggalMasuk: string;
  isAyamSisa: boolean;
}

export interface PanenResponse {
  id: string;
  ayamId: string;
  jumlahEkorPanen: number;
  beratRataRata: number;
  totalBeratKg: number;
  tanggalPanen: string;
}
```

### 2. React Component (Complete Example)

```tsx
import React, { useState, useEffect } from 'react';
import { PanenForm, AyamInfo } from '@/types/panen';
import { createPanen, getAyamInfoByKandang } from '@/services/panenService';

const PanenCreateForm: React.FC = () => {
  const [form, setForm] = useState<PanenForm>({
    kandangId: '',
    tanggalPanen: new Date(),
    jumlahEkorPanen: 0,
    beratRataRata: 1.5,
    mode: 'manual-split',
    jumlahDariAyamLama: 0,
    jumlahDariAyamBaru: 0,
  });

  const [ayamLamaInfo, setAyamLamaInfo] = useState<AyamInfo | null>(null);
  const [ayamBaruInfo, setAyamBaruInfo] = useState<AyamInfo | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Fetch ayam info when kandang changes
  useEffect(() => {
    if (!form.kandangId) return;

    const fetchAyamInfo = async () => {
      try {
        const { ayamLama, ayamBaru } = await getAyamInfoByKandang(form.kandangId);
        setAyamLamaInfo(ayamLama);
        setAyamBaruInfo(ayamBaru);
      } catch (err) {
        console.error('Failed to fetch ayam info:', err);
      }
    };

    fetchAyamInfo();
  }, [form.kandangId]);

  // Handle slider change for ayam lama
  const handleAyamLamaChange = (value: number) => {
    const remaining = form.jumlahEkorPanen - value;
    setForm({
      ...form,
      jumlahDariAyamLama: value,
      jumlahDariAyamBaru: remaining,
    });
  };

  // Validate form
  const isValidForm = (): boolean => {
    if (!form.kandangId || form.jumlahEkorPanen <= 0) return false;
    if (form.beratRataRata < 0.01 || form.beratRataRata > 100) return false;

    if (form.mode === 'manual-split') {
      const total = (form.jumlahDariAyamLama || 0) + (form.jumlahDariAyamBaru || 0);
      return total === form.jumlahEkorPanen;
    }

    return true;
  };

  // Submit handler
  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setLoading(true);

    try {
      const response = await createPanen(form);
      alert(response.message);
      // Reset form or redirect
    } catch (err: any) {
      setError(err.message || 'Terjadi kesalahan');
    } finally {
      setLoading(false);
    }
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-6 max-w-2xl mx-auto p-6">
      <h2 className="text-2xl font-bold">Tambah Panen Baru</h2>

      {/* Error Display */}
      {error && (
        <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded">
          {error}
        </div>
      )}

      {/* Basic Fields */}
      <div className="space-y-4">
        <div>
          <label className="block text-sm font-medium mb-2">Kandang</label>
          <select
            value={form.kandangId}
            onChange={(e) => setForm({ ...form, kandangId: e.target.value })}
            className="w-full px-4 py-2 border rounded-lg"
            required
          >
            <option value="">Pilih Kandang</option>
            {/* Add kandang options */}
          </select>
        </div>

        <div>
          <label className="block text-sm font-medium mb-2">Tanggal Panen</label>
          <input
            type="date"
            value={form.tanggalPanen.toISOString().split('T')[0]}
            onChange={(e) => setForm({ ...form, tanggalPanen: new Date(e.target.value) })}
            className="w-full px-4 py-2 border rounded-lg"
            required
          />
        </div>

        <div className="grid grid-cols-2 gap-4">
          <div>
            <label className="block text-sm font-medium mb-2">Total Ekor</label>
            <input
              type="number"
              value={form.jumlahEkorPanen || ''}
              onChange={(e) => setForm({ ...form, jumlahEkorPanen: parseInt(e.target.value) || 0 })}
              className="w-full px-4 py-2 border rounded-lg"
              min="1"
              required
            />
          </div>

          <div>
            <label className="block text-sm font-medium mb-2">Berat Rata-rata (kg)</label>
            <input
              type="number"
              step="0.01"
              value={form.beratRataRata || ''}
              onChange={(e) => setForm({ ...form, beratRataRata: parseFloat(e.target.value) || 0 })}
              className="w-full px-4 py-2 border rounded-lg"
              min="0.01"
              max="100"
              required
            />
          </div>
        </div>
      </div>

      {/* Mode Selection */}
      <div>
        <label className="block text-sm font-medium mb-3">Mode Pencatatan</label>
        <div className="space-y-2">
          <label className="flex items-center space-x-3 p-3 border rounded-lg cursor-pointer hover:bg-gray-50">
            <input
              type="radio"
              value="auto-fifo"
              checked={form.mode === 'auto-fifo'}
              onChange={(e) => setForm({ ...form, mode: e.target.value as any })}
            />
            <div>
              <p className="font-medium">Auto FIFO</p>
              <p className="text-sm text-gray-600">Sistem otomatis pilih ayam</p>
            </div>
          </label>

          <label className="flex items-center space-x-3 p-3 border rounded-lg cursor-pointer hover:bg-gray-50 border-blue-500 bg-blue-50">
            <input
              type="radio"
              value="manual-split"
              checked={form.mode === 'manual-split'}
              onChange={(e) => setForm({ ...form, mode: e.target.value as any })}
            />
            <div>
              <p className="font-medium text-blue-900">Manual Split ? Recommended</p>
              <p className="text-sm text-blue-700">Tentukan sendiri distribusi</p>
            </div>
          </label>
        </div>
      </div>

      {/* Manual Split Section */}
      {form.mode === 'manual-split' && (
        <div className="space-y-6 bg-blue-50 p-6 rounded-lg">
          <h3 className="font-semibold text-blue-900">Distribusi Panen Manual</h3>

          {/* Ayam Lama */}
          {ayamLamaInfo && (
            <div className="bg-white p-4 rounded-lg space-y-3">
              <div className="flex justify-between items-start">
                <div>
                  <h4 className="font-medium">Ayam Lama (Periode Sisa)</h4>
                  <p className="text-sm text-gray-600">
                    Masuk: {new Date(ayamLamaInfo.tanggalMasuk).toLocaleDateString('id-ID')}
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
                  max={Math.min(form.jumlahEkorPanen, ayamLamaInfo.jumlahHidup)}
                  value={form.jumlahDariAyamLama || 0}
                  onChange={(e) => handleAyamLamaChange(parseInt(e.target.value))}
                  className="w-full h-2 bg-gray-200 rounded-lg cursor-pointer accent-blue-600"
                />
                <div className="flex justify-between text-sm text-gray-600">
                  <span>0 ekor</span>
                  <span className="font-bold text-blue-600">
                    {form.jumlahDariAyamLama || 0} ekor
                  </span>
                  <span>{Math.min(form.jumlahEkorPanen, ayamLamaInfo.jumlahHidup)} ekor</span>
                </div>
              </div>
            </div>
          )}

          {/* Ayam Baru */}
          {ayamBaruInfo && (
            <div className="bg-white p-4 rounded-lg space-y-3">
              <div className="flex justify-between items-start">
                <div>
                  <h4 className="font-medium">Ayam Baru (Periode Baru)</h4>
                  <p className="text-sm text-gray-600">
                    Masuk: {new Date(ayamBaruInfo.tanggalMasuk).toLocaleDateString('id-ID')}
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
                <p className="text-2xl font-bold">{form.jumlahDariAyamBaru || 0} ekor</p>
              </div>
            </div>
          )}

          {/* Total Summary */}
          <div className="bg-gradient-to-r from-blue-100 to-green-100 p-4 rounded-lg border-2 border-blue-300">
            <div className="flex justify-between items-center">
              <span className="font-medium">Total Panen</span>
              <div className="text-right">
                <p className="text-3xl font-bold text-blue-900">
                  {(form.jumlahDariAyamLama || 0) + (form.jumlahDariAyamBaru || 0)} ekor
                </p>
                <p className="text-sm text-gray-600">Target: {form.jumlahEkorPanen} ekor</p>
              </div>
            </div>

            {isValidForm() ? (
              <div className="mt-2 flex items-center text-green-600">
                <svg className="w-5 h-5 mr-2" fill="currentColor" viewBox="0 0 20 20">
                  <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clipRule="evenodd" />
                </svg>
                <span className="font-medium">Distribusi valid</span>
              </div>
            ) : (
              <div className="mt-2 flex items-center text-red-600">
                <svg className="w-5 h-5 mr-2" fill="currentColor" viewBox="0 0 20 20">
                  <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clipRule="evenodd" />
                </svg>
                <span className="font-medium">Total harus sama dengan target</span>
              </div>
            )}
          </div>
        </div>
      )}

      {/* Submit Button */}
      <button
        type="submit"
        disabled={!isValidForm() || loading}
        className={`w-full py-3 rounded-lg font-medium ${
          isValidForm() && !loading
            ? 'bg-blue-600 text-white hover:bg-blue-700'
            : 'bg-gray-300 text-gray-500 cursor-not-allowed'
        }`}
      >
        {loading ? 'Menyimpan...' : 'Simpan Panen'}
      </button>
    </form>
  );
};

export default PanenCreateForm;
```

### 3. Service Functions

```typescript
// services/panenService.ts

import { PanenForm, PanenResponse, AyamInfo } from '@/types/panen';

const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5000';

export const createPanen = async (data: PanenForm) => {
  const response = await fetch(`${API_BASE_URL}/api/panens`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${getToken()}`,
    },
    body: JSON.stringify({
      kandangId: data.kandangId,
      tanggalPanen: data.tanggalPanen.toISOString(),
      jumlahEkorPanen: data.jumlahEkorPanen,
      beratRataRata: data.beratRataRata,
      mode: data.mode,
      jumlahDariAyamLama: data.jumlahDariAyamLama,
      jumlahDariAyamBaru: data.jumlahDariAyamBaru,
    }),
  });

  if (!response.ok) {
    const error = await response.json();
    throw new Error(error.message || 'Gagal membuat panen');
  }

  return await response.json();
};

export const getAyamInfoByKandang = async (kandangId: string) => {
  const response = await fetch(`${API_BASE_URL}/api/ayam/kandang/${kandangId}`, {
    headers: {
      'Authorization': `Bearer ${getToken()}`,
    },
  });

  if (!response.ok) {
    throw new Error('Gagal mengambil data ayam');
  }

  const data = await response.json();
  const ayamList = data.data;

  // Separate ayam lama and ayam baru
  const ayamLama = ayamList.filter((a: any) => a.isAyamSisa);
  const ayamBaru = ayamList.filter((a: any) => !a.isAyamSisa);

  return {
    ayamLama: ayamLama.length > 0 ? {
      jumlahHidup: ayamLama.reduce((sum: number, a: any) => sum + a.jumlahHidup, 0),
      tanggalMasuk: ayamLama[0].tanggalMasuk,
      isAyamSisa: true,
    } : null,
    ayamBaru: ayamBaru.length > 0 ? {
      jumlahHidup: ayamBaru.reduce((sum: number, a: any) => sum + a.jumlahHidup, 0),
      tanggalMasuk: ayamBaru[0].tanggalMasuk,
      isAyamSisa: false,
    } : null,
  };
};

function getToken(): string {
  // Get token from localStorage or cookie
  return localStorage.getItem('token') || '';
}
```

---

## ? Validation Checklist

### Client-Side Validation
- [ ] Kandang selected
- [ ] Jumlah ekor > 0
- [ ] Berat rata-rata between 0.01 - 100 kg
- [ ] Tanggal panen not in future
- [ ] If manual-split: total split equals total ekor
- [ ] If manual-split: no negative values
- [ ] If manual-split: not exceed available stock

### Server-Side Validation (Handled by API)
- [ ] Kandang exists
- [ ] Ayam lama/baru exists in kandang
- [ ] Stock availability check
- [ ] Total validation
- [ ] Business logic validation

---

## ?? UI/UX Best Practices

1. **Show Available Stock Prominently**
   - Display current stock for both ayam groups
   - Show tanggal masuk for context
   - Update info when kandang changes

2. **Interactive Slider**
   - Use range slider for better UX
   - Auto-calculate remaining amount
   - Show min/max values clearly

3. **Real-time Feedback**
   - Validate as user types/slides
   - Show green checkmark when valid
   - Show red error when invalid

4. **Loading States**
   - Show spinner while submitting
   - Disable form during submission
   - Clear feedback after success

5. **Error Handling**
   - Display API errors clearly
   - Highlight invalid fields
   - Provide actionable error messages

---

## ?? Quick Start

1. Copy the React component code
2. Create service functions
3. Add TypeScript types
4. Style with Tailwind or your CSS framework
5. Test with your API endpoint

---

## ?? Additional Resources

- Full Documentation: `Docs/PanenManualSplitFeature.md`
- API Tests: `Tests/PanenManualSplitTests.http`
- Similar Feature: Mortalitas Manual Split

---

## ?? Support

Jika ada pertanyaan atau butuh bantuan implementasi, silakan hubungi tim backend development.

**Happy Coding! ??**

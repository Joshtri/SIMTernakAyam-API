# 🎨 Frontend Implementation Todos

> Backend API sudah selesai dibuat. Berikut adalah checklist untuk implementasi frontend berdasarkan 5 fitur baru yang telah dibuat.

---

## 📋 Feature 1: Asisten Kandang (Cage Assistant Management)

### API Endpoints Available:
- `GET /api/kandang-asistens` - Get all assistants
- `GET /api/kandang-asistens/{id}` - Get assistant by ID
- `GET /api/kandang-asistens/by-kandang/{kandangId}` - Get all assistants for a kandang
- `GET /api/kandang-asistens/by-kandang/{kandangId}/active` - Get active assistants only
- `GET /api/kandang-asistens/by-asisten/{asistenId}` - Get kandang assignments for an asisten
- `POST /api/kandang-asistens` - Create new assistant assignment
- `PUT /api/kandang-asistens/{id}` - Update assistant assignment
- `DELETE /api/kandang-asistens/{id}` - Delete assistant assignment

### Request/Response DTOs:
```typescript
// CreateKandangAsistenDto
{
  kandangId: string;      // UUID
  asistenId: string;      // UUID (must be Petugas role)
  catatan?: string;       // Optional notes
  isAktif: boolean;       // Default: true
}

// UpdateKandangAsistenDto
{
  catatan?: string;
  isAktif?: boolean;
}

// KandangAsistenResponseDto
{
  id: string;
  kandangId: string;
  kandangNama: string;
  asistenId: string;
  asistenNama: string;
  catatan?: string;
  isAktif: boolean;
  createdAt: string;
  updatedAt: string;
}
```

### Frontend Changes Needed:

#### ✅ Todo Items:
- [ ] **Create Kandang Assistant Management Page**
  - [ ] Create new page/component for managing kandang assistants
  - [ ] Add data table showing all assistant assignments with columns: Kandang, Asisten, Status (Aktif/Tidak Aktif), Catatan
  - [ ] Add filters: by Kandang, by Status (Aktif/Tidak Aktif)
  - [ ] Add search functionality

- [ ] **Create Add/Edit Assistant Form**
  - [ ] Create form modal/dialog with fields:
    - Kandang dropdown (select kandang)
    - Asisten dropdown (only show users with role "Petugas", exclude main officer)
    - Catatan textarea (optional)
    - IsAktif checkbox/toggle
  - [ ] Implement form validation
  - [ ] Handle success/error responses

- [ ] **Update Kandang Detail Page**
  - [ ] Add "Asisten" section showing list of assigned assistants
  - [ ] Show active assistants prominently
  - [ ] Add quick action buttons: Add Asisten, Edit, Deactivate
  - [ ] Display catatan if exists

- [ ] **Optional: Add to Dashboard**
  - [ ] Show widget "Kandang dengan Asisten" count
  - [ ] Show "Asisten Aktif Hari Ini" list

---

## 🧪 Feature 2: Vaksin & Vitamin Merge

### API Endpoints Available:
- `GET /api/vaksins/by-type/{type}` - Get by type (Vaksin or Vitamin)
  - Type values: `Vaksin`, `Vitamin`
- All existing vaksin endpoints still work

### Request/Response DTOs:
```typescript
// VaksinVitaminTypeEnum
enum VaksinVitaminType {
  Vaksin = 0,
  Vitamin = 1
}

// CreateVaksinDto (Modified)
{
  namaVaksin: string;     // Now can be vaksin or vitamin name
  jenis: string;
  stok: number;
  satuan: string;
  hargaPerSatuan: number;
  tipe: VaksinVitaminType;  // NEW FIELD: 0 = Vaksin, 1 = Vitamin
}

// VaksinResponseDto (Modified)
{
  id: string;
  namaVaksin: string;
  jenis: string;
  stok: number;
  satuan: string;
  hargaPerSatuan: number;
  tipe: VaksinVitaminType;  // NEW FIELD
  createdAt: string;
  updatedAt: string;
}
```

### Frontend Changes Needed:

#### ✅ Todo Items:
- [ ] **Update Vaksin Management Page**
  - [ ] Rename page from "Vaksin" to "Vaksin & Vitamin"
  - [ ] Add tab/toggle to switch between "Vaksin" and "Vitamin" view
  - [ ] Update table to filter by tipe when tab is switched
  - [ ] Add badge/chip showing type (Vaksin/Vitamin) in table

- [ ] **Update Create/Edit Vaksin Form**
  - [ ] Add "Tipe" radio button or dropdown (Vaksin/Vitamin)
  - [ ] Update form labels dynamically based on tipe selected
    - If Vaksin: "Nama Vaksin", "Jenis Vaksin"
    - If Vitamin: "Nama Vitamin", "Jenis Vitamin"
  - [ ] Update form validation to include tipe field

- [ ] **Update API Calls**
  - [ ] Modify GET requests to filter by type using `/api/vaksins/by-type/{type}`
  - [ ] Include `tipe` field in CREATE and UPDATE requests

- [ ] **Update Operasional Form**
  - [ ] When selecting Vaksin/Vitamin in operasional, show both types
  - [ ] Add visual indicator (icon/badge) to differentiate vaksin vs vitamin

---

## 💰 Feature 3: Biaya Operasional Bulanan (Monthly Operational Costs)

### API Endpoints Available:
- `GET /api/biayas/rekap-bulanan/{bulan}/{tahun}` - Get monthly recap
  - Example: `/api/biayas/rekap-bulanan/1/2025` (January 2025)
- All existing biaya endpoints still work

### Request/Response DTOs:
```typescript
// CreateBiayaDto (Modified - NEW FIELDS)
{
  jenisBiaya: string;
  jumlah: number;
  petugasId: string;
  tanggal: string;
  keterangan?: string;
  kandangId?: string;     // NEW: Optional kandang ID
  catatan?: string;       // NEW: Optional notes
  bulan?: number;         // NEW: Month (1-12)
  tahun?: number;         // NEW: Year
}

// BiayaBulananResponseDto (NEW)
{
  bulan: number;
  tahun: number;
  totalBiaya: number;
  detailPerKandang: BiayaKandangDetailDto[];
}

// BiayaKandangDetailDto (NEW)
{
  kandangId?: string;
  kandangNama: string;
  totalBiaya: number;
  detailBiaya: BiayaItemDto[];
}

// BiayaItemDto (NEW)
{
  jenisBiaya: string;
  jumlah: number;
  tanggal: string;
  keterangan?: string;
  catatan?: string;
}
```

### Frontend Changes Needed:

#### ✅ Todo Items:
- [ ] **Create Monthly Costs Page (Biaya Bulanan)**
  - [ ] Create new page "Rekap Biaya Bulanan"
  - [ ] Add month/year selector (dropdown or date picker)
  - [ ] Display total biaya for selected month
  - [ ] Show breakdown per kandang in cards or table
  - [ ] Show detail biaya items for each kandang

- [ ] **Update Biaya Create/Edit Form**
  - [ ] Add optional "Kandang" dropdown field
  - [ ] Add optional "Bulan" and "Tahun" fields
  - [ ] Add optional "Catatan" textarea field
  - [ ] Auto-populate bulan/tahun from selected tanggal
  - [ ] Show help text: "Untuk biaya listrik/air, pilih kandang dan isi bulan/tahun"

- [ ] **Add to Navigation Menu**
  - [ ] Add menu item "Biaya Bulanan" under Keuangan/Biaya section
  - [ ] Add breadcrumb navigation

- [ ] **Optional: Dashboard Widget**
  - [ ] Show "Biaya Bulan Ini" total
  - [ ] Show chart/graph of monthly costs trend

---

## 📄 Feature 4: Laporan Operasional — PDF Export

### API Endpoints Available:
- `GET /api/laporan/operasional/pdf/{kandangId}?startDate={date}&endDate={date}` - Download PDF
  - Example: `/api/laporan/operasional/pdf/{guid}?startDate=2025-01-01&endDate=2025-01-31`
  - Dates are optional (defaults to current month)

### Response:
- File download: `application/pdf`
- Filename: `Laporan_Operasional_YYYYMMDD_YYYYMMDD.pdf`

### PDF Contents:
- Header: Kandang info, Period, Petugas, Print date
- Summary cards: Total Pakan, Total Vaksin, Total Biaya
- Detail table: Tanggal, Kegiatan, Jumlah, Item, Biaya
- Catatan Pengeluaran section (if exists)

### Frontend Changes Needed:

#### ✅ Todo Items:
- [ ] **Add to Laporan Operasional Page**
  - [ ] Add "Download PDF" button on laporan operasional view
  - [ ] Add date range picker for PDF export
  - [ ] Add kandang selector if multiple kandang
  - [ ] Show loading state while generating PDF
  - [ ] Handle PDF download (trigger file download in browser)

- [ ] **Create PDF Preview/Download Modal (Optional)**
  - [ ] Show preview before download (if needed)
  - [ ] Show PDF generation progress
  - [ ] Add option to email PDF (future enhancement)

- [ ] **Add Quick Actions**
  - [ ] Add "Export to PDF" icon button in data table actions
  - [ ] Add bulk export option (multiple kandang)

---

## 🏥 Feature 5: Laporan Kesehatan Ayam — PDF Export

### API Endpoints Available:
- `GET /api/laporan/kesehatan/pdf/{kandangId}?startDate={date}&endDate={date}` - Download PDF
  - Example: `/api/laporan/kesehatan/pdf/{guid}?startDate=2025-01-01&endDate=2025-01-31`
  - Dates are optional (defaults to current month)

### Response:
- File download: `application/pdf`
- Filename: `Laporan_Kesehatan_Ayam_YYYYMMDD_YYYYMMDD.pdf`

### PDF Contents:
- Header: Kandang info, Period, Total Ayam, Print date
- Summary cards: Total Mortalitas, Persentase, Total Vaksinasi, Status Kesehatan
- Riwayat Mortalitas table (if exists)
- Riwayat Vaksinasi table (if exists)
- Rekomendasi section (if exists)

### Frontend Changes Needed:

#### ✅ Todo Items:
- [ ] **Add to Laporan Kesehatan Page**
  - [ ] Add "Download PDF" button on kesehatan laporan view
  - [ ] Add date range picker for PDF export
  - [ ] Add kandang selector
  - [ ] Show loading state while generating PDF
  - [ ] Handle PDF download

- [ ] **Create Unified Report Export Component (Reusable)**
  - [ ] Create reusable "ExportPDFButton" component
  - [ ] Props: reportType, kandangId, dateRange
  - [ ] Handle both operasional and kesehatan reports
  - [ ] Show success/error toast notifications

- [ ] **Add to Dashboard**
  - [ ] Add "Quick Reports" section with PDF export shortcuts
  - [ ] Show "Download Laporan Bulan Ini" buttons

---

## 🔒 Authorization Notes

All new endpoints require:
- **Authentication**: Bearer token required
- **Authorization**: Only `Operator` and `Pemilik` roles can access
- Handle 403 Forbidden responses gracefully in frontend

---

## 🎯 General Frontend Tasks

### Common Updates Needed Across All Features:

- [ ] **API Service Layer**
  - [ ] Create/update TypeScript interfaces for all new DTOs
  - [ ] Add API functions for all new endpoints
  - [ ] Add proper error handling

- [ ] **State Management**
  - [ ] Add state for kandang assistants (if using Redux/Zustand)
  - [ ] Add state for monthly costs
  - [ ] Add loading/error states for PDF downloads

- [ ] **Navigation**
  - [ ] Update sidebar menu with new pages
  - [ ] Add breadcrumbs for new pages
  - [ ] Update routing configuration

- [ ] **Testing**
  - [ ] Test all forms with validation
  - [ ] Test role-based access (Operator/Pemilik only)
  - [ ] Test PDF downloads in different browsers
  - [ ] Test date range selections
  - [ ] Test error scenarios (network errors, 403, 404, etc.)

- [ ] **UI/UX Polish**
  - [ ] Add loading spinners for async operations
  - [ ] Add success/error toast notifications
  - [ ] Add confirmation dialogs for delete actions
  - [ ] Ensure mobile responsiveness
  - [ ] Add help text/tooltips where needed

---

## 📝 Summary of Changes

### Pages to Create:
1. **Kandang Assistant Management** - `/kandang-asistens`
2. **Biaya Bulanan (Monthly Costs Recap)** - `/biaya/bulanan`

### Pages to Modify:
1. **Vaksin Management** - Add tipe filter and form field
2. **Biaya Management** - Add kandang, bulan, tahun, catatan fields
3. **Laporan Operasional** - Add PDF export button
4. **Laporan Kesehatan** - Add PDF export button
5. **Kandang Detail** - Add asisten section
6. **Dashboard** (Optional) - Add widgets for new features

### New Components to Create:
1. `KandangAsistenForm` - Add/Edit assistant form
2. `KandangAsistenTable` - List of assistants
3. `ExportPDFButton` - Reusable PDF export button
4. `MonthYearSelector` - Month/year picker for biaya bulanan
5. `BiayaBulananCard` - Display monthly cost summary per kandang

---

## 🚀 Recommended Implementation Order

1. **Start with simplest**: Vaksin & Vitamin merge (just add filter + form field)
2. **Then**: PDF Exports (just add download buttons)
3. **Then**: Biaya Bulanan (new page + form updates)
4. **Finally**: Kandang Asisten (new CRUD pages + integration with kandang)

---

✍️ **Notes:**
- All API endpoints are already tested and working on backend
- Make sure to handle authorization (only Operator/Pemilik can access laporan endpoints)
- Use consistent UI components across all new features
- Test with different roles (Petugas should not see laporan features)
- Consider adding loading states for better UX during PDF generation

Good luck with the frontend implementation! 🎉

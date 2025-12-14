# ?? QUICK FIX CARD: Laporan Keuntungan Kosong

## ? **PROBLEM**
```json
{
  "totalPendapatan": 0,      // ? SHOULD BE FILLED
  "hargaPerKg": 0,           // ? SHOULD BE FILLED
  "hargaPasarInfo": null     // ? SHOULD BE FILLED
}
```

## ?? **ROOT CAUSE**
No active market price (`HargaPasar`) for harvest date **2025-12-10**

## ? **SOLUTION** (3 steps, 2 minutes)

### 1?? Deactivate Old Prices
```http
POST https://localhost:7195/api/harga_pasar/deactivate-all
Authorization: Bearer YOUR_TOKEN
```

### 2?? Create New Market Price
```http
POST https://localhost:7195/api/harga_pasar
Content-Type: application/json
Authorization: Bearer YOUR_TOKEN

{
  "hargaPerKg": 27000,
  "tanggalMulai": "2025-12-01T00:00:00Z",
  "tanggalBerakhir": null,
  "keterangan": "Harga Desember 2025",
  "wilayah": "Jakarta",
  "isAktif": true
}
```

### 3?? Test Again
```http
GET https://localhost:7195/api/harga_pasar/laporan-keuntungan-bulanan?tahun=2025&bulan=12
Authorization: Bearer YOUR_TOKEN
```

## ?? **EXPECTED RESULT**
```json
{
  "totalPendapatan": 167918400,  // ? FILLED: Rp 167,918,400
  "hargaPerKg": 27000,           // ? FILLED: Rp 27,000/kg
  "hargaPasarInfo": {            // ? FILLED
    "hargaPerKg": 27000,
    "tanggalMulai": "2025-12-01T00:00:00"
  }
}
```

## ?? **CALCULATION**
| Panen | Ayam | Berat (kg) | Harga/kg | Pendapatan |
|-------|------|------------|----------|------------|
| #1 | 990 | 2,079.00 | 27,000 | 56,133,000 |
| #2 | 981 | 2,158.20 | 27,000 | 58,271,400 |
| #3 | 991 | 1,982.00 | 27,000 | 53,514,000 |
| **TOTAL** | **2,962** | **6,219.20** | | **167,918,400** |

## ?? **DEBUG COMMANDS**

```http
# Check if market price exists for harvest date
GET https://localhost:7195/api/harga_pasar/debug/cek-harga-pasar?tanggal=2025-12-10

# Check current active market price
GET https://localhost:7195/api/harga_pasar/terbaru

# Check harvest data
GET https://localhost:7195/api/panen
```

## ?? **KEY RULES**

? **DO:**
- Keep **ONLY 1** market price with `IsAktif = true` at a time
- Use `TanggalBerakhir = null` for ongoing prices
- Always check active price before viewing profit reports

? **DON'T:**
- Have multiple active prices (`IsAktif = true`) simultaneously
- Set `TanggalBerakhir` before harvest dates
- Forget to activate newly created prices

## ?? **FULL DOCS**

- ?? **TROUBLESHOOTING_LAPORAN_KEUNTUNGAN_KOSONG.md** - Detailed troubleshooting guide
- ?? **LAPORAN_KEUNTUNGAN_KOSONG_SUMMARY.md** - Complete solution summary
- ?? **VISUAL_DIAGRAM_LAPORAN_KEUNTUNGAN.md** - Visual diagrams & flow charts
- ?? **HargaPasarKeuntunganTests.http** - Full test suite

## ? **ONE-LINER FIX**

For the impatient:
```bash
# Just run this sequence in your .http file:
POST /api/harga_pasar/deactivate-all
? POST /api/harga_pasar {hargaPerKg:27000, tanggalMulai:"2025-12-01", tanggalBerakhir:null, isAktif:true}
? GET /api/harga_pasar/laporan-keuntungan-bulanan?tahun=2025&bulan=12
```

---

**Created:** 2025-12-14  
**Fix Time:** 2 minutes  
**Status:** ? Ready to Apply

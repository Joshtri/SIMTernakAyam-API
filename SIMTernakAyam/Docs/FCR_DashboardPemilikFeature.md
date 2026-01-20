# ?? FCR (Feed Conversion Ratio) Enhancement for Dashboard Pemilik

## ?? Deskripsi Fitur

Menambahkan **FCR (Feed Conversion Ratio)** ke Dashboard Pemilik sebagai metrik penting untuk mengukur efisiensi penggunaan pakan.

### **Apa itu FCR?**
```
FCR = Total Pakan Dikonsumsi (kg) / Total Berat Ayam yang Dihasilkan (kg)
```

**Semakin rendah FCR, semakin baik efisiensi pakan.**

### **Kategori FCR:**
- **Excellent**: < 1.6
- **Good**: 1.6 - 1.8
- **Fair**: 1.8 - 2.0
- **Poor**: > 2.0

---

## ? Perubahan Yang Diperlukan

### **1. Update DTOs - `DashboardDtos.cs`**

#### A. `BusinessKpiDto` - Tambahkan FCR Property

```csharp
public class BusinessKpiDto
{
    public decimal MonthlyRevenue { get; set; }
    public decimal MonthlyProfit { get; set; }
    public double ReturnOnInvestment { get; set; }
    public int TotalAyamStock { get; set; }
    public double AverageProductivity { get; set; }
    
    /// <summary>
    /// ? BARU: Feed Conversion Ratio (FCR) - Rasio konversi pakan
    /// Semakin rendah semakin baik (ideal: 1.5-1.8)
    /// FCR = Total Pakan (kg) / Total Berat Ayam (kg)
    /// </summary>
    public double FeedConversionRatio { get; set; }
    
    public double CustomerSatisfaction { get; set; }
    public int MarketShare { get; set; }
}
```

#### B. `BenchmarkDto` - Tambahkan FCR Comparison

```csharp
public class BenchmarkDto
{
    public double IndustryAvgMortalityRate { get; set; }
    public double YourMortalityRate { get; set; }
    public double IndustryAvgProductivity { get; set; }
    public double YourProductivity { get; set; }
    
    /// <summary>
    /// ? BARU: Industry average FCR
    /// Standard industri: 1.6 - 1.8
    /// </summary>
    public double IndustryAvgFcr { get; set; }
    
    /// <summary>
    /// ? BARU: Your actual FCR
    /// </summary>
    public double YourFcr { get; set; }
    
    public string PerformanceRating { get; set; } = string.Empty;
}
```

#### C. `MonthlyTrendsDto` - Tambahkan FCR Trend

```csharp
public class MonthlyTrendsDto
{
    public List<MonthlyDataDto> RevenueData { get; set; } = new();
    public List<MonthlyDataDto> ProfitData { get; set; } = new();
    
    /// <summary>
    /// ? BARU: Trend FCR 6 bulan terakhir
    /// </summary>
    public List<MonthlyFcrDataDto> FcrData { get; set; } = new();
    
    public List<MonthlyDataDto> ProductivityData { get; set; } = new();
    public List<MonthlyDataDto> MortalityData { get; set; } = new();
}

/// <summary>
/// ? BARU: DTO khusus untuk FCR trend
/// </summary>
public class MonthlyFcrDataDto
{
    public string Month { get; set; } = string.Empty;
    public double FcrValue { get; set; }
    public string Status { get; set; } = string.Empty; // Excellent, Good, Fair, Poor
}
```

---

### **2. Update Service - `DashboardService.cs`**

#### A. `GetBusinessKpiAsync()` - Tambahkan FCR Calculation

```csharp
private async Task<BusinessKpiDto> GetBusinessKpiAsync(int targetYear, int targetMonth)
{
    var currentMonthStart = DateTime.SpecifyKind(new DateTime(targetYear, targetMonth, 1), DateTimeKind.Utc);
    var currentMonthEnd = currentMonthStart.AddMonths(1);

    // ... existing code untuk revenue, expenses, roi, stok ayam, productivity ...

    // ? BARU: Calculate FCR for the selected period
    // FCR = Total Pakan Dikonsumsi (kg) / Total Berat Ayam yang Dihasilkan (kg)
    var totalFeedConsumed = await _context.Operasionals
        .Where(o => o.Tanggal >= currentMonthStart && 
                   o.Tanggal < currentMonthEnd && 
                   o.PakanId != null)
        .SumAsync(o => o.Jumlah);

    var totalWeightProduced = await _context.Panens
        .Where(p => p.TanggalPanen >= currentMonthStart && p.TanggalPanen < currentMonthEnd)
        .SumAsync(p => p.JumlahEkorPanen * p.BeratRataRata);

    // Calculate FCR - default to 1.8 (good FCR) if no data
    var feedConversionRatio = totalWeightProduced > 0 
        ? (double)totalFeedConsumed / (double)totalWeightProduced 
        : 1.8;

    return new BusinessKpiDto
    {
        // ... existing properties ...
        FeedConversionRatio = Math.Round(feedConversionRatio, 2), // ? BARU
        // ... existing properties ...
    };
}
```

#### B. `GetStrategicInsightsAsync()` - Tambahkan FCR Recommendations

```csharp
private async Task<StrategicInsightsDto> GetStrategicInsightsAsync(int targetYear, int targetMonth)
{
    var recommendations = new List<string>();
    var opportunities = new List<string>();
    var risks = new List<string>();

    // ... existing code untuk mortality, utilization, lowStock ...

    // ? BARU: Get FCR for analysis
    var currentMonthStart = DateTime.SpecifyKind(new DateTime(targetYear, targetMonth, 1), DateTimeKind.Utc);
    var currentMonthEnd = currentMonthStart.AddMonths(1);
    
    var totalFeedConsumed = await _context.Operasionals
        .Where(o => o.Tanggal >= currentMonthStart && 
                   o.Tanggal < currentMonthEnd && 
                   o.PakanId != null)
        .SumAsync(o => o.Jumlah);

    var totalWeightProduced = await _context.Panens
        .Where(p => p.TanggalPanen >= currentMonthStart && p.TanggalPanen < currentMonthEnd)
        .SumAsync(p => p.JumlahEkorPanen * p.BeratRataRata);

    var currentFcr = totalWeightProduced > 0 
        ? (double)totalFeedConsumed / (double)totalWeightProduced 
        : 1.8;

    // ... existing recommendations ...

    // ? BARU: FCR-based recommendations
    if (currentFcr > 2.0)
    {
        recommendations.Add($"FCR terlalu tinggi ({currentFcr:F2}) - Optimalisasi pemberian pakan dan evaluasi kualitas pakan");
        recommendations.Add("Pertimbangkan untuk mengganti jenis pakan atau supplier");
        risks.Add("FCR tinggi dapat mengurangi profitabilitas secara signifikan");
    }
    else if (currentFcr > 1.8 && currentFcr <= 2.0)
    {
        recommendations.Add($"FCR saat ini {currentFcr:F2} (cukup baik) - Masih ada ruang untuk improvement ke target 1.5-1.8");
        opportunities.Add("Optimalisasi jadwal dan jumlah pemberian pakan untuk menurunkan FCR");
    }
    else if (currentFcr <= 1.8)
    {
        opportunities.Add($"FCR sangat baik ({currentFcr:F2}) - Pertahankan praktik pemberian pakan saat ini");
    }

    return new StrategicInsightsDto
    {
        Recommendations = recommendations,
        Opportunities = opportunities,
        Risks = risks,
        KeySuccessFactors = new List<string>
        {
            "Menjaga tingkat mortalitas rendah (< 3%)",
            "Optimalisasi efisiensi pakan (FCR < 1.8)", // ? UPDATED
            "Konsistensi kualitas dan berat ayam",
            "Manajemen biaya operasional yang efektif",
            "Monitoring FCR secara berkala untuk efisiensi maksimal" // ? BARU
        }
    };
}
```

#### C. `GetMonthlyTrendsAsync()` - Tambahkan FCR Trend

```csharp
private async Task<MonthlyTrendsDto> GetMonthlyTrendsAsync(int targetYear, int targetMonth)
{
    var revenueData = new List<MonthlyDataDto>();
    var profitData = new List<MonthlyDataDto>();
    var fcrData = new List<MonthlyFcrDataDto>(); // ? BARU
    var targetDate = new DateTime(targetYear, targetMonth, 1);

    // Get last 6 months data ending at target month
    for (int i = 5; i >= 0; i--)
    {
        var date = targetDate.AddMonths(-i);
        var monthStart = DateTime.SpecifyKind(new DateTime(date.Year, date.Month, 1), DateTimeKind.Utc);
        var monthEnd = monthStart.AddMonths(1);

        // ... existing code untuk revenue dan profit ...

        // ? BARU: Calculate FCR for each month
        var monthFeedConsumed = await _context.Operasionals
            .Where(o => o.Tanggal >= monthStart && 
                       o.Tanggal < monthEnd && 
                       o.PakanId != null)
            .SumAsync(o => o.Jumlah);

        var monthWeightProduced = await _context.Panens
            .Where(p => p.TanggalPanen >= monthStart && p.TanggalPanen < monthEnd)
            .SumAsync(p => p.JumlahEkorPanen * p.BeratRataRata);

        var monthFcr = monthWeightProduced > 0 
            ? (double)monthFeedConsumed / (double)monthWeightProduced 
            : 0;

        // Determine FCR status
        var fcrStatus = monthFcr switch
        {
            0 => "No Data",
            < 1.6 => "Excellent",
            >= 1.6 and < 1.8 => "Good",
            >= 1.8 and < 2.0 => "Fair",
            _ => "Poor"
        };

        // ... existing code untuk add revenue dan profit data ...

        // ? BARU: Add FCR data
        fcrData.Add(new MonthlyFcrDataDto
        {
            Month = date.ToString("MMM yyyy"),
            FcrValue = Math.Round(monthFcr, 2),
            Status = fcrStatus
        });
    }

    return new MonthlyTrendsDto
    {
        RevenueData = revenueData,
        ProfitData = profitData,
        FcrData = fcrData // ? BARU
    };
}
```

#### D. `GetComparisonAnalysisAsync()` - Tambahkan FCR Benchmark

```csharp
private async Task<ComparisonAnalysisDto> GetComparisonAnalysisAsync(int targetYear, int targetMonth)
{
    // ... existing code ...

    // ? BARU: Calculate current FCR
    var totalFeedConsumed = await _context.Operasionals
        .Where(o => o.Tanggal >= currentMonthStart && 
                   o.Tanggal < currentMonthEnd && 
                   o.PakanId != null)
        .SumAsync(o => o.Jumlah);

    var totalWeightProduced = await _context.Panens
        .Where(p => p.TanggalPanen >= currentMonthStart && p.TanggalPanen < currentMonthEnd)
        .SumAsync(p => p.JumlahEkorPanen * p.BeratRataRata);

    var currentFcr = totalWeightProduced > 0 
        ? (double)totalFeedConsumed / (double)totalWeightProduced 
        : 1.8;

    // ? BARU: Enhanced performance rating dengan FCR
    var performanceRating = GetOverallPerformanceRating(currentMortalityRate, currentFcr);

    return new ComparisonAnalysisDto
    {
        // ... existing properties ...
        IndustryBenchmark = new BenchmarkDto
        {
            IndustryAvgMortalityRate = 5.5,
            YourMortalityRate = Math.Round(currentMortalityRate, 1),
            IndustryAvgProductivity = 78.5,
            YourProductivity = 85.2,
            IndustryAvgFcr = 1.7, // ? BARU
            YourFcr = Math.Round(currentFcr, 2), // ? BARU
            PerformanceRating = performanceRating // ? UPDATED
        }
    };
}

/// <summary>
/// ? BARU: Get overall performance rating based on mortality rate and FCR
/// </summary>
private string GetOverallPerformanceRating(double mortalityRate, double fcr)
{
    // Excellent: Low mortality (<3%) AND Excellent FCR (<1.6)
    if (mortalityRate < 3.0 && fcr < 1.6)
        return "Excellent";

    // Good: Low-medium mortality (<5%) AND Good FCR (<1.8)
    if (mortalityRate < 5.0 && fcr < 1.8)
        return "Good";

    // Above Average: Acceptable mortality (<5.5%) AND Fair FCR (<2.0)
    if (mortalityRate < 5.5 && fcr < 2.0)
        return "Above Average";

    // Average: Either mortality or FCR needs improvement
    if (mortalityRate < 7.0 || fcr < 2.2)
        return "Average";

    // Below Average: Both metrics need significant improvement
    return "Below Average";
}
```

---

## ?? Response Example

### **Dashboard Pemilik dengan FCR:**

```json
{
  "success": true,
  "message": "Berhasil mengambil dashboard pemilik untuk 2025-01.",
  "data": {
    "businessKpi": {
      "monthlyRevenue": 50000000,
      "monthlyProfit": 20000000,
      "returnOnInvestment": 15.5,
      "totalAyamStock": 750,
      "averageProductivity": 85.2,
      "feedConversionRatio": 1.68,  // ? BARU: FCR
      "customerSatisfaction": 4.2,
      "marketShare": 15
    },
    "profitability": {
      // ... existing properties ...
    },
    "comparisonAnalysis": {
      "currentVsPreviousMonth": {
        // ... existing properties ...
      },
      "currentVsPreviousYear": {
        // ... existing properties ...
      },
      "industryBenchmark": {
        "industryAvgMortalityRate": 5.5,
        "yourMortalityRate": 3.2,
        "industryAvgProductivity": 78.5,
        "yourProductivity": 85.2,
        "industryAvgFcr": 1.7,      // ? BARU
        "yourFcr": 1.68,             // ? BARU
        "performanceRating": "Good"  // ? Enhanced dengan FCR
      }
    },
    "strategicInsights": {
      "recommendations": [
        "FCR sangat baik (1.68) - Pertahankan praktik pemberian pakan saat ini"
      ],
      "opportunities": [
        "Optimalisasi jadwal dan jumlah pemberian pakan untuk menurunkan FCR"
      ],
      "risks": [],
      "keySuccessFactors": [
        "Menjaga tingkat mortalitas rendah (< 3%)",
        "Optimalisasi efisiensi pakan (FCR < 1.8)",  // ? Updated
        "Konsistensi kualitas dan berat ayam",
        "Manajemen biaya operasional yang efektif",
        "Monitoring FCR secara berkala untuk efisiensi maksimal"  // ? Baru
      ]
    },
    "monthlyTrends": {
      "revenueData": [ /* ... */ ],
      "profitData": [ /* ... */ ],
      "fcrData": [  // ? BARU: FCR Trend 6 bulan
        {
          "month": "Aug 2024",
          "fcrValue": 1.75,
          "status": "Good"
        },
        {
          "month": "Sep 2024",
          "fcrValue": 1.82,
          "status": "Fair"
        },
        {
          "month": "Oct 2024",
          "fcrValue": 1.70,
          "status": "Good"
        },
        {
          "month": "Nov 2024",
          "fcrValue": 1.65,
          "status": "Good"
        },
        {
          "month": "Dec 2024",
          "fcrValue": 1.72,
          "status": "Good"
        },
        {
          "month": "Jan 2025",
          "fcrValue": 1.68,
          "status": "Good"
        }
      ]
    }
  }
}
```

---

## ?? Manfaat Fitur FCR

### **1. Monitoring Efisiensi Pakan**
- Melihat berapa efektif penggunaan pakan
- Target: FCR < 1.8 (semakin rendah semakin baik)

### **2. Cost Optimization**
- FCR tinggi = pemborosan pakan = biaya tinggi
- FCR rendah = efisien = profit lebih besar

### **3. Benchmarking**
- Bandingkan FCR Anda dengan industry standard (1.6-1.8)
- Tahu posisi performa Anda di industri

### **4. Trend Analysis**
- Lihat perubahan FCR dari bulan ke bulan
- Identifikasi pattern dan seasonal changes

### **5. Smart Recommendations**
- Sistem memberikan saran otomatis berdasarkan FCR:
  - FCR > 2.0: Alert critical, perlu action segera
  - FCR 1.8-2.0: Warning, ada room for improvement
  - FCR < 1.8: Good performance, maintain strategy

---

## ? Implementation Checklist

- [ ] Update `BusinessKpiDto` dengan `FeedConversionRatio` property
- [ ] Update `BenchmarkDto` dengan `IndustryAvgFcr` dan `YourFcr`
- [ ] Tambah `MonthlyFcrDataDto` class baru
- [ ] Update `MonthlyTrendsDto` dengan `FcrData` property
- [ ] Update `GetBusinessKpiAsync()` untuk calculate FCR
- [ ] Update `GetStrategicInsightsAsync()` untuk FCR recommendations
- [ ] Update `GetMonthlyTrendsAsync()` untuk FCR trend
- [ ] Update `GetComparisonAnalysisAsync()` untuk FCR benchmark
- [ ] Tambah `GetOverallPerformanceRating()` helper method
- [ ] Test dengan periode berbeda
- [ ] Dokumentasi API response

---

## ?? Notes

### **Formula FCR:**
```
FCR = Total Pakan (kg) / Total Berat Ayam (kg)
```

### **Data Source:**
- **Total Pakan**: Dari `Operasionals` table yang linked ke `PakanId`
- **Total Berat Ayam**: Dari `Panens` table (`JumlahEkorPanen * BeratRataRata`)

### **Time Period:**
FCR dihitung berdasarkan periode yang dipilih di dashboard (month & year parameter)

### **Default Value:**
Jika tidak ada data panen (totalWeightProduced = 0), FCR default = 1.8 (good FCR standard)

---

**Status:** ? **READY TO IMPLEMENT**  
**Priority:** **HIGH** (Request from client)  
**Impact:** Dashboard Pemilik will have complete performance metrics

using Microsoft.EntityFrameworkCore;
using SIMTernakAyam.Data;
using SIMTernakAyam.DTOs.Chart;
using SIMTernakAyam.Services.Interfaces;

namespace SIMTernakAyam.Services
{
    public class ChartService : IChartService
    {
        private readonly ApplicationDbContext _context;

        public ChartService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ProduktivitasTrendDto> GetProduktivitasTrendAsync(string period, DateTime? startDate = null, DateTime? endDate = null)
        {
            // Set default date range
            var now = DateTime.Now;
            var start = startDate ?? (period == "weekly" ? now.AddDays(-30) : now.AddMonths(-6));
            var end = endDate ?? now;

            // Get operasional data
            var operasionals = await _context.Operasionals
                .Include(o => o.Kandang)
                .Where(o => o.Tanggal >= start && o.Tanggal <= end)
                .ToListAsync();

            var chartData = new ChartDataDto();
            var labels = new List<string>();
            var operasionalCount = new List<decimal>();

            if (period == "weekly")
            {
                // Group by week
                var weeklyData = operasionals
                    .GroupBy(o => new
                    {
                        Year = o.Tanggal.Year,
                        Week = System.Globalization.CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(
                            o.Tanggal,
                            System.Globalization.CalendarWeekRule.FirstDay,
                            DayOfWeek.Monday)
                    })
                    .OrderBy(g => g.Key.Year)
                    .ThenBy(g => g.Key.Week)
                    .Select(g => new
                    {
                        Label = $"Week {g.Key.Week} {g.Key.Year}",
                        Count = g.Count()
                    })
                    .ToList();

                labels = weeklyData.Select(w => w.Label).ToList();
                operasionalCount = weeklyData.Select(w => (decimal)w.Count).ToList();
            }
            else // monthly
            {
                // Group by month
                var monthlyData = operasionals
                    .GroupBy(o => new { o.Tanggal.Year, o.Tanggal.Month })
                    .OrderBy(g => g.Key.Year)
                    .ThenBy(g => g.Key.Month)
                    .Select(g => new
                    {
                        Label = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy"),
                        Count = g.Count()
                    })
                    .ToList();

                labels = monthlyData.Select(m => m.Label).ToList();
                operasionalCount = monthlyData.Select(m => (decimal)m.Count).ToList();
            }

            chartData.Labels = labels;
            chartData.Series.Add(new ChartSeriesDto
            {
                Name = "Jumlah Operasional",
                Data = operasionalCount,
                Color = "#4CAF50"
            });

            return new ProduktivitasTrendDto
            {
                Periode = period == "weekly" ? "Mingguan" : "Bulanan",
                TanggalMulai = start,
                TanggalSelesai = end,
                ChartData = chartData,
                TotalKandangAktif = operasionals.Select(o => o.KandangId).Distinct().Count(),
                TotalOperasionalDilakukan = operasionals.Count,
                RataProduktivitas = operasionals.Count > 0 ? (decimal)operasionals.Count / labels.Count : 0
            };
        }

        public async Task<MortalitasStatistikDto> GetMortalitasStatistikAsync(Guid? kandangId = null, string period = "monthly")
        {
            var now = DateTime.Now;
            var startDate = period == "weekly" ? now.AddDays(-30) : now.AddMonths(-6);

            // Get mortalitas data
            var query = _context.Mortalitas
                .Include(m => m.Ayam)
                .ThenInclude(a => a.Kandang)
                .Where(m => m.TanggalKematian >= startDate);

            if (kandangId.HasValue)
            {
                query = query.Where(m => m.Ayam.KandangId == kandangId.Value);
            }

            var mortalitasList = await query.ToListAsync();

            // Get total ayam
            var ayamQuery = _context.Ayams.AsQueryable();
            if (kandangId.HasValue)
            {
                ayamQuery = ayamQuery.Where(a => a.KandangId == kandangId.Value);
            }
            var totalAyam = await ayamQuery.SumAsync(a => a.JumlahMasuk);

            // Build chart data
            var chartData = new ChartDataDto();
            var labels = new List<string>();
            var mortalitasCount = new List<decimal>();

            if (period == "weekly")
            {
                var weeklyData = mortalitasList
                    .GroupBy(m => new
                    {
                        Year = m.TanggalKematian.Year,
                        Week = System.Globalization.CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(
                            m.TanggalKematian,
                            System.Globalization.CalendarWeekRule.FirstDay,
                            DayOfWeek.Monday)
                    })
                    .OrderBy(g => g.Key.Year)
                    .ThenBy(g => g.Key.Week)
                    .Select(g => new
                    {
                        Label = $"Week {g.Key.Week} {g.Key.Year}",
                        Count = g.Sum(m => m.JumlahKematian)
                    })
                    .ToList();

                labels = weeklyData.Select(w => w.Label).ToList();
                mortalitasCount = weeklyData.Select(w => (decimal)w.Count).ToList();
            }
            else
            {
                var monthlyData = mortalitasList
                    .GroupBy(m => new { m.TanggalKematian.Year, m.TanggalKematian.Month })
                    .OrderBy(g => g.Key.Year)
                    .ThenBy(g => g.Key.Month)
                    .Select(g => new
                    {
                        Label = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy"),
                        Count = g.Sum(m => m.JumlahKematian)
                    })
                    .ToList();

                labels = monthlyData.Select(m => m.Label).ToList();
                mortalitasCount = monthlyData.Select(m => (decimal)m.Count).ToList();
            }

            chartData.Labels = labels;
            chartData.Series.Add(new ChartSeriesDto
            {
                Name = "Mortalitas",
                Data = mortalitasCount,
                Color = "#F44336"
            });

            // Top penyebab mortalitas
            var topPenyebab = mortalitasList
                .GroupBy(m => m.PenyebabKematian)
                .Select(g => new PenyebabMortalitasDto
                {
                    Penyebab = g.Key,
                    Jumlah = g.Sum(m => m.JumlahKematian),
                    Persentase = 0
                })
                .OrderByDescending(p => p.Jumlah)
                .Take(5)
                .ToList();

            var totalMortalitas = mortalitasList.Sum(m => m.JumlahKematian);
            foreach (var penyebab in topPenyebab)
            {
                penyebab.Persentase = totalMortalitas > 0 ? Math.Round((decimal)penyebab.Jumlah / totalMortalitas * 100, 2) : 0;
            }

            return new MortalitasStatistikDto
            {
                KandangId = kandangId,
                NamaKandang = kandangId.HasValue ? (await _context.Kandangs.FindAsync(kandangId.Value))?.NamaKandang : "Semua Kandang",
                Periode = period == "weekly" ? "Mingguan" : "Bulanan",
                ChartData = chartData,
                TotalMortalitas = totalMortalitas,
                TotalAyam = totalAyam,
                PersentaseMortalitas = totalAyam > 0 ? Math.Round((decimal)totalMortalitas / totalAyam * 100, 2) : 0,
                TopPenyebab = topPenyebab
            };
        }

        public async Task<OperasionalBreakdownDto> GetOperasionalBreakdownAsync(Guid? petugasId = null, string period = "monthly")
        {
            var now = DateTime.Now;
            var startDate = new DateTime(now.Year, now.Month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var query = _context.Operasionals
                .Include(o => o.JenisKegiatan)
                .Include(o => o.Petugas)
                .Include(o => o.Kandang)
                .Where(o => o.Tanggal >= startDate && o.Tanggal <= endDate);

            if (petugasId.HasValue)
            {
                query = query.Where(o => o.PetugasId == petugasId.Value);
            }

            var operasionals = await query.ToListAsync();
            var totalOperasional = operasionals.Count;

            var categories = operasionals
                .GroupBy(o => o.JenisKegiatan.NamaKegiatan)
                .Select(g => new OperasionalCategoryDto
                {
                    NamaKegiatan = g.Key,
                    JumlahOperasional = g.Count(),
                    Persentase = totalOperasional > 0 ? Math.Round((decimal)g.Count() / totalOperasional * 100, 2) : 0
                })
                .OrderByDescending(c => c.JumlahOperasional)
                .ToList();

            // Assign colors
            var colors = new[] { "#4CAF50", "#2196F3", "#FF9800", "#9C27B0", "#F44336", "#00BCD4", "#FFEB3B" };
            for (int i = 0; i < categories.Count; i++)
            {
                categories[i].Color = colors[i % colors.Length];
            }

            return new OperasionalBreakdownDto
            {
                PetugasId = petugasId,
                NamaPetugas = petugasId.HasValue ? (await _context.Users.FindAsync(petugasId.Value))?.FullName : "Semua Petugas",
                Periode = startDate.ToString("MMMM yyyy"),
                Categories = categories,
                TotalOperasional = totalOperasional,
                TotalKandangDikelola = operasionals.Select(o => o.KandangId).Distinct().Count(),
                KategoriTerbanyak = categories.FirstOrDefault()?.NamaKegiatan ?? "-"
            };
        }

        public async Task<FinancialAnalysisDto> GetFinancialAnalysisAsync(DateTime startDate, DateTime endDate)
        {
            // Get biaya (cost)
            var biayaList = await _context.Biayas
                .Where(b => b.Tanggal >= startDate && b.Tanggal <= endDate)
                .ToListAsync();

            // Get panen (revenue)
            var panenList = await _context.Panens
                .Where(p => p.TanggalPanen >= startDate && p.TanggalPanen <= endDate)
                .ToListAsync();

            // Build chart data - monthly comparison
            var chartData = new ChartDataDto();
            var labels = new List<string>();
            var revenueData = new List<decimal>();
            var costData = new List<decimal>();

            var months = Enumerable.Range(0, ((endDate.Year - startDate.Year) * 12) + endDate.Month - startDate.Month + 1)
                .Select(i => startDate.AddMonths(i))
                .ToList();

            foreach (var month in months)
            {
                labels.Add(month.ToString("MMM yyyy"));

                // Revenue calculation: Jumlah Panen * Berat Rata-rata * Harga per kg (estimasi Rp 30.000/kg)
                var hargaPerKg = 30000m; // Harga estimasi per kg ayam
                var monthRevenue = panenList
                    .Where(p => p.TanggalPanen.Year == month.Year && p.TanggalPanen.Month == month.Month)
                    .Sum(p => p.JumlahEkorPanen * p.BeratRataRata * hargaPerKg);
                revenueData.Add(monthRevenue);

                var monthCost = biayaList
                    .Where(b => b.Tanggal.Year == month.Year && b.Tanggal.Month == month.Month)
                    .Sum(b => b.Jumlah);
                costData.Add(monthCost);
            }

            chartData.Labels = labels;
            chartData.Series.Add(new ChartSeriesDto
            {
                Name = "Revenue",
                Data = revenueData,
                Color = "#4CAF50"
            });
            chartData.Series.Add(new ChartSeriesDto
            {
                Name = "Cost",
                Data = costData,
                Color = "#F44336"
            });

            var hargaPerKgTotal = 30000m; // Harga estimasi per kg ayam
            var totalRevenue = panenList.Sum(p => p.JumlahEkorPanen * p.BeratRataRata * hargaPerKgTotal);
            var totalCost = biayaList.Sum(b => b.Jumlah);
            var netProfit = totalRevenue - totalCost;
            var profitMargin = totalRevenue > 0 ? Math.Round(netProfit / totalRevenue * 100, 2) : 0;

            // Biaya breakdown
            var biayaBreakdown = biayaList
                .GroupBy(b => b.JenisBiaya)
                .Select(g => new BiayaBreakdownDto
                {
                    JenisBiaya = g.Key,
                    TotalBiaya = g.Sum(b => b.Jumlah),
                    Persentase = totalCost > 0 ? Math.Round(g.Sum(b => b.Jumlah) / totalCost * 100, 2) : 0
                })
                .OrderByDescending(b => b.TotalBiaya)
                .ToList();

            return new FinancialAnalysisDto
            {
                TanggalMulai = startDate,
                TanggalSelesai = endDate,
                ChartData = chartData,
                TotalRevenue = totalRevenue,
                TotalCost = totalCost,
                NetProfit = netProfit,
                ProfitMargin = profitMargin,
                BiayaBreakdown = biayaBreakdown
            };
        }
    }
}

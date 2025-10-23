using Microsoft.EntityFrameworkCore;
using SIMTernakAyam.Data;
using SIMTernakAyam.DTOs.Dashboard;
using SIMTernakAyam.DTOs.Dashboard.Charts;
using SIMTernakAyam.Enums;
using SIMTernakAyam.Services.Interfaces;

namespace SIMTernakAyam.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly ApplicationDbContext _context;

        public DashboardService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<object> GetDashboardDataAsync(Guid userId, RoleEnum role)
        {
            return role switch
            {
                RoleEnum.Operator => await GetOperatorDashboardAsync(),
                RoleEnum.Petugas => await GetPetugasDashboardAsync(userId),
                RoleEnum.Pemilik => await GetPemilikDashboardAsync(),
                _ => throw new ArgumentException("Invalid role")
            };
        }

        public async Task<OperatorDashboardDto> GetOperatorDashboardAsync()
        {
            var systemOverview = await GetSystemOverviewAsync();
            var kandangPerformances = await GetKandangPerformancesAsync();
            var financialSummary = await GetFinancialSummaryAsync();
            var systemAlerts = await GetSystemAlertsAsync();
            var productivityStats = await GetProductivityStatsAsync();

            return new OperatorDashboardDto
            {
                SystemOverview = systemOverview,
                KandangPerformances = kandangPerformances,
                FinancialSummary = financialSummary,
                SystemAlerts = systemAlerts,
                ProductivityStats = productivityStats
            };
        }

        public async Task<PetugasDashboardDto> GetPetugasDashboardAsync(Guid petugasId)
        {
            var myKandangs = await GetMyKandangsAsync(petugasId);
            var dailyTasks = await GetDailyTasksAsync(petugasId);
            var stockAlerts = await GetStockAlertsAsync();
            var myPerformance = await GetMyPerformanceAsync(petugasId);
            var upcomingActivities = await GetUpcomingActivitiesAsync(petugasId);

            return new PetugasDashboardDto
            {
                MyKandangs = myKandangs,
                DailyTasks = dailyTasks,
                StockAlerts = stockAlerts,
                MyPerformance = myPerformance,
                UpcomingActivities = upcomingActivities
            };
        }

        public async Task<PemilikDashboardDto> GetPemilikDashboardAsync()
        {
            var businessKpi = await GetBusinessKpiAsync();
            var profitability = await GetProfitabilityAsync();
            var comparisonAnalysis = await GetComparisonAnalysisAsync();
            var strategicInsights = await GetStrategicInsightsAsync();
            var monthlyTrends = await GetMonthlyTrendsAsync();

            return new PemilikDashboardDto
            {
                BusinessKpi = businessKpi,
                Profitability = profitability,
                ComparisonAnalysis = comparisonAnalysis,
                StrategicInsights = strategicInsights,
                MonthlyTrends = monthlyTrends
            };
        }

        #region Private Helper Methods for Dashboard Data

        private async Task<SystemOverviewDto> GetSystemOverviewAsync()
        {
            var today = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc);
            var totalKandangs = await _context.Kandangs.CountAsync();
            var totalAyams = await _context.Ayams.SumAsync(a => a.JumlahMasuk);
            var totalUsers = await _context.Users.CountAsync();
            var activeOperations = await _context.Operasionals
                .Where(o => o.Tanggal.Date == today.Date)
                .CountAsync();

            return new SystemOverviewDto
            {
                TotalKandangs = totalKandangs,
                TotalAyams = totalAyams,
                TotalUsers = totalUsers,
                ActiveOperations = activeOperations,
                LastUpdated = DateTime.UtcNow
            };
        }

        private async Task<List<KandangPerformanceDto>> GetKandangPerformancesAsync()
        {
            var kandangs = await _context.Kandangs
                .Include(k => k.User)
                .ToListAsync();

            var performances = new List<KandangPerformanceDto>();

            foreach (var kandang in kandangs)
            {
                // Calculate initial count from Ayams - query directly to ensure we get the data
                var initialAyamCount = await _context.Ayams
                    .Where(a => a.KandangId == kandang.Id)
                    .SumAsync(a => a.JumlahMasuk);

                // Get total mortality for this kandang (all time)
                var totalMortality = await _context.Mortalitas
                    .Where(m => m.Ayam.KandangId == kandang.Id)
                    .SumAsync(m => m.JumlahKematian);

                // Current ayams = initial count - total mortality
                var currentAyams = Math.Max(0, initialAyamCount - totalMortality);

                var utilizationPercentage = kandang.Kapasitas > 0 ? (double)currentAyams / kandang.Kapasitas * 100 : 0;

                // Get mortality for this month only
                var mortalityThisMonth = await _context.Mortalitas
                    .Where(m => m.Ayam.KandangId == kandang.Id &&
                               m.TanggalKematian.Month == DateTime.UtcNow.Month &&
                               m.TanggalKematian.Year == DateTime.UtcNow.Year)
                    .SumAsync(m => m.JumlahKematian);

                // Calculate mortality rate based on initial count (before deaths)
                var basePopulation = initialAyamCount > 0 ? initialAyamCount : currentAyams;
                var mortalityRate = basePopulation > 0 ? (double)mortalityThisMonth / basePopulation * 100 : 0;

                var status = GetKandangStatus(utilizationPercentage, mortalityRate);

                performances.Add(new KandangPerformanceDto
                {
                    KandangId = kandang.Id,
                    KandangName = kandang.NamaKandang,
                    PetugasName = kandang.User?.FullName ?? kandang.User?.Username ?? "Unknown",
                    CurrentAyams = currentAyams,
                    Capacity = kandang.Kapasitas,
                    UtilizationPercentage = utilizationPercentage,
                    MortalityThisMonth = mortalityThisMonth,
                    MortalityRate = mortalityRate,
                    Status = status
                });
            }

            return performances;
        }

        private async Task<FinancialSummaryDto> GetFinancialSummaryAsync()
        {
            var now = DateTime.UtcNow;
            var currentMonth = now.Month;
            var currentYear = now.Year;

            // Calculate revenue from Panen
            var revenue = await _context.Panens
                .Where(p => p.TanggalPanen.Month == currentMonth && p.TanggalPanen.Year == currentYear)
                .SumAsync(p => p.JumlahEkorPanen * p.BeratRataRata * 25000); // Assume 25k per kg

            // Calculate expenses from Biaya
            var expenses = await _context.Biayas
                .Where(b => b.Tanggal.Month == currentMonth && b.Tanggal.Year == currentYear)
                .SumAsync(b => b.Jumlah);

            var netProfit = revenue - expenses;
            var profitMargin = revenue > 0 ? (double)(netProfit / revenue) * 100 : 0;

            // Calculate monthly change compared to previous month
            var previousMonth = now.AddMonths(-1);
            var previousRevenue = await _context.Panens
                .Where(p => p.TanggalPanen.Month == previousMonth.Month && p.TanggalPanen.Year == previousMonth.Year)
                .SumAsync(p => p.JumlahEkorPanen * p.BeratRataRata * 25000);

            var monthlyChange = previousRevenue > 0 ? (decimal)(((double)revenue - (double)previousRevenue) / (double)previousRevenue) * 100 : 0;

            return new FinancialSummaryDto
            {
                TotalRevenue = revenue,
                TotalExpenses = expenses,
                NetProfit = netProfit,
                ProfitMargin = (decimal)profitMargin,
                MonthlyChange = monthlyChange
            };
        }

        private async Task<List<AlertDto>> GetSystemAlertsAsync()
        {
            var alerts = new List<AlertDto>();

            // Check low stock alerts
            var lowStockPakan = await _context.Pakans.Where(p => p.StokKg <= 10).ToListAsync();
            var lowStockVaksin = await _context.Vaksins.Where(v => v.Stok <= 5).ToListAsync();

            foreach (var pakan in lowStockPakan)
            {
                alerts.Add(new AlertDto
                {
                    Type = "Stock",
                    Message = $"Stok pakan {pakan.NamaPakan} rendah ({pakan.StokKg} kg tersisa)",
                    Severity = pakan.StokKg <= 5 ? "Critical" : "Medium",
                    CreatedAt = DateTime.UtcNow,
                    RelatedEntityId = pakan.Id,
                    RelatedEntityName = pakan.NamaPakan
                });
            }

            foreach (var vaksin in lowStockVaksin)
            {
                alerts.Add(new AlertDto
                {
                    Type = "Stock",
                    Message = $"Stok vaksin {vaksin.NamaVaksin} rendah ({vaksin.Stok} tersisa)",
                    Severity = vaksin.Stok <= 2 ? "Critical" : "Medium",
                    CreatedAt = DateTime.UtcNow,
                    RelatedEntityId = vaksin.Id,
                    RelatedEntityName = vaksin.NamaVaksin
                });
            }

            // Check high mortality rate alerts
            var highMortalityKandangs = await GetKandangPerformancesAsync();
            foreach (var kandang in highMortalityKandangs.Where(k => k.MortalityRate > 5))
            {
                alerts.Add(new AlertDto
                {
                    Type = "Mortality",
                    Message = $"Tingkat mortalitas tinggi di {kandang.KandangName} ({kandang.MortalityRate:F1}%)",
                    Severity = kandang.MortalityRate > 10 ? "Critical" : "High",
                    CreatedAt = DateTime.UtcNow,
                    RelatedEntityId = kandang.KandangId,
                    RelatedEntityName = kandang.KandangName
                });
            }

            return alerts.OrderByDescending(a => a.CreatedAt).Take(10).ToList();
        }

        private async Task<ProductivityStatsDto> GetProductivityStatsAsync()
        {
            var now = DateTime.UtcNow;
            var currentMonth = now.Month;
            var currentYear = now.Year;

            var avgPanenWeight = await _context.Panens
                .Where(p => p.TanggalPanen.Month == currentMonth && p.TanggalPanen.Year == currentYear)
                .AverageAsync(p => (double?)p.BeratRataRata) ?? 0;

            var totalPanenThisMonth = await _context.Panens
                .Where(p => p.TanggalPanen.Month == currentMonth && p.TanggalPanen.Year == currentYear)
                .SumAsync(p => p.JumlahEkorPanen);

            var avgMortalityRate = await CalculateAverageMortalityRateAsync();
            var feedConversionRatio = await CalculateFeedConversionRatioAsync();
            var activeKandangs = await _context.Kandangs.CountAsync();

            return new ProductivityStatsDto
            {
                AveragePanenWeight = avgPanenWeight,
                TotalPanenThisMonth = totalPanenThisMonth,
                AverageMortalityRate = avgMortalityRate,
                FeedConversionRatio = feedConversionRatio,
                ActiveKandangs = activeKandangs
            };
        }

        private async Task<List<MyKandangDto>> GetMyKandangsAsync(Guid petugasId)
        {
            var today = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc);
            var weekAgo = today.AddDays(-7);

            var kandangs = await _context.Kandangs
                .Include(k => k.Ayams)
                .Where(k => k.petugasId == petugasId)
                .ToListAsync();

            var myKandangs = new List<MyKandangDto>();

            foreach (var kandang in kandangs)
            {
                // Calculate initial count
                var initialCount = await _context.Ayams
                    .Where(a => a.KandangId == kandang.Id)
                    .SumAsync(a => a.JumlahMasuk);

                // Get total mortality untuk kandang ini
                var totalMortality = await _context.Mortalitas
                    .Include(m => m.Ayam)
                    .Where(m => m.Ayam.KandangId == kandang.Id)
                    .SumAsync(m => m.JumlahKematian);

                // Current ayams = initial - mortality
                var currentAyams = Math.Max(0, initialCount - totalMortality);

                var mortalityToday = await _context.Mortalitas
                    .Include(m => m.Ayam)
                    .Where(m => m.Ayam.KandangId == kandang.Id &&
                               m.TanggalKematian.Year == today.Year &&
                               m.TanggalKematian.Month == today.Month &&
                               m.TanggalKematian.Day == today.Day)
                    .SumAsync(m => m.JumlahKematian);

                var mortalityThisWeek = await _context.Mortalitas
                    .Include(m => m.Ayam)
                    .Where(m => m.Ayam.KandangId == kandang.Id &&
                               m.TanggalKematian >= weekAgo && m.TanggalKematian <= today)
                    .SumAsync(m => m.JumlahKematian);

                // Get last feed and vaccination from Jurnal Harian
                var lastFeedJurnal = await _context.JurnalHarians
                    .Where(j => j.KandangId == kandang.Id &&
                               (j.JudulKegiatan.ToLower().Contains("pakan") ||
                                j.DeskripsiKegiatan.ToLower().Contains("pakan") ||
                                j.JudulKegiatan.ToLower().Contains("feed")))
                    .OrderByDescending(j => j.Tanggal)
                    .ThenByDescending(j => j.WaktuSelesai)
                    .FirstOrDefaultAsync();

                var lastFeedTime = lastFeedJurnal != null ? DateTime.SpecifyKind(lastFeedJurnal.Tanggal, DateTimeKind.Utc) : default;

                var lastVaccinationJurnal = await _context.JurnalHarians
                    .Where(j => j.KandangId == kandang.Id &&
                               (j.JudulKegiatan.ToLower().Contains("vaksin") ||
                                j.DeskripsiKegiatan.ToLower().Contains("vaksin") ||
                                j.JudulKegiatan.ToLower().Contains("vaccin")))
                    .OrderByDescending(j => j.Tanggal)
                    .ThenByDescending(j => j.WaktuSelesai)
                    .FirstOrDefaultAsync();

                var lastVaccinationTime = lastVaccinationJurnal != null ? DateTime.SpecifyKind(lastVaccinationJurnal.Tanggal, DateTimeKind.Utc) : default;

                var healthStatus = GetHealthStatus(mortalityToday, mortalityThisWeek, currentAyams);

                myKandangs.Add(new MyKandangDto
                {
                    Id = kandang.Id,
                    Name = kandang.NamaKandang,
                    CurrentAyams = currentAyams,
                    Capacity = kandang.Kapasitas,
                    MortalityToday = mortalityToday,
                    MortalityThisWeek = mortalityThisWeek,
                    LastFeedTime = lastFeedTime,
                    LastVaccinationTime = lastVaccinationTime,
                    HealthStatus = healthStatus
                });
            }

            return myKandangs;
        }

        private async Task<DailyTasksDto> GetDailyTasksAsync(Guid petugasId)
        {
            var today = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc);
            var userKandangs = await _context.Kandangs
                .Where(k => k.petugasId == petugasId)
                .Select(k => k.Id)
                .ToListAsync();

            // Get jurnal harian for today
            var todayJurnals = await _context.JurnalHarians
                .Where(j => j.PetugasId == petugasId &&
                           j.Tanggal.Year == today.Year &&
                           j.Tanggal.Month == today.Month &&
                           j.Tanggal.Day == today.Day)
                .ToListAsync();

            // Categorize based on judul/deskripsi
            var feedingJurnals = todayJurnals.Count(j =>
                j.JudulKegiatan.ToLower().Contains("pakan") ||
                j.DeskripsiKegiatan.ToLower().Contains("pakan") ||
                j.JudulKegiatan.ToLower().Contains("feeding"));

            var vaccinationJurnals = todayJurnals.Count(j =>
                j.JudulKegiatan.ToLower().Contains("vaksin") ||
                j.DeskripsiKegiatan.ToLower().Contains("vaksin") ||
                j.JudulKegiatan.ToLower().Contains("vaccination"));

            var cleaningJurnals = todayJurnals.Count(j =>
                j.JudulKegiatan.ToLower().Contains("bersih") ||
                j.DeskripsiKegiatan.ToLower().Contains("bersih") ||
                j.JudulKegiatan.ToLower().Contains("cleaning"));

            // Calculate pending tasks
            var kandangCount = userKandangs.Count;
            var pendingFeedings = Math.Max(0, kandangCount * 3 - feedingJurnals);
            var pendingVaccinations = Math.Max(0, kandangCount / 7 - vaccinationJurnals);
            var pendingCleanings = Math.Max(0, kandangCount - cleaningJurnals);

            var completedTasks = todayJurnals.Count;
            var totalTasks = completedTasks + pendingFeedings + pendingVaccinations + pendingCleanings;
            var completionRate = totalTasks > 0 ? (double)completedTasks / totalTasks * 100 : 0;

            return new DailyTasksDto
            {
                PendingFeedings = pendingFeedings,
                PendingVaccinations = pendingVaccinations,
                PendingCleanings = pendingCleanings,
                CompletedTasks = completedTasks,
                TotalTasks = totalTasks,
                CompletionRate = Math.Round(completionRate, 1)
            };
        }

        private async Task<StockAlertsDto> GetStockAlertsAsync()
        {
            var lowStockPakan = await _context.Pakans
                .Where(p => p.StokKg <= 50)
                .Select(p => new StockItemDto
                {
                    Id = p.Id,
                    Name = p.NamaPakan,
                    CurrentStock = (int)p.StokKg,
                    MinimumStock = 50,
                    Status = p.StokKg <= 20 ? "Critical" : "Warning"
                })
                .ToListAsync();

            var lowStockVaksin = await _context.Vaksins
                .Where(v => v.Stok <= 10)
                .Select(v => new StockItemDto
                {
                    Id = v.Id,
                    Name = v.NamaVaksin,
                    CurrentStock = v.Stok,
                    MinimumStock = 10,
                    Status = v.Stok <= 5 ? "Critical" : "Warning"
                })
                .ToListAsync();

            return new StockAlertsDto
            {
                LowStockPakan = lowStockPakan,
                LowStockVaksin = lowStockVaksin,
                CriticalStockCount = lowStockPakan.Count(p => p.Status == "Critical") + lowStockVaksin.Count(v => v.Status == "Critical"),
                WarningStockCount = lowStockPakan.Count(p => p.Status == "Warning") + lowStockVaksin.Count(v => v.Status == "Warning")
            };
        }

        private async Task<MyPerformanceDto> GetMyPerformanceAsync(Guid petugasId)
        {
            var now = DateTime.UtcNow;
            var currentWeekStart = DateTime.SpecifyKind(now.Date.AddDays(-(int)now.DayOfWeek), DateTimeKind.Utc);
            var currentMonthStart = DateTime.SpecifyKind(new DateTime(now.Year, now.Month, 1), DateTimeKind.Utc);

            var userKandangs = await _context.Kandangs
                .Where(k => k.petugasId == petugasId)
                .ToListAsync();

            var kandangsManaged = userKandangs.Count;

            // Count jurnal harian completed this week (as tasks)
            var tasksThisWeek = await _context.JurnalHarians
                .Where(j => j.PetugasId == petugasId && j.Tanggal >= currentWeekStart)
                .CountAsync();

            // Count jurnal harian completed this month
            var tasksThisMonth = await _context.JurnalHarians
                .Where(j => j.PetugasId == petugasId && j.Tanggal >= currentMonthStart)
                .CountAsync();

            // Calculate average mortality rate for user's kandangs
            var kandangIds = userKandangs.Select(k => k.Id).ToList();

            var totalAyams = await _context.Ayams
                .Where(a => kandangIds.Contains(a.KandangId))
                .SumAsync(a => a.JumlahMasuk);

            var totalMortality = await _context.Mortalitas
                .Where(m => kandangIds.Contains(m.Ayam.KandangId) &&
                           m.TanggalKematian >= currentMonthStart)
                .SumAsync(m => m.JumlahKematian);

            var avgMortalityRate = totalAyams > 0 ? (double)totalMortality / totalAyams * 100 : 0;

            // Calculate efficiency score based on jurnal harian completion
            var expectedTasksPerWeek = kandangsManaged * 7; // At least 1 jurnal per day per kandang
            var taskEfficiency = expectedTasksPerWeek > 0 ? Math.Min(100, (double)tasksThisWeek / (double)expectedTasksPerWeek * 100) : 0;
            var mortalityScore = Math.Max(0, 100 - (avgMortalityRate * 10)); // Lower mortality = higher score
            var efficiencyScore = (taskEfficiency + mortalityScore) / 2;

            var performanceLevel = efficiencyScore switch
            {
                >= 90 => "Excellent",
                >= 80 => "Good",
                >= 70 => "Average",
                _ => "Needs Improvement"
            };

            return new MyPerformanceDto
            {
                EfficiencyScore = Math.Round(efficiencyScore, 1),
                TasksCompletedThisWeek = tasksThisWeek,
                TasksCompletedThisMonth = tasksThisMonth,
                AverageMortalityRate = Math.Round(avgMortalityRate, 1),
                KandangsManaged = kandangsManaged,
                PerformanceLevel = performanceLevel
            };
        }

        private async Task<UpcomingActivitiesDto> GetUpcomingActivitiesAsync(Guid petugasId)
        {
            var userKandangs = await _context.Kandangs
                .Where(k => k.petugasId == petugasId)
                .ToListAsync();

            var now = DateTime.UtcNow;
            var today = DateTime.SpecifyKind(now.Date, DateTimeKind.Utc);
            var tomorrow = today.AddDays(1);
            var thisWeekEnd = today.AddDays(7 - (int)today.DayOfWeek);

            var activities = new List<ScheduledActivityDto>();

            // Generate scheduled activities based on jurnal harian
            foreach (var kandang in userKandangs)
            {
                // Check if feeding jurnal was done today
                var lastFeedingJurnal = await _context.JurnalHarians
                    .Where(j => j.KandangId == kandang.Id &&
                               (j.JudulKegiatan.ToLower().Contains("pakan") ||
                                j.DeskripsiKegiatan.ToLower().Contains("pakan") ||
                                j.JudulKegiatan.ToLower().Contains("feed")))
                    .OrderByDescending(j => j.Tanggal)
                    .ThenByDescending(j => j.WaktuSelesai)
                    .FirstOrDefaultAsync();

                // Check if feeding was done today by comparing date parts
                var feedingDoneToday = lastFeedingJurnal != null &&
                                      lastFeedingJurnal.Tanggal.Year == today.Year &&
                                      lastFeedingJurnal.Tanggal.Month == today.Month &&
                                      lastFeedingJurnal.Tanggal.Day == today.Day;

                if (!feedingDoneToday)
                {
                    activities.Add(new ScheduledActivityDto
                    {
                        ActivityType = "Feeding",
                        KandangName = kandang.NamaKandang,
                        ScheduledTime = DateTime.SpecifyKind(today.AddHours(8), DateTimeKind.Utc),
                        Priority = "High",
                        IsOverdue = now.Hour > 8
                    });
                }

                // Check if cleaning jurnal was done today
                var lastCleaningJurnal = await _context.JurnalHarians
                    .Where(j => j.KandangId == kandang.Id &&
                               (j.JudulKegiatan.ToLower().Contains("bersih") ||
                                j.DeskripsiKegiatan.ToLower().Contains("bersih") ||
                                j.JudulKegiatan.ToLower().Contains("clean")))
                    .OrderByDescending(j => j.Tanggal)
                    .ThenByDescending(j => j.WaktuSelesai)
                    .FirstOrDefaultAsync();

                // Check if cleaning was done today
                var cleaningDoneToday = lastCleaningJurnal != null &&
                                       lastCleaningJurnal.Tanggal.Year == today.Year &&
                                       lastCleaningJurnal.Tanggal.Month == today.Month &&
                                       lastCleaningJurnal.Tanggal.Day == today.Day;

                if (!cleaningDoneToday)
                {
                    activities.Add(new ScheduledActivityDto
                    {
                        ActivityType = "Cleaning",
                        KandangName = kandang.NamaKandang,
                        ScheduledTime = DateTime.SpecifyKind(today.AddHours(14), DateTimeKind.Utc),
                        Priority = "Medium",
                        IsOverdue = now.Hour > 14
                    });
                }
            }

            return new UpcomingActivitiesDto
            {
                TodayActivities = activities.Where(a => a.ScheduledTime.Date == today.Date).ToList(),
                TomorrowActivities = new List<ScheduledActivityDto>(), // Untuk besok bisa generate juga kalau mau
                ThisWeekActivities = new List<ScheduledActivityDto>()
            };
        }

        private async Task<BusinessKpiDto> GetBusinessKpiAsync()
        {
            var now = DateTime.UtcNow;
            var currentMonth = now.Month;
            var currentYear = now.Year;
            var currentMonthStart = DateTime.SpecifyKind(new DateTime(currentYear, currentMonth, 1), DateTimeKind.Utc);

            var monthlyRevenue = await _context.Panens
                .Where(p => p.TanggalPanen >= currentMonthStart)
                .SumAsync(p => p.JumlahEkorPanen * p.BeratRataRata * 25000);

            var monthlyExpenses = await _context.Biayas
                .Where(b => b.Tanggal >= currentMonthStart)
                .SumAsync(b => b.Jumlah);

            var monthlyProfit = monthlyRevenue - monthlyExpenses;

            // Calculate ROI (simplified calculation)
            var yearlyExpenses = await _context.Biayas
                .Where(b => b.Tanggal.Year == currentYear)
                .SumAsync(b => b.Jumlah);
            var yearlyRevenue = await _context.Panens
                .Where(p => p.TanggalPanen.Year == currentYear)
                .SumAsync(p => p.JumlahEkorPanen * p.BeratRataRata * 25000);
            var roi = yearlyExpenses > 0 ? (double)(yearlyRevenue - yearlyExpenses) / (double)yearlyExpenses * 100 : 0;

            var totalAyamStock = await _context.Ayams.SumAsync(a => a.JumlahMasuk);

            // Calculate average productivity (simplified as average weight gain)
            var avgProductivity = await _context.Panens
                .Where(p => p.TanggalPanen >= currentMonthStart)
                .AverageAsync(p => (double?)p.BeratRataRata) ?? 0;

            return new BusinessKpiDto
            {
                MonthlyRevenue = monthlyRevenue,
                MonthlyProfit = monthlyProfit,
                ReturnOnInvestment = Math.Round(roi, 1),
                TotalAyamStock = totalAyamStock,
                AverageProductivity = Math.Round(avgProductivity * 100, 1), // Convert to percentage
                CustomerSatisfaction = 4.2, // This would need a separate customer feedback system
                MarketShare = 15 // This would need market data
            };
        }

        private async Task<ProfitabilityDto> GetProfitabilityAsync()
        {
            var now = DateTime.UtcNow;
            var currentMonth = now.Month;
            var currentYear = now.Year;
            var currentMonthStart = DateTime.SpecifyKind(new DateTime(currentYear, currentMonth, 1), DateTimeKind.Utc);

            var revenue = await _context.Panens
                .Where(p => p.TanggalPanen >= currentMonthStart)
                .SumAsync(p => p.JumlahEkorPanen * p.BeratRataRata * 25000);

            var operatingExpenses = await _context.Biayas
                .Where(b => b.Tanggal >= currentMonthStart)
                .SumAsync(b => b.Jumlah);

            var grossProfit = revenue;
            var netProfit = revenue - operatingExpenses;

            var totalKgPanen = await _context.Panens
                .Where(p => p.TanggalPanen >= currentMonthStart)
                .SumAsync(p => p.JumlahEkorPanen * p.BeratRataRata);

            var costPerKg = totalKgPanen > 0 ? (decimal)operatingExpenses / (decimal)totalKgPanen : 0;
            var pricePerKg = 25000; // Assumed market price
            var profitPerKg = pricePerKg - costPerKg;
            var profitMargin = revenue > 0 ? (double)netProfit / (double)revenue * 100 : 0;

            return new ProfitabilityDto
            {
                GrossProfit = grossProfit,
                NetProfit = netProfit,
                OperatingExpenses = operatingExpenses,
                CostPerKg = costPerKg,
                PricePerKg = pricePerKg,
                ProfitPerKg = profitPerKg,
                ProfitMargin = Math.Round(profitMargin, 1)
            };
        }

        private async Task<ComparisonAnalysisDto> GetComparisonAnalysisAsync()
        {
            var currentMonth = DateTime.UtcNow;
            var previousMonth = currentMonth.AddMonths(-1);
            var currentYear = currentMonth.Year;
            var previousYear = currentYear - 1;

            // Current month data
            var currentMonthRevenue = await _context.Panens
                .Where(p => p.TanggalPanen.Month == currentMonth.Month && p.TanggalPanen.Year == currentMonth.Year)
                .SumAsync(p => p.JumlahEkorPanen * p.BeratRataRata * 25000);

            var currentMonthExpenses = await _context.Biayas
                .Where(b => b.Tanggal.Month == currentMonth.Month && b.Tanggal.Year == currentMonth.Year)
                .SumAsync(b => b.Jumlah);

            // Previous month data
            var previousMonthRevenue = await _context.Panens
                .Where(p => p.TanggalPanen.Month == previousMonth.Month && p.TanggalPanen.Year == previousMonth.Year)
                .SumAsync(p => p.JumlahEkorPanen * p.BeratRataRata * 25000);

            var previousMonthExpenses = await _context.Biayas
                .Where(b => b.Tanggal.Month == previousMonth.Month && b.Tanggal.Year == previousMonth.Year)
                .SumAsync(b => b.Jumlah);

            // Calculate month-over-month changes
            var revenueChange = previousMonthRevenue > 0 ? (double)((currentMonthRevenue - previousMonthRevenue) / previousMonthRevenue) * 100 : 0;
            var profitChange = previousMonthExpenses > 0 ? (double)(((currentMonthRevenue - currentMonthExpenses) - (previousMonthRevenue - previousMonthExpenses)) / (previousMonthRevenue - previousMonthExpenses)) * 100 : 0;

            // Current year data
            var currentYearRevenue = await _context.Panens
                .Where(p => p.TanggalPanen.Year == currentYear)
                .SumAsync(p => p.JumlahEkorPanen * p.BeratRataRata * 25000);

            var previousYearRevenue = await _context.Panens
                .Where(p => p.TanggalPanen.Year == previousYear)
                .SumAsync(p => p.JumlahEkorPanen * p.BeratRataRata * 25000);

            var revenueGrowth = previousYearRevenue > 0 ? (double)((currentYearRevenue - previousYearRevenue) / previousYearRevenue) * 100 : 0;

            // Get current mortality rate
            var currentMortalityRate = await CalculateAverageMortalityRateAsync();

            return new ComparisonAnalysisDto
            {
                CurrentVsPreviousMonth = new MonthComparisonDto
                {
                    RevenueChange = (decimal)Math.Round(revenueChange, 1),
                    ProfitChange = (decimal)Math.Round(profitChange, 1),
                    ProductivityChange = 0, // Would need more detailed calculation
                    MortalityRateChange = 0, // Would need previous month mortality data
                    EfficiencyChange = 0 // Would need efficiency metrics
                },
                CurrentVsPreviousYear = new YearComparisonDto
                {
                    RevenueGrowth = (decimal)Math.Round(revenueGrowth, 1),
                    ProfitGrowth = 0, // Would need previous year profit calculation
                    ProductivityGrowth = 0, // Would need productivity comparison
                    CapacityGrowth = 0, // Would need capacity comparison
                    EfficiencyImprovement = 0 // Would need efficiency comparison
                },
                IndustryBenchmark = new BenchmarkDto
                {
                    IndustryAvgMortalityRate = 5.5, // Industry standard
                    YourMortalityRate = Math.Round(currentMortalityRate, 1),
                    IndustryAvgProductivity = 78.5, // Industry standard
                    YourProductivity = 85.2, // Would calculate from actual data
                    PerformanceRating = currentMortalityRate < 5.5 ? "Above Average" : "Below Average"
                }
            };
        }

        private async Task<StrategicInsightsDto> GetStrategicInsightsAsync()
        {
            var recommendations = new List<string>();
            var opportunities = new List<string>();
            var risks = new List<string>();

            // Analyze current performance and generate insights
            var avgMortalityRate = await CalculateAverageMortalityRateAsync();
            var kandangUtilization = await GetKandangUtilizationAsync();
            var lowStockItems = await GetLowStockItemsCountAsync();

            // Generate recommendations based on data
            if (avgMortalityRate > 5)
            {
                recommendations.Add("Tingkatkan program kesehatan ayam dan vaksinasi");
                recommendations.Add("Review kondisi kandang dan ventilasi");
            }

            if (kandangUtilization < 80)
            {
                recommendations.Add($"Optimalisasi kapasitas kandang (utilisasi saat ini: {kandangUtilization:F1}%)");
                opportunities.Add("Potensi peningkatan produksi dengan mengoptimalkan kapasitas kandang");
            }

            if (lowStockItems > 0)
            {
                recommendations.Add("Tingkatkan manajemen persediaan pakan dan vaksin");
                risks.Add("Risiko kekurangan stok dapat mengganggu operasional");
            }

            // Add generic insights
            opportunities.Add("Peluang ekspansi pasar ayam organik");
            opportunities.Add("Implementasi teknologi IoT untuk monitoring otomatis");

            risks.Add("Fluktuasi harga pakan dapat mempengaruhi profitabilitas");
            risks.Add("Risiko wabah penyakit di wilayah sekitar");

            return new StrategicInsightsDto
            {
                Recommendations = recommendations,
                Opportunities = opportunities,
                Risks = risks,
                KeySuccessFactors = new List<string>
                {
                    "Menjaga tingkat mortalitas rendah (< 3%)",
                    "Optimalisasi efisiensi pakan (FCR < 1.8)",
                    "Konsistensi kualitas dan berat ayam",
                    "Manajemen biaya operasional yang efektif"
                }
            };
        }

        private async Task<MonthlyTrendsDto> GetMonthlyTrendsAsync()
        {
            var revenueData = new List<MonthlyDataDto>();
            var profitData = new List<MonthlyDataDto>();
            var now = DateTime.UtcNow;

            // Get last 6 months data
            for (int i = 5; i >= 0; i--)
            {
                var date = now.AddMonths(-i);
                var monthRevenue = await _context.Panens
                    .Where(p => p.TanggalPanen.Month == date.Month && p.TanggalPanen.Year == date.Year)
                    .SumAsync(p => p.JumlahEkorPanen * p.BeratRataRata * 25000);

                var monthExpense = await _context.Biayas
                    .Where(b => b.Tanggal.Month == date.Month && b.Tanggal.Year == date.Year)
                    .SumAsync(b => b.Jumlah);

                var monthProfit = monthRevenue - monthExpense;

                revenueData.Add(new MonthlyDataDto
                {
                    Month = date.ToString("MMM yyyy"),
                    Value = monthRevenue,
                    Percentage = 100 // Would calculate based on baseline
                });

                profitData.Add(new MonthlyDataDto
                {
                    Month = date.ToString("MMM yyyy"),
                    Value = monthProfit,
                    Percentage = 100 // Would calculate based on baseline
                });
            }

            return new MonthlyTrendsDto
            {
                RevenueData = revenueData,
                ProfitData = profitData
            };
        }

        #endregion

        #region Helper Methods

        private string GetKandangStatus(double utilization, double mortalityRate)
        {
            if (mortalityRate > 10 || utilization > 95)
                return "Critical";
            if (mortalityRate > 5 || utilization > 85)
                return "Warning";
            return "Good";
        }

        private string GetHealthStatus(int mortalityToday, int mortalityWeek, int totalAyams)
        {
            if (totalAyams == 0) return "Empty";
            
            var weeklyMortalityRate = (double)mortalityWeek / totalAyams * 100;
            
            if (weeklyMortalityRate > 10) return "Critical";
            if (weeklyMortalityRate > 5) return "Warning";
            if (mortalityToday > 0) return "Monitoring";
            return "Healthy";
        }

        private async Task<double> CalculateAverageMortalityRateAsync()
        {
            var now = DateTime.UtcNow;
            var currentMonth = now.Month;
            var currentYear = now.Year;

            var totalAyams = await _context.Ayams.SumAsync(a => a.JumlahMasuk);
            var totalMortality = await _context.Mortalitas
                .Where(m => m.TanggalKematian.Month == currentMonth && m.TanggalKematian.Year == currentYear)
                .SumAsync(m => m.JumlahKematian);

            return totalAyams > 0 ? (double)totalMortality / totalAyams * 100 : 0;
        }

        private async Task<double> CalculateFeedConversionRatioAsync()
        {
            var now = DateTime.UtcNow;
            var currentMonth = now.Month;
            var currentYear = now.Year;

            // Calculate total feed consumed this month
            var totalFeedConsumed = await _context.Operasionals
                .Where(o => o.Tanggal.Month == currentMonth && o.Tanggal.Year == currentYear && o.PakanId != null)
                .SumAsync(o => o.Jumlah);

            // Calculate total weight gained (simplified calculation)
            var totalWeightGained = await _context.Panens
                .Where(p => p.TanggalPanen.Month == currentMonth && p.TanggalPanen.Year == currentYear)
                .SumAsync(p => p.JumlahEkorPanen * p.BeratRataRata);

            return totalWeightGained > 0 ? (double)totalFeedConsumed / (double)totalWeightGained : 1.8; // Default good FCR
        }

        private async Task<double> GetKandangUtilizationAsync()
        {
            var totalCapacity = await _context.Kandangs.SumAsync(k => k.Kapasitas);
            var totalOccupied = await _context.Ayams.SumAsync(a => a.JumlahMasuk);

            return totalCapacity > 0 ? (double)totalOccupied / totalCapacity * 100 : 0;
        }

        private async Task<int> GetLowStockItemsCountAsync()
        {
            var lowStockPakan = await _context.Pakans.CountAsync(p => p.StokKg <= 50);
            var lowStockVaksin = await _context.Vaksins.CountAsync(v => v.Stok <= 10);

            return lowStockPakan + lowStockVaksin;
        }

        #endregion

        #region Chart Methods - Simplified implementations

        public async Task<RevenueExpenseChartDto> GetRevenueExpenseChartAsync(string period = "monthly")
        {
            var labels = new List<string>();
            var revenueData = new List<decimal>();
            var expenseData = new List<decimal>();
            var now = DateTime.UtcNow;

            // Get last 6 months data
            for (int i = 5; i >= 0; i--)
            {
                var date = now.AddMonths(-i);
                labels.Add(date.ToString("MMM yyyy"));

                var monthRevenue = await _context.Panens
                    .Where(p => p.TanggalPanen.Month == date.Month && p.TanggalPanen.Year == date.Year)
                    .SumAsync(p => p.JumlahEkorPanen * p.BeratRataRata * 25000);

                var monthExpense = await _context.Biayas
                    .Where(b => b.Tanggal.Month == date.Month && b.Tanggal.Year == date.Year)
                    .SumAsync(b => b.Jumlah);

                revenueData.Add(monthRevenue);
                expenseData.Add(monthExpense);
            }

            return new RevenueExpenseChartDto
            {
                ChartType = "bar",
                Title = "Revenue vs Expenses",
                Labels = labels,
                Datasets = new List<ChartDatasetDto>
                {
                    new ChartDatasetDto
                    {
                        Label = "Revenue",
                        Data = revenueData,
                        BackgroundColor = "#4CAF50",
                        BorderColor = "#4CAF50"
                    },
                    new ChartDatasetDto
                    {
                        Label = "Expenses",
                        Data = expenseData,
                        BackgroundColor = "#F44336",
                        BorderColor = "#F44336"
                    }
                },
                TotalRevenue = revenueData.Sum(),
                TotalExpenses = expenseData.Sum(),
                NetProfit = revenueData.Sum() - expenseData.Sum()
            };
        }

        // Placeholder implementations for other chart methods
        public async Task<MortalityTrendChartDto> GetMortalityTrendChartAsync(Guid? kandangId = null, string period = "monthly")
        {
            // Implementation would go here
            throw new NotImplementedException("Chart implementation pending");
        }

        public async Task<ProductionChartDto> GetProductionChartAsync(string period = "monthly")
        {
            throw new NotImplementedException("Chart implementation pending");
        }

        public async Task<KandangUtilizationChartDto> GetKandangUtilizationChartAsync()
        {
            throw new NotImplementedException("Chart implementation pending");
        }

        public async Task<FeedConsumptionChartDto> GetFeedConsumptionChartAsync(string period = "monthly")
        {
            throw new NotImplementedException("Chart implementation pending");
        }

        public async Task<FinancialPerformanceChartDto> GetFinancialPerformanceChartAsync(string period = "monthly")
        {
            throw new NotImplementedException("Chart implementation pending");
        }

        public async Task<OperationalActivitiesChartDto> GetOperationalActivitiesChartAsync(Guid? petugasId = null, string period = "weekly")
        {
            throw new NotImplementedException("Chart implementation pending");
        }

        public async Task<StockLevelsChartDto> GetStockLevelsChartAsync()
        {
            throw new NotImplementedException("Chart implementation pending");
        }

        public async Task<PerformanceComparisonChartDto> GetPerformanceComparisonChartAsync(Guid? kandangId = null)
        {
            throw new NotImplementedException("Chart implementation pending");
        }

        public async Task<SeasonalTrendsChartDto> GetSeasonalTrendsChartAsync()
        {
            throw new NotImplementedException("Chart implementation pending");
        }

        public async Task<CostBreakdownChartDto> GetCostBreakdownChartAsync(string period = "monthly")
        {
            throw new NotImplementedException("Chart implementation pending");
        }

        #endregion
    }
}
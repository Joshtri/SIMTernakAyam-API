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

        public async Task<PemilikDashboardDto> GetPemilikDashboardAsync(int? year = null, int? month = null)
        {
            // Default to current month if not specified
            var targetYear = year ?? DateTime.UtcNow.Year;
            var targetMonth = month ?? DateTime.UtcNow.Month;

            var businessKpi = await GetBusinessKpiAsync(targetYear, targetMonth);
            var profitability = await GetProfitabilityAsync(targetYear, targetMonth);
            var comparisonAnalysis = await GetComparisonAnalysisAsync(targetYear, targetMonth);
            var strategicInsights = await GetStrategicInsightsAsync(targetYear, targetMonth);
            var monthlyTrends = await GetMonthlyTrendsAsync(targetYear, targetMonth);

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
                var statusLevel = pakan.StokKg switch
                {
                    0 => "Level 0",
                    <= 10 => "Level 1",
                    _ => "Level 2"
                };

                alerts.Add(new AlertDto
                {
                    Type = "Stock",
                    Message = $"Stok pakan {pakan.NamaPakan} {statusLevel} ({pakan.StokKg} kg tersisa) - Perlu Restock",
                    Severity = pakan.StokKg <= 10 ? "Critical" : "Warning",
                    CreatedAt = DateTime.UtcNow,
                    RelatedEntityId = pakan.Id,
                    RelatedEntityName = pakan.NamaPakan
                });
            }

            foreach (var vaksin in lowStockVaksin)
            {
                var statusLevel = vaksin.Stok switch
                {
                    0 => "Level 0",
                    <= 2 => "Level 1",
                    _ => "Level 2"
                };

                alerts.Add(new AlertDto
                {
                    Type = "Stock",
                    Message = $"Stok vaksin {vaksin.NamaVaksin} {statusLevel} ({vaksin.Stok} tersisa) - Perlu Restock",
                    Severity = vaksin.Stok <= 2 ? "Critical" : "Warning",
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

            var avgMortalityRate = await CalculateAverageMortalityRateAsync(currentYear, currentMonth);
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
                    Status = p.StokKg == 0 ? "Level 0" : p.StokKg <= 10 ? "Level 1" : "Level 2"
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
                    Status = v.Stok == 0 ? "Level 0" : v.Stok <= 2 ? "Level 1" : "Level 2"
                })
                .ToListAsync();

            return new StockAlertsDto
            {
                LowStockPakan = lowStockPakan,
                LowStockVaksin = lowStockVaksin,
                CriticalStockCount = lowStockPakan.Count(p => p.Status == "Level 0" || p.Status == "Level 1") + lowStockVaksin.Count(v => v.Status == "Level 0" || v.Status == "Level 1"),
                WarningStockCount = lowStockPakan.Count(p => p.Status == "Level 2") + lowStockVaksin.Count(v => v.Status == "Level 2")
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

        private async Task<BusinessKpiDto> GetBusinessKpiAsync(int targetYear, int targetMonth)
        {
            var currentMonthStart = DateTime.SpecifyKind(new DateTime(targetYear, targetMonth, 1), DateTimeKind.Utc);
            var currentMonthEnd = currentMonthStart.AddMonths(1);

            var monthlyRevenue = await _context.Panens
                .Where(p => p.TanggalPanen >= currentMonthStart && p.TanggalPanen < currentMonthEnd)
                .SumAsync(p => p.JumlahEkorPanen * p.BeratRataRata * 25000);

            // Hitung biaya yang MATCH dengan ayam yang dipanen di bulan ini
            // Biaya dihitung dari TanggalMasuk ayam sampai TanggalPanen (bukan per bulan kalender)
            var monthlyExpenses = await CalculateMatchedExpensesAsync(targetYear, targetMonth);

            var monthlyProfit = monthlyRevenue - monthlyExpenses;

            // Calculate ROI (simplified calculation) - for the full year up to target month
            var yearlyExpenses = await _context.Biayas
                .Where(b => b.Tanggal.Year == targetYear &&
                           (b.Tanggal.Month < targetMonth || (b.Tanggal.Month == targetMonth && b.Tanggal < currentMonthEnd)))
                .SumAsync(b => b.Jumlah);
            var yearlyRevenue = await _context.Panens
                .Where(p => p.TanggalPanen.Year == targetYear &&
                           (p.TanggalPanen.Month < targetMonth || (p.TanggalPanen.Month == targetMonth && p.TanggalPanen < currentMonthEnd)))
                .SumAsync(p => p.JumlahEkorPanen * p.BeratRataRata * 25000);
            var roi = yearlyExpenses > 0 ? (double)(yearlyRevenue - yearlyExpenses) / (double)yearlyExpenses * 100 : 0;

            // Calculate actual ayam stock at the end of target month
            // Stock = Ayam masuk (cumulative up to end of month) - Mortalitas - Panen
            // Use cutoff: minimum of (today, end of target month) to avoid showing data for future months
            var today = DateTime.SpecifyKind(DateTime.UtcNow.Date.AddDays(1), DateTimeKind.Utc); // Start of tomorrow = end of today
            var stockCutoff = currentMonthEnd < today ? currentMonthEnd : today;

            // If target month is in the future (no data yet), stock should be 0
            int totalAyamStock = 0;
            if (stockCutoff > currentMonthStart)
            {
                var totalAyamMasuk = await _context.Ayams
                    .Where(a => a.TanggalMasuk < stockCutoff)
                    .SumAsync(a => a.JumlahMasuk);

                var totalMortalitas = await _context.Mortalitas
                    .Where(m => m.TanggalKematian < stockCutoff)
                    .SumAsync(m => m.JumlahKematian);

                var totalPanen = await _context.Panens
                    .Where(p => p.TanggalPanen < stockCutoff)
                    .SumAsync(p => p.JumlahEkorPanen);

                totalAyamStock = Math.Max(0, totalAyamMasuk - totalMortalitas - totalPanen);
            }

            // Calculate average productivity (simplified as average weight gain)
            var avgProductivity = await _context.Panens
                .Where(p => p.TanggalPanen >= currentMonthStart && p.TanggalPanen < currentMonthEnd)
                .AverageAsync(p => (double?)p.BeratRataRata) ?? 0;

            // Calculate FCR for the target month
            var fcr = await CalculateFeedConversionRatioAsync(targetYear, targetMonth);

            return new BusinessKpiDto
            {
                MonthlyRevenue = monthlyRevenue,
                MonthlyProfit = monthlyProfit,
                ReturnOnInvestment = Math.Round(roi, 1),
                TotalAyamStock = totalAyamStock,
                AverageProductivity = Math.Round(avgProductivity * 100, 1), // Convert to percentage
                FeedConversionRatio = fcr,
                CustomerSatisfaction = 4.2, // This would need a separate customer feedback system
                MarketShare = 15 // This would need market data
            };
        }

        private async Task<ProfitabilityDto> GetProfitabilityAsync(int targetYear, int targetMonth)
        {
            var currentMonthStart = DateTime.SpecifyKind(new DateTime(targetYear, targetMonth, 1), DateTimeKind.Utc);
            var currentMonthEnd = currentMonthStart.AddMonths(1);

            // Revenue dari panen di bulan target
            var revenue = await _context.Panens
                .Where(p => p.TanggalPanen >= currentMonthStart && p.TanggalPanen < currentMonthEnd)
                .SumAsync(p => p.JumlahEkorPanen * p.BeratRataRata * 25000);

            // Hitung biaya yang MATCH dengan ayam yang dipanen di bulan ini
            // Biaya dihitung dari TanggalMasuk ayam sampai TanggalPanen
            var operatingExpenses = await CalculateMatchedExpensesAsync(targetYear, targetMonth);

            var grossProfit = revenue;
            var netProfit = revenue - operatingExpenses;

            var totalKgPanen = await _context.Panens
                .Where(p => p.TanggalPanen >= currentMonthStart && p.TanggalPanen < currentMonthEnd)
                .SumAsync(p => p.JumlahEkorPanen * p.BeratRataRata);

            var costPerKg = totalKgPanen > 0 ? operatingExpenses / (decimal)totalKgPanen : 0;
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

        private async Task<ComparisonAnalysisDto> GetComparisonAnalysisAsync(int targetYear, int targetMonth)
        {
            var currentMonthDate = new DateTime(targetYear, targetMonth, 1);
            var previousMonthDate = currentMonthDate.AddMonths(-1);
            var previousYear = targetYear - 1;

            var currentMonthStart = DateTime.SpecifyKind(new DateTime(targetYear, targetMonth, 1), DateTimeKind.Utc);
            var currentMonthEnd = currentMonthStart.AddMonths(1);

            var previousMonthStart = DateTime.SpecifyKind(new DateTime(previousMonthDate.Year, previousMonthDate.Month, 1), DateTimeKind.Utc);
            var previousMonthEnd = previousMonthStart.AddMonths(1);

            // Current month data
            var currentMonthRevenue = await _context.Panens
                .Where(p => p.TanggalPanen >= currentMonthStart && p.TanggalPanen < currentMonthEnd)
                .SumAsync(p => p.JumlahEkorPanen * p.BeratRataRata * 25000);

            // Hitung biaya yang MATCH dengan ayam yang dipanen
            var currentMonthExpenses = await CalculateMatchedExpensesAsync(targetYear, targetMonth);

            // Previous month data
            var previousMonthRevenue = await _context.Panens
                .Where(p => p.TanggalPanen >= previousMonthStart && p.TanggalPanen < previousMonthEnd)
                .SumAsync(p => p.JumlahEkorPanen * p.BeratRataRata * 25000);

            var previousMonthExpenses = await CalculateMatchedExpensesAsync(previousMonthDate.Year, previousMonthDate.Month);

            // Calculate month-over-month changes
            var revenueChange = previousMonthRevenue > 0 ? (double)((currentMonthRevenue - previousMonthRevenue) / previousMonthRevenue) * 100 : 0;
            var currentMonthProfit = currentMonthRevenue - currentMonthExpenses;
            var previousMonthProfit = previousMonthRevenue - previousMonthExpenses;
            var profitChange = previousMonthProfit != 0 ? (double)((currentMonthProfit - previousMonthProfit) / Math.Abs(previousMonthProfit)) * 100 : 0;

            // Year-to-date comparison
            var currentYearRevenue = await _context.Panens
                .Where(p => p.TanggalPanen.Year == targetYear && p.TanggalPanen < currentMonthEnd)
                .SumAsync(p => p.JumlahEkorPanen * p.BeratRataRata * 25000);

            var previousYearRevenue = await _context.Panens
                .Where(p => p.TanggalPanen.Year == previousYear && p.TanggalPanen < currentMonthEnd.AddYears(-1))
                .SumAsync(p => p.JumlahEkorPanen * p.BeratRataRata * 25000);

            var revenueGrowth = previousYearRevenue > 0 ? (double)((currentYearRevenue - previousYearRevenue) / previousYearRevenue) * 100 : 0;

            // Get current mortality rate and FCR
            var currentMortalityRate = await CalculateAverageMortalityRateAsync(targetYear, targetMonth);
            var currentFcr = await CalculateFeedConversionRatioAsync(targetYear, targetMonth);

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
                    IndustryAvgFcr = 1.7, // Industry standard (1.6-1.8)
                    YourFcr = currentFcr,
                    PerformanceRating = currentMortalityRate < 5.5 ? "Above Average" : "Below Average"
                }
            };
        }

        private async Task<StrategicInsightsDto> GetStrategicInsightsAsync(int targetYear, int targetMonth)
        {
            var recommendations = new List<string>();
            var opportunities = new List<string>();
            var risks = new List<string>();

            // Analyze current performance and generate insights
            var avgMortalityRate = await CalculateAverageMortalityRateAsync(targetYear, targetMonth);
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

        private async Task<MonthlyTrendsDto> GetMonthlyTrendsAsync(int targetYear, int targetMonth)
        {
            var revenueData = new List<MonthlyDataDto>();
            var profitData = new List<MonthlyDataDto>();
            var fcrData = new List<MonthlyFcrDataDto>();
            var targetDate = new DateTime(targetYear, targetMonth, 1);

            // Get last 6 months data ending at target month
            for (int i = 5; i >= 0; i--)
            {
                var date = targetDate.AddMonths(-i);
                var monthStart = DateTime.SpecifyKind(new DateTime(date.Year, date.Month, 1), DateTimeKind.Utc);
                var monthEnd = monthStart.AddMonths(1);

                var monthRevenue = await _context.Panens
                    .Where(p => p.TanggalPanen >= monthStart && p.TanggalPanen < monthEnd)
                    .SumAsync(p => p.JumlahEkorPanen * p.BeratRataRata * 25000);

                // Hitung biaya yang MATCH dengan ayam yang dipanen di bulan ini
                var monthExpense = await CalculateMatchedExpensesAsync(date.Year, date.Month);

                var monthProfit = monthRevenue - monthExpense;

                // Calculate FCR for this month
                var monthFcr = await CalculateFeedConversionRatioAsync(date.Year, date.Month);

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

                // FCR status: Excellent (<1.6), Good (1.6-1.8), Fair (1.8-2.0), Poor (>2.0)
                var fcrStatus = monthFcr switch
                {
                    0 => "No Data",
                    < 1.6 => "Excellent",
                    <= 1.8 => "Good",
                    <= 2.0 => "Fair",
                    _ => "Poor"
                };

                fcrData.Add(new MonthlyFcrDataDto
                {
                    Month = date.ToString("MMM yyyy"),
                    FcrValue = monthFcr,
                    Status = fcrStatus
                });
            }

            return new MonthlyTrendsDto
            {
                RevenueData = revenueData,
                ProfitData = profitData,
                FcrData = fcrData
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

        private async Task<double> CalculateAverageMortalityRateAsync(int targetYear, int targetMonth)
        {
            var monthStart = DateTime.SpecifyKind(new DateTime(targetYear, targetMonth, 1), DateTimeKind.Utc);
            var monthEnd = monthStart.AddMonths(1);

            var totalAyams = await _context.Ayams.SumAsync(a => a.JumlahMasuk);
            var totalMortality = await _context.Mortalitas
                .Where(m => m.TanggalKematian >= monthStart && m.TanggalKematian < monthEnd)
                .SumAsync(m => m.JumlahKematian);

            return totalAyams > 0 ? (double)totalMortality / totalAyams * 100 : 0;
        }

        private async Task<double> CalculateFeedConversionRatioAsync(int targetYear, int targetMonth)
        {
            var monthStart = DateTime.SpecifyKind(new DateTime(targetYear, targetMonth, 1), DateTimeKind.Utc);
            var monthEnd = monthStart.AddMonths(1);

            // Calculate total feed consumed this month (in kg)
            var totalFeedConsumed = await _context.Operasionals
                .Where(o => o.Tanggal >= monthStart && o.Tanggal < monthEnd && o.PakanId != null)
                .SumAsync(o => o.Jumlah);

            // Calculate total weight gained (in kg) = jumlah ekor * berat rata-rata
            var totalWeightGained = await _context.Panens
                .Where(p => p.TanggalPanen >= monthStart && p.TanggalPanen < monthEnd)
                .SumAsync(p => p.JumlahEkorPanen * p.BeratRataRata);

            // FCR = Total Pakan (kg) / Total Berat Ayam (kg)
            // Lower is better (ideal: 1.5-1.8)
            if (totalWeightGained > 0)
            {
                return Math.Round((double)totalFeedConsumed / (double)totalWeightGained, 2);
            }

            // Return 0 if no data (not default 1.8, karena itu menyesatkan)
            return 0;
        }

        // Overload for backward compatibility (uses current month)
        private async Task<double> CalculateFeedConversionRatioAsync()
        {
            var now = DateTime.UtcNow;
            return await CalculateFeedConversionRatioAsync(now.Year, now.Month);
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

        /// <summary>
        /// Menghitung biaya yang MATCH dengan ayam yang dipanen di bulan target.
        /// Biaya dihitung dari TanggalMasuk ayam sampai TanggalPanen.
        /// Ini memastikan profit lebih akurat karena biaya di-match dengan revenue batch yang sama.
        /// </summary>
        private async Task<decimal> CalculateMatchedExpensesAsync(int targetYear, int targetMonth)
        {
            var currentMonthStart = DateTime.SpecifyKind(new DateTime(targetYear, targetMonth, 1), DateTimeKind.Utc);
            var currentMonthEnd = currentMonthStart.AddMonths(1);

            // Ambil semua panen di bulan target beserta data Ayam-nya
            var panensWithAyam = await _context.Panens
                .Include(p => p.Ayam)
                .Where(p => p.TanggalPanen >= currentMonthStart && p.TanggalPanen < currentMonthEnd)
                .ToListAsync();

            if (!panensWithAyam.Any())
            {
                return 0;
            }

            decimal totalMatchedExpenses = 0;

            // Untuk setiap panen, hitung biaya yang terjadi selama periode perawatan ayam
            foreach (var panen in panensWithAyam)
            {
                if (panen.Ayam == null) continue;

                var ayamMasuk = DateTime.SpecifyKind(panen.Ayam.TanggalMasuk.Date, DateTimeKind.Utc);
                var ayamPanen = DateTime.SpecifyKind(panen.TanggalPanen.Date.AddDays(1), DateTimeKind.Utc); // Include panen day
                var kandangId = panen.Ayam.KandangId;

                // Hitung biaya yang terkait dengan kandang ini selama periode perawatan
                // Biaya dengan KandangId yang sama
                var kandangExpenses = await _context.Biayas
                    .Where(b => b.KandangId == kandangId &&
                               b.Tanggal >= ayamMasuk &&
                               b.Tanggal < ayamPanen)
                    .SumAsync(b => b.Jumlah);

                // Biaya dari Operasional (pakan, vaksin) untuk kandang ini
                var operasionalExpenses = await _context.Biayas
                    .Where(b => b.OperasionalId != null &&
                               b.Operasional != null &&
                               b.Operasional.KandangId == kandangId &&
                               b.Tanggal >= ayamMasuk &&
                               b.Tanggal < ayamPanen)
                    .SumAsync(b => b.Jumlah);

                // Biaya shared (tanpa KandangId dan tanpa OperasionalId) - di-prorate berdasarkan jumlah kandang aktif
                var activeKandangCount = await _context.Kandangs.CountAsync();
                if (activeKandangCount > 0)
                {
                    var sharedExpenses = await _context.Biayas
                        .Where(b => b.KandangId == null &&
                                   b.OperasionalId == null &&
                                   b.Tanggal >= ayamMasuk &&
                                   b.Tanggal < ayamPanen)
                        .SumAsync(b => b.Jumlah);

                    // Alokasikan shared cost per kandang
                    totalMatchedExpenses += sharedExpenses / activeKandangCount;
                }

                totalMatchedExpenses += kandangExpenses + operasionalExpenses;
            }

            return totalMatchedExpenses;
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
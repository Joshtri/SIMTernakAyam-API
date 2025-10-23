# Dashboard Petugas API Fixes

## Issues Fixed

### 1. **Mortality Data Not Showing (mortalityToday, mortalityThisWeek)**

**Problem**: Mortality data was returning 0 even when mortality records existed in the database.

**Root Cause**:
- Date comparison using `.Date` property doesn't work correctly with Entity Framework in all scenarios
- Missing `Include()` for the `Ayam` navigation property in mortality queries

**Fixes Applied**:
- Changed date comparison from `m.TanggalKematian.Date == today.Date` to explicit year/month/day comparison
- Added `.Include(m => m.Ayam)` to all mortality queries to ensure the navigation property is loaded
- Updated mortalityThisWeek to include proper date range with both start and end bounds

**Location**: `Services/DashboardService.cs:305-317`

```csharp
// Before
var mortalityToday = await _context.Mortalitas
    .Where(m => m.Ayam.KandangId == kandang.Id && m.TanggalKematian.Date == today.Date)
    .SumAsync(m => m.JumlahKematian);

// After
var mortalityToday = await _context.Mortalitas
    .Include(m => m.Ayam)
    .Where(m => m.Ayam.KandangId == kandang.Id &&
               m.TanggalKematian.Year == today.Year &&
               m.TanggalKematian.Month == today.Month &&
               m.TanggalKematian.Day == today.Day)
    .SumAsync(m => m.JumlahKematian);
```

### 2. **Current Ayams Count Issue**

**Problem**: `currentAyams` field was showing 0 even when chickens exist in the kandang.

**Root Cause**:
- The totalMortality query was missing `.Include(m => m.Ayam)` for proper navigation

**Fix Applied**:
- Added `.Include(m => m.Ayam)` to the totalMortality query

**Location**: `Services/DashboardService.cs:298-301`

### 3. **JurnalHarian Query Logic Issues**

**Problem**: Last feed time and vaccination time were not being retrieved correctly.

**Root Cause**:
- OR operator precedence issue - queries like `j.JudulKegiatan.ToLower().Contains("pakan") || j.DeskripsiKegiatan.ToLower().Contains("pakan")` were being evaluated incorrectly without proper parentheses

**Fixes Applied**:
- Added proper parentheses around OR conditions
- Added additional search terms ("feed", "vaccin", "clean") for better matching

**Location**: `Services/DashboardService.cs:318-338`

```csharp
// Before
var lastFeedJurnal = await _context.JurnalHarians
    .Where(j => j.KandangId == kandang.Id &&
               j.JudulKegiatan.ToLower().Contains("pakan") || j.DeskripsiKegiatan.ToLower().Contains("pakan"))
    .OrderByDescending(j => j.Tanggal)
    .ThenByDescending(j => j.WaktuSelesai)
    .FirstOrDefaultAsync();

// After
var lastFeedJurnal = await _context.JurnalHarians
    .Where(j => j.KandangId == kandang.Id &&
               (j.JudulKegiatan.ToLower().Contains("pakan") ||
                j.DeskripsiKegiatan.ToLower().Contains("pakan") ||
                j.JudulKegiatan.ToLower().Contains("feed")))
    .OrderByDescending(j => j.Tanggal)
    .ThenByDescending(j => j.WaktuSelesai)
    .FirstOrDefaultAsync();
```

### 4. **Daily Tasks Not Populating**

**Problem**: `dailyTasks` data was empty or incorrect.

**Root Cause**:
- Date comparison issue in `GetDailyTasksAsync` using `.Date` property

**Fix Applied**:
- Changed to explicit year/month/day comparison for today's jurnal harian

**Location**: `Services/DashboardService.cs:368-373`

### 5. **Upcoming Activities Not Showing**

**Problem**: `upcomingActivities` was empty even when tasks were pending.

**Root Causes**:
- Date comparison issues when checking if tasks were completed today
- OR operator precedence issues in activity type filtering

**Fixes Applied**:
- Changed from `.Date` comparison to explicit year/month/day comparison
- Added proper parentheses around OR conditions
- Improved activity detection logic with additional search terms
- Created separate boolean variables for clearer logic

**Location**: `Services/DashboardService.cs:524-578`

## Summary of Changes

### Files Modified:
- `Services/DashboardService.cs`

### Key Improvements:
1. All date comparisons now use explicit year/month/day comparison instead of `.Date`
2. All navigation properties are properly included with `.Include()`
3. All OR conditions have proper parentheses for correct evaluation
4. Enhanced search terms for better activity matching (pakan/feed, vaksin/vaccin, bersih/clean)

## Testing Recommendations

1. **Test with existing mortality data**:
   - Create mortality records for today
   - Create mortality records for this week
   - Verify they appear in the API response

2. **Test with JurnalHarian data**:
   - Create feeding journal entries
   - Create vaccination journal entries
   - Create cleaning journal entries
   - Verify lastFeedTime and lastVaccinationTime are populated

3. **Test daily tasks**:
   - Create some journal entries for today
   - Verify completedTasks count is correct
   - Verify pending tasks are calculated properly

4. **Test upcoming activities**:
   - Don't create any feeding journal for today
   - Verify feeding activity appears in todayActivities
   - Create a feeding journal entry
   - Verify it disappears from upcoming activities

## Expected API Response After Fixes

```json
{
    "myKandangs": [{
        "mortalityToday": <actual_count>,
        "mortalityThisWeek": <actual_count>,
        "currentAyams": <actual_count>,
        "lastFeedTime": "<actual_datetime>",
        "lastVaccinationTime": "<actual_datetime>"
    }],
    "dailyTasks": {
        "completedTasks": <actual_count>,
        "pendingFeedings": <calculated_count>
    },
    "upcomingActivities": {
        "todayActivities": [<actual_activities>]
    }
}
```

All calculations now properly use JurnalHarian data as the primary source for activity tracking.

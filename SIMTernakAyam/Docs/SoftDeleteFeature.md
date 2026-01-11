# Soft Delete Feature - Mencegah Stok Ayam Kembali Saat Data Dihapus

## ?? Problem Statement

### Masalah Sebelumnya:
Ketika user menghapus data **Panen** atau **Mortalitas**, stok ayam di kandang **kembali bertambah** karena sistem menghitung real-time dari database.

**Contoh Kasus:**
```
Kondisi Awal:
- Ayam Masuk: 1000 ekor
- Panen: 300 ekor
- Mortalitas: 100 ekor
- Sisa: 600 ekor ?

User Hapus Data Panen (300 ekor):
- Sisa jadi: 900 ekor ? (SALAH! Ayam yang sudah dipanen tidak bisa balik lagi!)

User Hapus Data Mortalitas (100 ekor):
- Sisa jadi: 1000 ekor ? (SALAH! Ayam yang sudah mati tidak bisa hidup lagi!)
```

###  Problem Root Cause:
Method `GetCurrentAyamCountAsync` menghitung stok secara real-time:
```csharp
// BEFORE (Wrong)
var totalAyam = await _context.Ayams.Where(a => a.KandangId == kandangId).SumAsync(a => a.JumlahMasuk);
var totalMortalitas = await _context.Mortalitas.Where(m => m.Ayam.KandangId == kandangId).SumAsync(m => m.JumlahKematian);
var totalPanen = await _context.Panens.Where(p => p.Ayam.KandangId == kandangId).SumAsync(p => p.JumlahEkorPanen);

return totalAyam - totalMortalitas - totalPanen; // Ketika data dihapus, nilai ini berubah!
```

## ? Solution: Soft Delete

### Konsep Soft Delete
**Soft Delete** = Data **tidak benar-benar dihapus** dari database, hanya ditandai sebagai "deleted" dengan flag `IsDeleted = true`.

**Benefit:**
1. ? **Stok tidak berubah** ketika data "dihapus"
2. ? **Data audit tetap terjaga** (bisa tracking siapa hapus kapan)
3. ? **Bisa restore** kalau ternyata salah hapus
4. ? **Comply dengan regulasi** (data keuangan tidak boleh dihapus permanen)

### Implementasi

#### 1. Update `BaseModel.cs`
```csharp
public abstract class BaseModel
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdateAt { get; set; } = DateTime.UtcNow;
    
    // ? NEW: Soft Delete Support
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; } // User ID yang menghapus
}
```

#### 2. Update `BaseRepository.cs` - Filter Soft Deleted Records & Implement Soft Delete
```csharp
/// <summary>
/// ? SOFT DELETE: Mark entity as deleted instead of removing it
/// </summary>
public virtual void Delete(T entity)
{
    // Instead of removing, mark as deleted
    entity.IsDeleted = true;
    entity.DeletedAt = DateTime.UtcNow;
    
    // Update the entity state
    var entry = _context.Entry(entity);
    if (entry.State == EntityState.Detached)
    {
        _database.Attach(entity);
    }
    entry.State = EntityState.Modified;
}

public virtual async Task<IEnumerable<T>> GetAllAsync()
{
    // ? Filter out soft-deleted records
    return await _database.Where(e => !e.IsDeleted).ToListAsync();
}

public virtual async Task<T?> GetByIdAsync(Guid id)
{
    // ? Filter out soft-deleted records
    return await _database.FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);
}

/// <summary>
/// ? HARD DELETE: Permanently remove entity from database (use with caution!)
/// </summary>
public virtual void HardDelete(T entity)
{
    _database.Remove(entity);
}

/// <summary>
/// ? RESTORE: Restore soft-deleted entity
/// </summary>
public virtual void Restore(T entity)
{
    entity.IsDeleted = false;
    entity.DeletedAt = null;
    
    var entry = _context.Entry(entity);
    if (entry.State == EntityState.Detached)
    {
        _database.Attach(entity);
    }
    entry.State = EntityState.Modified;
}
```

#### 3. Update `BaseService.cs` - Delete Method Now Uses Soft Delete
```csharp
public virtual async Task<(bool Success, string Message)> DeleteAsync(Guid id)
{
    var entity = await _repository.GetByIdAsync(id);
    if (entity == null)
    {
        return (false, $"{typeof(T).Name} tidak ditemukan.");
    }

    // Validate before delete
    var validateResult = await ValidateOnDeleteAsync(entity);
    if (!validateResult.IsValid)
    {
        return (false, validateResult.ErrorMessage);
    }

    // ? SOFT DELETE: Tandai sebagai deleted, jangan benar-benar hapus
    // BaseRepository.Delete() sekarang otomatis melakukan soft delete
    _repository.Delete(entity);
    await _repository.SaveChangesAsync();

    return (true, $"{typeof(T).Name} berhasil dihapus.");
}
```

#### 4. Update `KandangRepository.cs` - Exclude Soft Deleted from Stock Calculation
```csharp
public async Task<int> GetCurrentAyamCountAsync(Guid kandangId)
{
    // Total ayam yang masuk (exclude yang soft deleted)
    var totalAyam = await _context.Ayams
        .Where(a => a.KandangId == kandangId && !a.IsDeleted)
        .SumAsync(a => a.JumlahMasuk);

    // Total mortalitas (exclude yang soft deleted)
    var totalMortalitas = await _context.Mortalitas
        .Where(m => m.Ayam.KandangId == kandangId && !m.IsDeleted && !m.Ayam.IsDeleted)
        .SumAsync(m => m.JumlahKematian);

    // Total panen (exclude yang soft deleted)
    var totalPanen = await _context.Panens
        .Where(p => p.Ayam.KandangId == kandangId && !p.IsDeleted && !p.Ayam.IsDeleted)
        .SumAsync(p => p.JumlahEkorPanen);

    return totalAyam - totalMortalitas - totalPanen;
}
```

## ?? Bug Fix: Mortalitas Delete Not Working

### Masalah yang Ditemukan
```
Request: DELETE /api/mortalitas/bdc92b78-59a5-45e3-85cf-32a8ada2efc6

Response:
{
    "success": true,
    "message": "Mortalitas berhasil dihapus.",
    "statusCode": 200
}

Tetapi data TIDAK terhapus (bahkan soft delete tidak berfungsi) ?
```

### Root Cause
`BaseRepository.Delete()` masih menggunakan **hard delete** (`.Remove()`) yang sebenarnya:
```csharp
// BEFORE (WRONG!)
public virtual void Delete(T entity)
{
    _database.Remove(entity);  // ? Hard delete!
}
```

### Solution
Ubah method `Delete()` menjadi soft delete:
```csharp
// AFTER (CORRECT!)
public virtual void Delete(T entity)
{
    // ? Soft delete instead of hard delete
    entity.IsDeleted = true;
    entity.DeletedAt = DateTime.UtcNow;
    
    var entry = _context.Entry(entity);
    if (entry.State == EntityState.Detached)
    {
        _database.Attach(entity);
    }
    entry.State = EntityState.Modified;
}
```

### Testing After Fix
```
1. DELETE /api/mortalitas/{id}
2. Database check:
   SELECT IsDeleted, DeletedAt FROM Mortalitas WHERE Id = '{id}'
   ? IsDeleted = true ?
   ? DeletedAt = current timestamp ?
3. GET /api/mortalitas
   ? Record tidak muncul lagi ?
4. Check stok kandang
   ? Stok tidak berubah ?
```

## ?? Before vs After Comparison

### BEFORE (Without Soft Delete):
```
Initial State:
- Ayam Masuk: 1000 ekor
- Panen: 300 ekor
- Mortalitas: 100 ekor
- Sisa: 600 ekor

Action: DELETE Panen (300 ekor)
Result: Sisa = 900 ekor ? (WRONG!)

Action: DELETE Mortalitas (100 ekor)
Result: Sisa = 1000 ekor ? (WRONG!)
```

### AFTER (With Soft Delete - FIXED):
```
Initial State:
- Ayam Masuk: 1000 ekor
- Panen: 300 ekor (IsDeleted=false)
- Mortalitas: 100 ekor (IsDeleted=false)
- Sisa: 600 ekor

Action: DELETE Panen (300 ekor)
Database: Panen.IsDeleted = true, DeletedAt = timestamp
Result: Sisa = 600 ekor ? (CORRECT! Tetap sama)

Action: DELETE Mortalitas (100 ekor)
Database: Mortalitas.IsDeleted = true, DeletedAt = timestamp
Result: Sisa = 600 ekor ? (CORRECT! Tetap sama)
```

## ?? Use Cases

### 1. **Normal Delete (Soft Delete)**
```csharp
// User klik button "Hapus"
await _mortalitasService.DeleteAsync(mortalitasId);

// Database:
// UPDATE Mortalitas 
// SET IsDeleted = 1, DeletedAt = '2024-04-01 10:00:00' 
// WHERE Id = @mortalitasId

// Stok ayam TIDAK BERUBAH ?
```

### 2. **Hard Delete (Admin Only)**
```csharp
// Admin perlu hapus permanen (jarang digunakan)
// Harus manual call HardDelete() method
_repository.HardDelete(entity);
await _repository.SaveChangesAsync();

// Database:
// DELETE FROM Mortalitas WHERE Id = @mortalitasId

// Data benar-benar hilang dari database
```

### 3. **View Deleted Records (Admin)**
```csharp
// Admin bisa lihat data yang sudah dihapus
var deletedRecords = await _context.Mortalitas
    .IgnoreQueryFilters()  // Bypass soft delete filter
    .Where(m => m.IsDeleted)
    .OrderByDescending(m => m.DeletedAt)
    .ToListAsync();
```

### 4. **Restore Deleted Records**
```csharp
// Restore data yang salah dihapus
var entity = await _context.Mortalitas
    .IgnoreQueryFilters()
    .FirstOrDefaultAsync(m => m.Id == id);
    
if (entity != null && entity.IsDeleted)
{
    _repository.Restore(entity);
    await _repository.SaveChangesAsync();
}
```

## ??? Database Changes

### Migration: `AddSoftDeleteToBaseModel`

Menambahkan kolom baru ke semua tabel yang inherit dari `BaseModel`:

```sql
ALTER TABLE Ayams ADD IsDeleted BIT NOT NULL DEFAULT 0;
ALTER TABLE Ayams ADD DeletedAt DATETIME NULL;
ALTER TABLE Ayams ADD DeletedBy UNIQUEIDENTIFIER NULL;

ALTER TABLE Panens ADD IsDeleted BIT NOT NULL DEFAULT 0;
ALTER TABLE Panens ADD DeletedAt DATETIME NULL;
ALTER TABLE Panens ADD DeletedBy UNIQUEIDENTIFIER NULL;

ALTER TABLE Mortalitas ADD IsDeleted BIT NOT NULL DEFAULT 0;
ALTER TABLE Mortalitas ADD DeletedAt DATETIME NULL;
ALTER TABLE Mortalitas ADD DeletedBy UNIQUEIDENTIFIER NULL;

-- Dan semua tabel lain yang extend BaseModel
```

### Initialization Script
Run `InitializeSoftDelete.sql` untuk set default values pada existing data:
```bash
# Run initialization script
sqlcmd -S localhost -d SIMTernakAyam -i Scripts/InitializeSoftDelete.sql
```

## ?? API Impact

### Endpoint Behavior Changes

#### 1. **DELETE Endpoints**
**Before:**
```http
DELETE /api/mortalitas/{id}
? Hard delete dari database (TIDAK JALAN!) ?
? Response success tapi data masih ada
```

**After:**
```http
DELETE /api/mortalitas/{id}
? Soft delete (IsDeleted = true) ?
? Response success DAN data ter-soft delete
? Stok ayam TETAP ?
```

#### 2. **GET Endpoints**
**Before:**
```http
GET /api/mortalitas
? Menampilkan SEMUA mortalitas (including deleted)
```

**After:**
```http
GET /api/mortalitas
? Hanya menampilkan mortalitas yang IsDeleted = false
? Otomatis difilter oleh BaseRepository
```

#### 3. **Stock Calculation**
**Before:**
```csharp
Sisa = TotalMasuk - SUM(Mortalitas) - SUM(Panen)
// Jika ada yang dihapus, SUM berkurang ? Sisa bertambah ?
```

**After:**
```csharp
Sisa = TotalMasuk - SUM(Mortalitas WHERE IsDeleted=false) - SUM(Panen WHERE IsDeleted=false)
// Data yang dihapus diabaikan ? Sisa tetap ?
```

## ?? Important Notes

### 1. **Data Existing**
Untuk data yang sudah ada sebelum fitur ini diimplementasi, jalankan script:
```bash
# Run initialization script
sqlcmd -S localhost -d SIMTernakAyam -i Scripts/InitializeSoftDelete.sql
```

### 2. **Relational Data**
Ketika parent entity dihapus (soft delete), child entities **TIDAK otomatis** dihapus.

**Example:**
```csharp
// Jika Ayam di-soft delete:
Ayam.IsDeleted = true;

// Mortalitas dari ayam tersebut TETAP visible!
// Perlu filter tambahan di query:
.Where(m => !m.IsDeleted && !m.Ayam.IsDeleted)
```

### 3. **Query Performance**
Soft delete menambah kondisi `WHERE IsDeleted = false` di setiap query.

**Optimization - Index Creation:**
Jalankan script `InitializeSoftDelete.sql` yang sudah include:
```sql
CREATE INDEX IX_Ayams_IsDeleted ON Ayams(IsDeleted);
CREATE INDEX IX_Panens_IsDeleted ON Panens(IsDeleted);
CREATE INDEX IX_Mortalitas_IsDeleted ON Mortalitas(IsDeleted);
-- dst...
```

### 4. **Storage Space**
Soft delete **TIDAK menghapus data** dari database, jadi:
- Database size tetap bertambah
- Perlu periodic cleanup (archive old deleted records)
- Consider auto-purge policy (e.g., hard delete after 1 year)

## ?? Admin Features (Future Enhancement)

### 1. **View Deleted Records**
```http
GET /api/admin/mortalitas/deleted
```

### 2. **Restore Deleted Record**
```http
POST /api/admin/mortalitas/{id}/restore
```

### 3. **Hard Delete (Permanent)**
```http
DELETE /api/admin/mortalitas/{id}/permanent
```

## ? Testing

### Test Scenarios:

#### Test 1: Delete Mortalitas, Check Stock
```
1. Ayam masuk: 1000 ekor
2. Buat mortalitas: 100 ekor
3. Check stok ? Harus: 900 ekor
4. DELETE mortalitas ? Success response
5. Check database ? IsDeleted = true ?
6. Check stok ? Harus: 900 ekor (TETAP!) ?
7. GET /api/mortalitas ? Data tidak muncul ?
```

#### Test 2: Delete Panen, Check Stock
```
1. Ayam masuk: 1000 ekor
2. Buat panen: 300 ekor
3. Check stok ? Harus: 700 ekor
4. DELETE panen
5. Check stok ? Harus: 700 ekor (TETAP!) ?
```

#### Test 3: Multiple Deletes
```
1. Ayam masuk: 1000 ekor
2. Panen 300 + Mortalitas 100
3. Stok: 600 ekor
4. DELETE panen ? Stok: 600 ekor ?
5. DELETE mortalitas ? Stok: 600 ekor ?
```

#### Test 4: Verify Database State
```sql
-- Check soft delete working
SELECT Id, JumlahKematian, IsDeleted, DeletedAt 
FROM Mortalitas 
WHERE Id = 'bdc92b78-59a5-45e3-85cf-32a8ada2efc6'

-- Should show:
-- IsDeleted = 1
-- DeletedAt = [timestamp]
```

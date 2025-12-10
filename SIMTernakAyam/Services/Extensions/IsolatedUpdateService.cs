using Microsoft.EntityFrameworkCore;
using SIMTernakAyam.Data;
using SIMTernakAyam.Models;

namespace SIMTernakAyam.Services.Extensions
{
    /// <summary>
    /// Service extension untuk mengatasi entity tracking conflicts
    /// dengan menggunakan separate DbContext untuk operasi yang berpotensi conflict
    /// </summary>
    public class IsolatedUpdateService
    {
        private readonly IServiceProvider _serviceProvider;

        public IsolatedUpdateService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Update operasional dengan isolated context untuk menghindari tracking conflicts
        /// </summary>
        public async Task<(bool Success, string Message)> UpdateOperasionalIsolated(Operasional entity)
        {
            try
            {
                // Create separate scope untuk isolated operation
                using var scope = _serviceProvider.CreateScope();
                using var isolatedContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                
                // Disable change tracking untuk performa dan menghindari conflicts
                isolatedContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
                
                // Get existing entity tanpa tracking
                var existingEntity = await isolatedContext.Operasionals
                    .AsNoTracking()
                    .FirstOrDefaultAsync(o => o.Id == entity.Id);
                
                if (existingEntity == null)
                {
                    return (false, "Data operasional tidak ditemukan.");
                }
                
                // Preserve timestamps
                entity.CreatedAt = existingEntity.CreatedAt;
                entity.UpdateAt = DateTime.UtcNow;
                
                // Re-enable tracking untuk update operation
                isolatedContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll;
                
                // Update dengan fresh context
                isolatedContext.Operasionals.Update(entity);
                await isolatedContext.SaveChangesAsync();
                
                // Process stock updates in separate transactions jika perlu
                await ProcessStockUpdatesIsolated(entity, existingEntity);
                
                // Process biaya updates in separate transactions jika perlu  
                await ProcessBiayaUpdatesIsolated(entity);
                
                return (true, "Data berhasil diupdate dengan isolated context.");
            }
            catch (Exception ex)
            {
                return (false, $"Terjadi kesalahan dalam isolated update: {ex.Message}");
            }
        }

        private async Task ProcessStockUpdatesIsolated(Operasional newEntity, Operasional existingEntity)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var stokService = scope.ServiceProvider.GetRequiredService<Services.Interfaces.IStokService>();
                
                // Restore original stock jika ada
                if (existingEntity.PakanId.HasValue)
                {
                    await stokService.TambahStokPakan(existingEntity.PakanId.Value, 
                                                    existingEntity.Tanggal, 
                                                    existingEntity.Jumlah);
                }
                
                if (existingEntity.VaksinId.HasValue)
                {
                    await stokService.TambahStokVaksin(existingEntity.VaksinId.Value, 
                                                     existingEntity.Tanggal, 
                                                     existingEntity.Jumlah);
                }
                
                // Apply new stock changes
                if (newEntity.PakanId.HasValue)
                {
                    await stokService.KurangiStokPakan(newEntity.PakanId.Value, 
                                                     newEntity.Tanggal, 
                                                     newEntity.Jumlah);
                }
                
                if (newEntity.VaksinId.HasValue)
                {
                    await stokService.KurangiStokVaksin(newEntity.VaksinId.Value, 
                                                      newEntity.Tanggal, 
                                                      newEntity.Jumlah);
                }
            }
            catch (Exception ex)
            {
                // Log error tapi jangan fail main operation
                Console.WriteLine($"Warning: Stock update failed in isolated context: {ex.Message}");
            }
        }

        private async Task ProcessBiayaUpdatesIsolated(Operasional entity)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                using var isolatedContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                
                // Get jenis kegiatan tanpa tracking
                var jenisKegiatan = await isolatedContext.JenisKegiatans
                    .AsNoTracking()
                    .FirstOrDefaultAsync(jk => jk.Id == entity.JenisKegiatanId);
                
                if (jenisKegiatan == null) return;
                
                var biayaPerUnit = jenisKegiatan.BiayaDefault ?? 0m;
                var totalBiaya = biayaPerUnit * entity.Jumlah;
                
                // Check existing biaya tanpa tracking
                var existingBiaya = await isolatedContext.Biayas
                    .AsNoTracking()
                    .FirstOrDefaultAsync(b => b.OperasionalId == entity.Id);
                
                if (existingBiaya == null)
                {
                    // Create new biaya
                    var newBiaya = new Biaya
                    {
                        JenisBiaya = jenisKegiatan.NamaKegiatan ?? "Operasional",
                        Tanggal = entity.Tanggal,
                        Jumlah = totalBiaya,
                        PetugasId = entity.PetugasId,
                        OperasionalId = entity.Id,
                        KandangId = entity.KandangId,
                        Bulan = entity.Tanggal.Month,
                        Tahun = entity.Tanggal.Year,
                        Catatan = jenisKegiatan.Deskripsi
                    };
                    
                    isolatedContext.Biayas.Add(newBiaya);
                }
                else
                {
                    // Update existing biaya
                    existingBiaya.JenisBiaya = jenisKegiatan.NamaKegiatan ?? existingBiaya.JenisBiaya;
                    existingBiaya.Tanggal = entity.Tanggal;
                    existingBiaya.Jumlah = totalBiaya;
                    existingBiaya.PetugasId = entity.PetugasId;
                    existingBiaya.KandangId = entity.KandangId;
                    existingBiaya.Bulan = entity.Tanggal.Month;
                    existingBiaya.Tahun = entity.Tanggal.Year;
                    existingBiaya.Catatan = jenisKegiatan.Deskripsi;
                    existingBiaya.UpdateAt = DateTime.UtcNow;
                    
                    isolatedContext.Biayas.Update(existingBiaya);
                }
                
                await isolatedContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Log error tapi jangan fail main operation
                Console.WriteLine($"Warning: Biaya update failed in isolated context: {ex.Message}");
            }
        }
    }
}
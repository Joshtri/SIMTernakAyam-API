using Microsoft.EntityFrameworkCore;
using SIMTernakAyam.Models;

namespace SIMTernakAyam.Data
{
    public class ApplicationDbContext : DbContext
    {

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            
        }

        //set all the model on here.

        public DbSet<User> Users { get; set; }
        public DbSet<Kandang> Kandangs { get; set; }
        public DbSet<Ayam> Ayams { get; set; }
        public DbSet<Mortalitas> Mortalitas { get; set; }
        public DbSet<Panen> Panens { get; set; }
        public DbSet<Operasional> Operasionals { get; set; }
        public DbSet<Biaya> Biayas { get; set; }
        public DbSet<JenisKegiatan> JenisKegiatans { get; set; }
        public DbSet<Pakan> Pakans { get; set; }
        public DbSet<Vaksin> Vaksins { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<KandangAsisten> KandangAsistens { get; set; }
        public DbSet<JurnalHarian> JurnalHarians { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Relasi User -> Kandang (One-to-Many)
            modelBuilder.Entity<Kandang>()
                .HasOne(k => k.User)
                .WithMany(u => u.Kandangs)
                .HasForeignKey(k => k.petugasId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relasi Kandang -> Ayam (One-to-Many)
            modelBuilder.Entity<Ayam>()
                .HasOne(a => a.Kandang)
                .WithMany()
                .HasForeignKey(a => a.KandangId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relasi Ayam -> Mortalitas (One-to-Many)
            modelBuilder.Entity<Mortalitas>()
                .HasOne(m => m.Ayam)
                .WithMany()
                .HasForeignKey(m => m.AyamId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relasi Ayam -> Panen (One-to-Many)
            modelBuilder.Entity<Panen>()
                .HasOne(p => p.Ayam)
                .WithMany()
                .HasForeignKey(p => p.AyamId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relasi User -> Biaya (One-to-Many)
            modelBuilder.Entity<Biaya>()
                .HasOne(b => b.Petugas)
                .WithMany()
                .HasForeignKey(b => b.PetugasId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relasi Kandang -> Biaya (One-to-Many, optional)
            modelBuilder.Entity<Biaya>()
                .HasOne(b => b.Kandang)
                .WithMany()
                .HasForeignKey(b => b.KandangId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);

            // Relasi Operasional -> Biaya (One-to-Many, optional)
            modelBuilder.Entity<Biaya>()
                .HasOne(b => b.Operasional)
                .WithMany()
                .HasForeignKey(b => b.OperasionalId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);

            // Relasi JenisKegiatan -> Operasional (One-to-Many)
            modelBuilder.Entity<Operasional>()
                .HasOne(o => o.JenisKegiatan)
                .WithMany()
                .HasForeignKey(o => o.JenisKegiatanId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relasi User -> Operasional (One-to-Many)
            modelBuilder.Entity<Operasional>()
                .HasOne(o => o.Petugas)
                .WithMany()
                .HasForeignKey(o => o.PetugasId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relasi Kandang -> Operasional (One-to-Many)
            modelBuilder.Entity<Operasional>()
                .HasOne(o => o.Kandang)
                .WithMany()
                .HasForeignKey(o => o.KandangId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relasi Pakan -> Operasional (One-to-Many, optional)
            modelBuilder.Entity<Operasional>()
                .HasOne(o => o.Pakan)
                .WithMany()
                .HasForeignKey(o => o.PakanId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);

            // Relasi Vaksin -> Operasional (One-to-Many, optional)
            modelBuilder.Entity<Operasional>()
                .HasOne(o => o.Vaksin)
                .WithMany()
                .HasForeignKey(o => o.VaksinId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);

            // Konfigurasi properti string yang required
            modelBuilder.Entity<JenisKegiatan>()
                .Property(j => j.NamaKegiatan)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<Pakan>()
                .Property(p => p.NamaPakan)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<Vaksin>()
                .Property(v => v.NamaVaksin)
                .IsRequired()
                .HasMaxLength(100);

            // Konfigurasi decimal precision untuk Biaya
            modelBuilder.Entity<Biaya>()
                .Property(b => b.Jumlah)
                .HasPrecision(18, 2);

            // Konfigurasi decimal precision untuk Panen
            modelBuilder.Entity<Panen>()
                .Property(p => p.BeratRataRata)
                .HasPrecision(18, 2);

            // Konfigurasi decimal precision untuk JenisKegiatan
            modelBuilder.Entity<JenisKegiatan>()
                .Property(j => j.BiayaDefault)
                .HasPrecision(18, 2);

            // Konfigurasi decimal precision untuk StokKg pada Pakan
            modelBuilder.Entity<Pakan>()
                .Property(p => p.StokKg)
                .HasPrecision(18, 2);

            // Relasi User -> Notification (One-to-Many)
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Notification>()
                .Property(n => n.Title)
                .IsRequired()
                .HasMaxLength(200);

            modelBuilder.Entity<Notification>()
                .Property(n => n.Message)
                .IsRequired()
                .HasMaxLength(1000);

            modelBuilder.Entity<Notification>()
                .Property(n => n.Type)
                .IsRequired()
                .HasMaxLength(50);

            // Relasi KandangAsisten (Many-to-Many Junction)
            modelBuilder.Entity<KandangAsisten>()
                .HasOne(ka => ka.Kandang)
                .WithMany(k => k.KandangAsistens)
                .HasForeignKey(ka => ka.KandangId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<KandangAsisten>()
                .HasOne(ka => ka.Asisten)
                .WithMany()
                .HasForeignKey(ka => ka.AsistenId)
                .OnDelete(DeleteBehavior.Restrict);

            // Unique constraint: satu asisten tidak boleh didaftarkan 2x pada kandang yang sama
            modelBuilder.Entity<KandangAsisten>()
                .HasIndex(ka => new { ka.KandangId, ka.AsistenId })
                .IsUnique();

            // Relasi JurnalHarian -> User (Petugas) (Many-to-One)
            modelBuilder.Entity<JurnalHarian>()
                .HasOne(j => j.Petugas)
                .WithMany()
                .HasForeignKey(j => j.PetugasId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relasi JurnalHarian -> Kandang (Many-to-One, optional)
            modelBuilder.Entity<JurnalHarian>()
                .HasOne(j => j.Kandang)
                .WithMany()
                .HasForeignKey(j => j.KandangId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);

            // Konfigurasi properti string yang required untuk JurnalHarian
            modelBuilder.Entity<JurnalHarian>()
                .Property(j => j.JudulKegiatan)
                .IsRequired()
                .HasMaxLength(200);

            modelBuilder.Entity<JurnalHarian>()
                .Property(j => j.DeskripsiKegiatan)
                .IsRequired()
                .HasMaxLength(1000);

            modelBuilder.Entity<JurnalHarian>()
                .Property(j => j.Catatan)
                .HasMaxLength(500);

            modelBuilder.Entity<JurnalHarian>()
                .Property(j => j.FotoKegiatan)
                .HasMaxLength(500);

            // ------------------------------------------------------------
            // Seed data for demo (petugas, kandang, jenis kegiatan, operasional & biaya)
            // ------------------------------------------------------------
            /*
            var seedPetugasId = Guid.Parse("11111111-2222-3333-4444-555555555555");
            var seedKandangId = Guid.Parse("66666666-7777-8888-9999-000000000000");
            var seedJenisDesinfektanId = Guid.Parse("294429b6-3320-4d58-8847-99dd1a205e49");
            var seedJenisSekamId = Guid.Parse("85b39a24-917e-406c-ba97-c45c3ed63f65");

            var seedOperasionalDesinfektanId = Guid.Parse("aaaaaaaa-1111-2222-3333-bbbbbbbbbbbb");
            var seedOperasionalSekamId = Guid.Parse("aaaaaaaa-1111-2222-3333-cccccccccccc");

            var seedBiayaDesinfektanId = Guid.Parse("dddddddd-1111-2222-3333-eeeeeeeeeeee");
            var seedBiayaSekamId = Guid.Parse("dddddddd-1111-2222-3333-fffffffffffe");

            // User (Petugas)
            modelBuilder.Entity<User>().HasData(new User
            {
                Id = seedPetugasId,
                Username = "petugas1",
                Password = "hashed_password",
                FullName = "Petugas Demo",
                Role = Enums.RoleEnum.Petugas,
                Email = "petugas1@example.com",
                NoWA = "081234567890",
                CreatedAt = DateTime.SpecifyKind(new DateTime(2025, 12, 1), DateTimeKind.Utc),
                UpdateAt = DateTime.SpecifyKind(new DateTime(2025, 12, 1), DateTimeKind.Utc)
            });

            // Kandang
            modelBuilder.Entity<Kandang>().HasData(new Kandang
            {
                Id = seedKandangId,
                NamaKandang = "Kandang A",
                Kapasitas = 2000,
                Lokasi = "Blok A",
                petugasId = seedPetugasId,
                CreatedAt = DateTime.SpecifyKind(new DateTime(2025, 12, 1), DateTimeKind.Utc),
                UpdateAt = DateTime.SpecifyKind(new DateTime(2025, 12, 1), DateTimeKind.Utc)
            });

            // Jenis Kegiatan
            modelBuilder.Entity<JenisKegiatan>().HasData(
                new JenisKegiatan
                {
                    Id = seedJenisDesinfektanId,
                    NamaKegiatan = "Pembelian desinfektan",
                    Deskripsi = "10 liter x Rp 100.000 = Rp 1.000.000",
                    BiayaDefault = 100000m,
                    CreatedAt = DateTime.SpecifyKind(new DateTime(2025, 12, 6), DateTimeKind.Utc),
                    UpdateAt = DateTime.SpecifyKind(new DateTime(2025, 12, 6, 12, 59, 36), DateTimeKind.Utc)
                },
                new JenisKegiatan
                {
                    Id = seedJenisSekamId,
                    NamaKegiatan = "Pembelian sekam padi",
                    Deskripsi = "30 karung x Rp 50.000 = Rp 1.500.000",
                    BiayaDefault = 50000m,
                    CreatedAt = DateTime.SpecifyKind(new DateTime(2025, 12, 6), DateTimeKind.Utc),
                    UpdateAt = DateTime.SpecifyKind(new DateTime(2025, 12, 6, 12, 59, 50), DateTimeKind.Utc)
                }
            );

            // Operasional (tanpa pakan/vaksin, hanya pengeluaran umum)
            modelBuilder.Entity<Operasional>().HasData(
                new Operasional
                {
                    Id = seedOperasionalDesinfektanId,
                    JenisKegiatanId = seedJenisDesinfektanId,
                    Tanggal = DateTime.SpecifyKind(new DateTime(2025, 12, 9, 0, 0, 0), DateTimeKind.Utc),
                    Jumlah = 10, // 10 liter
                    PetugasId = seedPetugasId,
                    KandangId = seedKandangId,
                    PakanId = null,
                    VaksinId = null,
                    CreatedAt = DateTime.SpecifyKind(new DateTime(2025, 12, 9), DateTimeKind.Utc),
                    UpdateAt = DateTime.SpecifyKind(new DateTime(2025, 12, 9), DateTimeKind.Utc)
                },
                new Operasional
                {
                    Id = seedOperasionalSekamId,
                    JenisKegiatanId = seedJenisSekamId,
                    Tanggal = DateTime.SpecifyKind(new DateTime(2025, 12, 9, 0, 0, 0), DateTimeKind.Utc),
                    Jumlah = 30, // 30 karung
                    PetugasId = seedPetugasId,
                    KandangId = seedKandangId,
                    PakanId = null,
                    VaksinId = null,
                    CreatedAt = DateTime.SpecifyKind(new DateTime(2025, 12, 9), DateTimeKind.Utc),
                    UpdateAt = DateTime.SpecifyKind(new DateTime(2025, 12, 9), DateTimeKind.Utc)
                }
            );

            // Biaya terhubung ke Operasional
            modelBuilder.Entity<Biaya>().HasData(
                new Biaya
                {
                    Id = seedBiayaDesinfektanId,
                    JenisBiaya = "Pembelian desinfektan",
                    KategoriBiaya = Enums.KategoriBiayaEnum.Pembelian,
                    Tanggal = DateTime.SpecifyKind(new DateTime(2025, 12, 9, 0, 0, 0), DateTimeKind.Utc),
                    Jumlah = 1000000m,
                    PetugasId = seedPetugasId,
                    OperasionalId = seedOperasionalDesinfektanId,
                    KandangId = seedKandangId,
                    BuktiBase64 = null,
                    Catatan = "10 liter x Rp 100.000 = Rp 1.000.000",
                    Bulan = 12,
                    Tahun = 2025,
                    CreatedAt = DateTime.SpecifyKind(new DateTime(2025, 12, 9), DateTimeKind.Utc),
                    UpdateAt = DateTime.SpecifyKind(new DateTime(2025, 12, 9), DateTimeKind.Utc)
                },
                new Biaya
                {
                    Id = seedBiayaSekamId,
                    JenisBiaya = "Pembelian sekam padi",
                    KategoriBiaya = Enums.KategoriBiayaEnum.Pembelian,
                    Tanggal = DateTime.SpecifyKind(new DateTime(2025, 12, 9, 0, 0, 0), DateTimeKind.Utc),
                    Jumlah = 1500000m,
                    PetugasId = seedPetugasId,
                    OperasionalId = seedOperasionalSekamId,
                    KandangId = seedKandangId,
                    BuktiBase64 = null,
                    Catatan = "30 karung x Rp 50.000 = Rp 1.500.000",
                    Bulan = 12,
                    Tahun = 2025,
                    CreatedAt = DateTime.SpecifyKind(new DateTime(2025, 12, 9), DateTimeKind.Utc),
                    UpdateAt = DateTime.SpecifyKind(new DateTime(2025, 12, 9), DateTimeKind.Utc)
                }
            );
            */
        }


    }
}

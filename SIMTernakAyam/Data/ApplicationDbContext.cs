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
        }


    }
}

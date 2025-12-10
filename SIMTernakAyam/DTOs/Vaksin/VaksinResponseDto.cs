using SIMTernakAyam.Enums;

namespace SIMTernakAyam.DTOs.Vaksin
{
    public class VaksinResponseDto
    {
        public Guid Id { get; set; }
        public string NamaVaksin { get; set; } = string.Empty;
        public int Stok { get; set; }
        public int StokAwal { get; set; } // Stok awal sebelum penggunaan
        public int StokTerpakai { get; set; } // Total stok yang sudah digunakan
        public int StokTersisa { get; set; } // Sisa stok yang bisa digunakan
        public bool IsStokCukup { get; set; } // Indikator apakah stok masih cukup
        public string StatusStok { get; set; } = string.Empty; // "Aman", "Menipis", "Habis"
        public int Bulan { get; set; } // 1-12 (Januari-Desember)
        public int Tahun { get; set; } // Contoh: 2024, 2025
        public VaksinVitaminTypeEnum Tipe { get; set; } // Vaksin atau Vitamin
        public string TipeNama => Tipe.ToString(); // String representation untuk frontend
        public DateTime CreatedAt { get; set; }
        public DateTime UpdateAt { get; set; }

        public static VaksinResponseDto FromEntity(Models.Vaksin vaksin)
        {
            var stokTersisa = vaksin.Stok;
            var statusStok = GetStatusStok(stokTersisa);
            
            return new VaksinResponseDto
            {
                Id = vaksin.Id,
                NamaVaksin = vaksin.NamaVaksin,
                Stok = vaksin.Stok,
                StokAwal = vaksin.Stok, // Untuk sementara sama dengan stok saat ini
                StokTerpakai = 0, // Akan dihitung dari operasional
                StokTersisa = stokTersisa,
                IsStokCukup = stokTersisa > 0,
                StatusStok = statusStok,
                Bulan = vaksin.Bulan,
                Tahun = vaksin.Tahun,
                Tipe = vaksin.Tipe,
                CreatedAt = vaksin.CreatedAt,
                UpdateAt = vaksin.UpdateAt
            };
        }

        public static VaksinResponseDto FromEntityWithUsage(Models.Vaksin vaksin, int stokTerpakai = 0)
        {
            var stokTersisa = vaksin.Stok;
            var statusStok = GetStatusStok(stokTersisa);
            
            return new VaksinResponseDto
            {
                Id = vaksin.Id,
                NamaVaksin = vaksin.NamaVaksin,
                Stok = vaksin.Stok,
                StokAwal = vaksin.Stok + stokTerpakai, // Stok awal = stok saat ini + yang sudah dipakai
                StokTerpakai = stokTerpakai,
                StokTersisa = stokTersisa,
                IsStokCukup = stokTersisa > 0,
                StatusStok = statusStok,
                Bulan = vaksin.Bulan,
                Tahun = vaksin.Tahun,
                Tipe = vaksin.Tipe,
                CreatedAt = vaksin.CreatedAt,
                UpdateAt = vaksin.UpdateAt
            };
        }

        private static string GetStatusStok(int stokTersisa)
        {
            return stokTersisa switch
            {
                0 => "Habis",
                <= 2 => "Kritis",
                <= 5 => "Menipis",
                _ => "Aman"
            };
        }

        public static List<VaksinResponseDto> FromEntities(IEnumerable<Models.Vaksin> vaksins)
        {
            return vaksins.Select(FromEntity).ToList();
        }
    }
}
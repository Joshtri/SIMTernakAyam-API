namespace SIMTernakAyam.DTOs.Pakan
{
    public class PakanResponseDto
    {
        public Guid Id { get; set; }
        public string NamaPakan { get; set; } = string.Empty;
        public decimal StokKg { get; set; }
        public decimal StokAwalKg { get; set; } // Stok awal sebelum penggunaan
        public decimal StokTerpakaiKg { get; set; } // Total stok yang sudah digunakan dalam kg
        public decimal StokTersisaKg { get; set; } // Sisa stok yang bisa digunakan dalam kg
        public bool IsStokCukup { get; set; } // Indikator apakah stok masih cukup
        public string StatusStok { get; set; } = string.Empty; // "Aman", "Menipis", "Habis"
        public int Bulan { get; set; }
        public int Tahun { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdateAt { get; set; }

        public static PakanResponseDto FromEntity(Models.Pakan pakan)
        {
            var stokTersisa = pakan.StokKg;
            var statusStok = GetStatusStok(stokTersisa);
            
            return new PakanResponseDto
            {
                Id = pakan.Id,
                NamaPakan = pakan.NamaPakan,
                StokKg = pakan.StokKg,
                StokAwalKg = pakan.StokKg, // Untuk sementara sama dengan stok saat ini
                StokTerpakaiKg = 0, // Akan dihitung dari operasional
                StokTersisaKg = stokTersisa,
                IsStokCukup = stokTersisa > 0,
                StatusStok = statusStok,
                Bulan = pakan.Bulan,
                Tahun = pakan.Tahun,
                CreatedAt = pakan.CreatedAt,
                UpdateAt = pakan.UpdateAt
            };
        }

        public static PakanResponseDto FromEntityWithUsage(Models.Pakan pakan, decimal stokTerpakai = 0)
        {
            var stokTersisa = pakan.StokKg;
            var statusStok = GetStatusStok(stokTersisa);
            
            return new PakanResponseDto
            {
                Id = pakan.Id,
                NamaPakan = pakan.NamaPakan,
                StokKg = pakan.StokKg,
                StokAwalKg = pakan.StokKg + stokTerpakai, // Stok awal = stok saat ini + yang sudah dipakai
                StokTerpakaiKg = stokTerpakai,
                StokTersisaKg = stokTersisa,
                IsStokCukup = stokTersisa > 0,
                StatusStok = statusStok,
                Bulan = pakan.Bulan,
                Tahun = pakan.Tahun,
                CreatedAt = pakan.CreatedAt,
                UpdateAt = pakan.UpdateAt
            };
        }

        private static string GetStatusStok(decimal stokTersisa)
        {
            return stokTersisa switch
            {
                0 => "Habis",
                <= 10 => "Kritis",
                <= 50 => "Menipis",
                _ => "Aman"
            };
        }

        public static List<PakanResponseDto> FromEntities(IEnumerable<Models.Pakan> pakans)
        {
            return pakans.Select(FromEntity).ToList();
        }
    }
}
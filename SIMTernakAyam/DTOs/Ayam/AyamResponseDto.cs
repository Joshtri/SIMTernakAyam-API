namespace SIMTernakAyam.DTOs.Ayam
{
    public class AyamResponseDto
    {
        public Guid Id { get; set; }
        public Guid KandangId { get; set; }
        public string? KandangNama { get; set; }
        public string? PetugasKandangNama { get; set; }
        public DateTime TanggalMasuk { get; set; }
        public int JumlahMasuk { get; set; }
        
        // ✅ NEW: Stock Information
        public int JumlahSudahDipanen { get; set; }
        public int JumlahMortalitas { get; set; }
        public int SisaAyamHidup { get; set; }
        public decimal PersentaseSurvival { get; set; } // Persentase ayam yang masih hidup
        public decimal PersentaseDipanen { get; set; } // Persentase yang sudah dipanen
        public decimal PersentaseMortalitas { get; set; } // Persentase kematian
        
        // ✅ NEW: Status Information
        public bool BisaDipanen { get; set; } // Apakah masih bisa dipanen
        public bool PerluPerhatianKesehatan { get; set; } // Mortalitas > 10%
        
        public DateTime CreatedAt { get; set; }
        public DateTime UpdateAt { get; set; }

        public static AyamResponseDto FromEntity(Models.Ayam ayam, int jumlahDipanen = 0, int jumlahMortalitas = 0)
        {
            var sisaHidup = Math.Max(0, ayam.JumlahMasuk - jumlahDipanen - jumlahMortalitas);
            var persentaseSurvival = ayam.JumlahMasuk > 0 ? Math.Round((decimal)sisaHidup / ayam.JumlahMasuk * 100, 2) : 0;
            var persentaseDipanen = ayam.JumlahMasuk > 0 ? Math.Round((decimal)jumlahDipanen / ayam.JumlahMasuk * 100, 2) : 0;
            var persentaseMortalitas = ayam.JumlahMasuk > 0 ? Math.Round((decimal)jumlahMortalitas / ayam.JumlahMasuk * 100, 2) : 0;

            return new AyamResponseDto
            {
                Id = ayam.Id,
                KandangId = ayam.KandangId,
                KandangNama = ayam.Kandang?.NamaKandang,
                PetugasKandangNama = ayam.Kandang?.User?.FullName ?? ayam.Kandang?.User?.Username,
                TanggalMasuk = ayam.TanggalMasuk,
                JumlahMasuk = ayam.JumlahMasuk,
                
                // Stock Information
                JumlahSudahDipanen = jumlahDipanen,
                JumlahMortalitas = jumlahMortalitas,
                SisaAyamHidup = sisaHidup,
                PersentaseSurvival = persentaseSurvival,
                PersentaseDipanen = persentaseDipanen,
                PersentaseMortalitas = persentaseMortalitas,
                
                // Status Information
                BisaDipanen = sisaHidup > 0,
                PerluPerhatianKesehatan = persentaseMortalitas > 10, // Alert jika mortalitas > 10%
                
                CreatedAt = ayam.CreatedAt,
                UpdateAt = ayam.UpdateAt
            };
        }

        public static List<AyamResponseDto> FromEntities(IEnumerable<Models.Ayam> ayams)
        {
            return ayams.Select(ayam => FromEntity(ayam)).ToList();
        }

        // ✅ NEW: Method with stock data
        public static List<AyamResponseDto> FromEntitiesWithStockData(
            IEnumerable<Models.Ayam> ayams, 
            Dictionary<Guid, int> panenData, 
            Dictionary<Guid, int> mortalitasData)
        {
            return ayams.Select(ayam => FromEntity(
                ayam, 
                panenData.GetValueOrDefault(ayam.Id, 0),
                mortalitasData.GetValueOrDefault(ayam.Id, 0)
            )).ToList();
        }
    }
}

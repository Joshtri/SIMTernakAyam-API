namespace SIMTernakAyam.DTOs.Mortalitas
{
    public class MortalitasResponseDto
    {
        public Guid Id { get; set; }
        public Guid AyamId { get; set; }
        
        // Informasi Kandang dan Petugas
        public Guid KandangId { get; set; }
        public string KandangNama { get; set; } = string.Empty;
        public string PetugasId { get; set; } = string.Empty;
        public string PetugasNama { get; set; } = string.Empty;
        
        // Detail Mortalitas
        public DateTime TanggalKematian { get; set; }
        public int JumlahKematian { get; set; }
        public string PenyebabKematian { get; set; } = string.Empty;
        public string? FotoMortalitas { get; set; }  // Path foto bukti mortalitas
        public string? FotoMortalitasBase64 { get; set; }  // Base64 string untuk display

        // Perhitungan dan Statistik
        public int JumlahAyamSebelumMati { get; set; }
        public int JumlahAyamSesudahMati { get; set; }
        public decimal PersentaseMortalitas { get; set; }
        public decimal KapasitasKandang { get; set; }
        public decimal PersentaseUtilisasiSebelum { get; set; }
        public decimal PersentaseUtilisasiSesudah { get; set; }
        
        // Impact Analysis
        public string StatusDampak { get; set; } = string.Empty; // Low, Medium, High, Critical
        public string Rekomendasi { get; set; } = string.Empty;
        
        // Timestamps
        public DateTime CreatedAt { get; set; }
        public DateTime UpdateAt { get; set; }

        public static MortalitasResponseDto FromEntity(Models.Mortalitas mortalitas, int? totalAyamSebelum = null, decimal? kapasitas = null)
        {
            var totalSebelum = totalAyamSebelum ?? 0;
            var totalSesudah = Math.Max(0, totalSebelum - mortalitas.JumlahKematian);
            var persentaseMortalitas = totalSebelum > 0 ? (decimal)mortalitas.JumlahKematian / totalSebelum * 100 : 0;
            var kandangKapasitas = kapasitas ?? 1; // Avoid division by zero
            
            var utilisasiSebelum = kandangKapasitas > 0 ? (decimal)totalSebelum / kandangKapasitas * 100 : 0;
            var utilisasiSesudah = kandangKapasitas > 0 ? (decimal)totalSesudah / kandangKapasitas * 100 : 0;

            // Determine impact status and recommendation
            var (statusDampak, rekomendasi) = GetImpactAnalysis(persentaseMortalitas, mortalitas.PenyebabKematian);

            return new MortalitasResponseDto
            {
                Id = mortalitas.Id,
                AyamId = mortalitas.AyamId,
                KandangId = mortalitas.Ayam?.KandangId ?? Guid.Empty,
                KandangNama = mortalitas.Ayam?.Kandang?.NamaKandang ?? "Unknown",
                PetugasId = mortalitas.Ayam?.Kandang?.petugasId.ToString() ?? string.Empty,
                PetugasNama = mortalitas.Ayam?.Kandang?.User?.FullName ?? 
                             mortalitas.Ayam?.Kandang?.User?.Username ?? "Unknown",
                
                TanggalKematian = mortalitas.TanggalKematian,
                JumlahKematian = mortalitas.JumlahKematian,
                PenyebabKematian = mortalitas.PenyebabKematian,
                FotoMortalitas = mortalitas.FotoMortalitas,

                JumlahAyamSebelumMati = totalSebelum,
                JumlahAyamSesudahMati = totalSesudah,
                PersentaseMortalitas = Math.Round(persentaseMortalitas, 2),
                KapasitasKandang = kandangKapasitas,
                PersentaseUtilisasiSebelum = Math.Round(utilisasiSebelum, 2),
                PersentaseUtilisasiSesudah = Math.Round(utilisasiSesudah, 2),
                
                StatusDampak = statusDampak,
                Rekomendasi = rekomendasi,
                
                CreatedAt = mortalitas.CreatedAt,
                UpdateAt = mortalitas.UpdateAt
            };
        }

        private static (string status, string rekomendasi) GetImpactAnalysis(decimal persentaseMortalitas, string penyebab)
        {
            return persentaseMortalitas switch
            {
                >= 10 => ("Critical", $"Tingkat mortalitas sangat tinggi ({persentaseMortalitas}%). Segera lakukan investigasi mendalam dan tindakan darurat untuk penyebab: {penyebab}"),
                >= 5 => ("High", $"Tingkat mortalitas tinggi ({persentaseMortalitas}%). Perlu monitoring ketat dan review kondisi kandang terkait: {penyebab}"),
                >= 2 => ("Medium", $"Tingkat mortalitas sedang ({persentaseMortalitas}%). Tingkatkan pencegahan dan monitoring rutin untuk: {penyebab}"),
                _ => ("Low", $"Tingkat mortalitas rendah ({persentaseMortalitas}%). Pertahankan kondisi kandang dan lanjutkan monitoring rutin")
            };
        }

        public static List<MortalitasResponseDto> FromEntities(IEnumerable<Models.Mortalitas> mortalitasList)
        {
            return mortalitasList.Select(m => FromEntity(m)).ToList();
        }
    }
}

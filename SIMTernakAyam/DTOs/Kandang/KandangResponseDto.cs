namespace SIMTernakAyam.DTOs.Kandang
{
    public class KandangResponseDto
    {
        public Guid Id { get; set; }
        public string NamaKandang { get; set; } = string.Empty;
        public int Kapasitas { get; set; }
        public string Lokasi { get; set; } = string.Empty;
        public Guid PetugasId { get; set; }
        public string? PetugasNama { get; set; }
        
        // ? NEW: Informasi jumlah ayam di kandang
        public int JumlahAyamTerisi { get; set; } // Total ayam hidup saat ini
        public int KapasitasTersedia { get; set; } // Kapasitas yang masih tersedia
        public decimal PersentaseTerisi { get; set; } // Persentase pengisian kandang
        public bool IsKandangPenuh { get; set; } // Apakah kandang sudah penuh
        public string StatusKapasitas { get; set; } = string.Empty; // "Kosong", "Tersedia", "Hampir Penuh", "Penuh"
        
        public DateTime CreatedAt { get; set; }
        public DateTime UpdateAt { get; set; }

        public static KandangResponseDto FromEntity(Models.Kandang kandang, int jumlahAyamHidup = 0)
        {
            var kapasitasTersedia = Math.Max(0, kandang.Kapasitas - jumlahAyamHidup);
            var persentaseTerisi = kandang.Kapasitas > 0 
                ? Math.Round((decimal)jumlahAyamHidup / kandang.Kapasitas * 100, 2) 
                : 0;
            
            var isKandangPenuh = jumlahAyamHidup >= kandang.Kapasitas;
            
            // Tentukan status kapasitas
            string statusKapasitas;
            if (jumlahAyamHidup == 0)
                statusKapasitas = "Kosong";
            else if (persentaseTerisi >= 100)
                statusKapasitas = "Penuh";
            else if (persentaseTerisi >= 80)
                statusKapasitas = "Hampir Penuh";
            else
                statusKapasitas = "Tersedia";

            return new KandangResponseDto
            {
                Id = kandang.Id,
                NamaKandang = kandang.NamaKandang,
                Kapasitas = kandang.Kapasitas,
                Lokasi = kandang.Lokasi,
                PetugasId = kandang.petugasId,
                PetugasNama = kandang.User?.FullName ?? kandang.User?.Username,
                
                // Informasi jumlah ayam
                JumlahAyamTerisi = jumlahAyamHidup,
                KapasitasTersedia = kapasitasTersedia,
                PersentaseTerisi = persentaseTerisi,
                IsKandangPenuh = isKandangPenuh,
                StatusKapasitas = statusKapasitas,
                
                CreatedAt = kandang.CreatedAt,
                UpdateAt = kandang.UpdateAt
            };
        }

        public static List<KandangResponseDto> FromEntities(IEnumerable<Models.Kandang> kandangs)
        {
            return kandangs.Select(k => FromEntity(k)).ToList();
        }

        // ? NEW: Method dengan data jumlah ayam hidup
        public static List<KandangResponseDto> FromEntitiesWithStockData(
            IEnumerable<Models.Kandang> kandangs, 
            Dictionary<Guid, int> ayamHidupData)
        {
            return kandangs.Select(kandang => FromEntity(
                kandang, 
                ayamHidupData.GetValueOrDefault(kandang.Id, 0)
            )).ToList();
        }
    }
}

using SIMTernakAyam.Models;

namespace SIMTernakAyam.DTOs.HargaPasar
{
    /// <summary>
    /// DTO response untuk harga pasar
    /// </summary>
    public class HargaPasarResponseDto
    {
        public Guid Id { get; set; }
        public decimal HargaPerEkor { get; set; }
        public decimal HargaPerKg { get; set; }
        public DateTime TanggalMulai { get; set; }
        public DateTime? TanggalBerakhir { get; set; }
        public string? Keterangan { get; set; }
        public bool IsAktif { get; set; }
        public string? Wilayah { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        /// <summary>
        /// Status harga dalam format yang mudah dibaca
        /// </summary>
        public string StatusText => IsAktif ? "Aktif" : "Tidak Aktif";
        
        /// <summary>
        /// Durasi berlaku harga dalam hari
        /// </summary>
        public int? DurasiBerlaku => TanggalBerakhir?.Subtract(TanggalMulai).Days;
        
        /// <summary>
        /// Format harga per ekor dalam rupiah
        /// </summary>
        public string HargaPerEkorFormatted => $"Rp {HargaPerEkor:N0}";
        
        /// <summary>
        /// Format harga per kg dalam rupiah
        /// </summary>
        public string HargaPerKgFormatted => $"Rp {HargaPerKg:N0}";

        /// <summary>
        /// Konversi dari entity model ke response DTO
        /// </summary>
        /// <param name="hargaPasar">Entity harga pasar</param>
        /// <returns>Response DTO</returns>
        public static HargaPasarResponseDto FromEntity(Models.HargaPasar hargaPasar)
        {
            return new HargaPasarResponseDto
            {
                Id = hargaPasar.Id,
                HargaPerEkor = hargaPasar.HargaPerEkor,
                HargaPerKg = hargaPasar.HargaPerKg,
                TanggalMulai = hargaPasar.TanggalMulai,
                TanggalBerakhir = hargaPasar.TanggalBerakhir,
                Keterangan = hargaPasar.Keterangan,
                IsAktif = hargaPasar.IsAktif,
                Wilayah = hargaPasar.Wilayah,
                CreatedAt = hargaPasar.CreatedAt,
                UpdatedAt = hargaPasar.UpdateAt
            };
        }

        /// <summary>
        /// Konversi dari list entity ke list response DTO
        /// </summary>
        /// <param name="hargaPasarList">List entity harga pasar</param>
        /// <returns>List response DTO</returns>
        public static List<HargaPasarResponseDto> FromEntities(IEnumerable<Models.HargaPasar> hargaPasarList)
        {
            return hargaPasarList.Select(FromEntity).ToList();
        }
    }
}
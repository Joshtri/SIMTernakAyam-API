using System;

namespace SIMTernakAyam.DTOs.Laporan
{
    public class LaporanBatchDto
    {
        public Guid BatchId { get; set; }
        public Guid KandangId { get; set; }
        public string NamaKandang { get; set; } = string.Empty;
        public string Periode { get; set; } = string.Empty; // e.g. "10 Nov 2025 - 20 Dec 2025"
        public DateTime TanggalMulai { get; set; }
        public DateTime? TanggalSelesai { get; set; }
        public string Status { get; set; } = string.Empty; // "Aktif" or "Selesai"

        public int PopulasiAwal { get; set; }
        public int PopulasiSaatIni { get; set; }
        public int TotalKematian { get; set; }
        public int TotalPanen { get; set; } // Ekor

        // Financials
        public decimal TotalPendapatan { get; set; }
        public decimal TotalBiaya { get; set; }
        public decimal Keuntungan { get; set; }
        
        // Breakdown
        public decimal BiayaPakan { get; set; }
        public decimal BiayaVaksin { get; set; }
        public decimal BiayaOperasionalLain { get; set; } 
        
        // FCR & Performance
        public double FCR { get; set; }
        public double MortalityRate { get; set; }
        public decimal AverageWeight { get; set; }
    }

    public class BatchOptionDto 
    {
        public Guid Id { get; set; } // AyamId
        public string Name { get; set; } = string.Empty; // "Batch {Date} - {Kandang}"
    }
}

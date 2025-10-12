﻿namespace SIMTernakAyam.Models
{
    public class Mortalitas : BaseModel
    {
        public Guid AyamId { get; set; }       // Relasi ke entri ayam masuk
        public Ayam Ayam { get; set; }

        public DateTime TanggalKematian { get; set; }
        public int JumlahKematian { get; set; }
        public string PenyebabKematian { get; set; }
    }
}

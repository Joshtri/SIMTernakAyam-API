namespace SIMTernakAyam.Enums
{
    /// <summary>
    /// Status proses relokasi ayam
    /// </summary>
    public enum StatusRelokasiEnum
    {
        /// <summary>
        /// Relokasi direncanakan tapi belum dilaksanakan
        /// </summary>
        Pending = 0,

        /// <summary>
        /// Relokasi sudah selesai dilaksanakan
        /// </summary>
        Selesai = 1,

        /// <summary>
        /// Relokasi dibatalkan
        /// </summary>
        Dibatalkan = 2
    }
}

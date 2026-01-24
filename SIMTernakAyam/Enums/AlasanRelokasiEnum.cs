namespace SIMTernakAyam.Enums
{
    /// <summary>
    /// Alasan/tujuan relokasi ayam
    /// </summary>
    public enum AlasanRelokasiEnum
    {
        /// <summary>
        /// Ayam sakit dipindahkan ke kandang isolasi
        /// </summary>
        Sakit = 0,

        /// <summary>
        /// Karantina preventif untuk mencegah penyebaran
        /// </summary>
        Karantina = 1,

        /// <summary>
        /// Ayam sudah pulih, dipindahkan kembali ke kandang normal
        /// </summary>
        Pulih = 2,

        /// <summary>
        /// Alasan lainnya
        /// </summary>
        Lainnya = 3
    }
}

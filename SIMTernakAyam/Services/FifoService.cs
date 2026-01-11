using SIMTernakAyam.Repository.Interfaces;
using SIMTernakAyam.Services.Interfaces;

namespace SIMTernakAyam.Services
{
    /// <summary>
    /// Service untuk handle Auto FIFO logic
    /// UPDATED: Mengambil ayam TERBARU dulu (LIFO - Last In First Out), bukan terlama
    /// Karena dalam realita, ayam terbaru yang lebih sering dipanen/mati
    /// </summary>
    public class FifoService : IFifoService
    {
        private readonly IAyamRepository _ayamRepository;
        private readonly IPanenRepository _panenRepository;
        private readonly IMortalitasRepository _mortalitasRepository;

        public FifoService(
            IAyamRepository ayamRepository,
            IPanenRepository panenRepository,
            IMortalitasRepository mortalitasRepository)
        {
            _ayamRepository = ayamRepository;
            _panenRepository = panenRepository;
            _mortalitasRepository = mortalitasRepository;
        }

        public async Task<List<(Guid AyamId, int Jumlah)>> DistributeAsync(Guid kandangId, int totalJumlah)
        {
            // Get all ayam di kandang
            // ? CHANGED: OrderByDescending = ambil ayam TERBARU dulu (LIFO)
            var ayamList = (await _ayamRepository.GetByKandangIdAsync(kandangId))
                .OrderByDescending(a => a.TanggalMasuk)  // ? Ayam terbaru dulu
                .ThenBy(a => a.IsAyamSisa ? 1 : 0)       // Prioritas: ayam baru (0) dulu, ayam sisa (1) belakangan
                .ToList();

            if (!ayamList.Any())
            {
                throw new Exception("Tidak ada ayam di kandang ini.");
            }

            // Get data panen dan mortalitas untuk calculate ayam hidup
            var ayamIds = ayamList.Select(a => a.Id).ToList();
            var panenData = ayamIds.Any() ? await _panenRepository.GetTotalEkorPanenByAyamIdsAsync(ayamIds) : new Dictionary<Guid, int>();
            var mortalitasData = ayamIds.Any() ? await _mortalitasRepository.GetTotalMortalitasByAyamIdsAsync(ayamIds) : new Dictionary<Guid, int>();

            // Distribute jumlah dengan LIFO (terbaru dulu)
            var result = new List<(Guid AyamId, int Jumlah)>();
            int sisaJumlah = totalJumlah;

            foreach (var ayam in ayamList)
            {
                if (sisaJumlah <= 0) break;

                var dipanen = panenData.ContainsKey(ayam.Id) ? panenData[ayam.Id] : 0;
                var mati = mortalitasData.ContainsKey(ayam.Id) ? mortalitasData[ayam.Id] : 0;
                var ayamHidup = ayam.JumlahMasuk - dipanen - mati;

                if (ayamHidup > 0)
                {
                    var ambil = Math.Min(ayamHidup, sisaJumlah);
                    result.Add((ayam.Id, ambil));
                    sisaJumlah -= ambil;
                }
            }

            if (sisaJumlah > 0)
            {
                throw new Exception($"Jumlah ayam hidup di kandang tidak cukup. Kekurangan: {sisaJumlah} ekor.");
            }

            return result;
        }
    }
}

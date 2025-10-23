using SIMTernakAyam.Models;
using SIMTernakAyam.Repository.Interfaces;
using SIMTernakAyam.Services.Interfaces;

namespace SIMTernakAyam.Services
{
    public class StokService : IStokService
    {
        private readonly IPakanRepository _pakanRepository;
        private readonly IVaksinRepository _vaksinRepository;

        public StokService(
            IPakanRepository pakanRepository,
            IVaksinRepository vaksinRepository)
        {
            _pakanRepository = pakanRepository;
            _vaksinRepository = vaksinRepository;
        }

        public async Task<(bool Success, string Message)> KurangiStokPakan(Guid pakanId, DateTime tanggal, decimal jumlah)
        {
            if (jumlah <= 0)
            {
                return (false, "Jumlah harus lebih besar dari 0.");
            }

            // Use a direct database operation to avoid tracking conflicts
            var result = await _pakanRepository.UpdateStokKgAsyncDirect(pakanId, -jumlah, tanggal);
            
            if (!result.Success)
            {
                return (result.Success, result.Message);
            }

            return (true, $"Stok pakan berhasil dikurangi sebesar {jumlah} kg. {result.Message}");
        }

        public async Task<(bool Success, string Message)> KurangiStokVaksin(Guid vaksinId, DateTime tanggal, int jumlah)
        {
            if (jumlah <= 0)
            {
                return (false, "Jumlah harus lebih besar dari 0.");
            }

            if (jumlah < 0)
            {
                return (false, "Jumlah tidak boleh negatif.");
            }

            // Use a direct database operation to avoid tracking conflicts
            var result = await _vaksinRepository.UpdateStokAsyncDirect(vaksinId, -jumlah, tanggal);
            
            if (!result.Success)
            {
                return (result.Success, result.Message);
            }

            return (true, $"Stok vaksin berhasil dikurangi sebesar {jumlah} dosis. {result.Message}");
        }

        public async Task<(bool Success, string Message)> TambahStokPakan(Guid pakanId, DateTime tanggal, decimal jumlah)
        {
            if (jumlah <= 0)
            {
                return (false, "Jumlah harus lebih besar dari 0.");
            }

            // Use a direct database operation to avoid tracking conflicts
            var result = await _pakanRepository.UpdateStokKgAsyncDirect(pakanId, jumlah, tanggal);
            
            if (!result.Success)
            {
                return (result.Success, result.Message);
            }

            return (true, $"Stok pakan berhasil ditambah sebesar {jumlah} kg. {result.Message}");
        }

        public async Task<(bool Success, string Message)> TambahStokVaksin(Guid vaksinId, DateTime tanggal, int jumlah)
        {
            if (jumlah <= 0)
            {
                return (false, "Jumlah harus lebih besar dari 0.");
            }

            // Use a direct database operation to avoid tracking conflicts
            var result = await _vaksinRepository.UpdateStokAsyncDirect(vaksinId, jumlah, tanggal);
            
            if (!result.Success)
            {
                return (result.Success, result.Message);
            }

            return (true, $"Stok vaksin berhasil ditambah sebesar {jumlah} dosis. {result.Message}");
        }
    }
}
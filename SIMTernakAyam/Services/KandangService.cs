using SIMTernakAyam.Models;
using SIMTernakAyam.Repository.Interfaces;
using SIMTernakAyam.Services.Interfaces;

namespace SIMTernakAyam.Services
{
    public class KandangService : BaseService<Kandang>, IkandangService
    {
        private readonly IKandangRepository _kandangRepository;

        public KandangService(IKandangRepository kandangRepository) : base(kandangRepository)
        {
            _kandangRepository = kandangRepository;

        }
    }
}

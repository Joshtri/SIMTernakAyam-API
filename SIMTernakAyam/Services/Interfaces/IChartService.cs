using SIMTernakAyam.DTOs.Chart;

namespace SIMTernakAyam.Services.Interfaces
{
    public interface IChartService
    {
        Task<ProduktivitasTrendDto> GetProduktivitasTrendAsync(string period, DateTime? startDate = null, DateTime? endDate = null);
        Task<MortalitasStatistikDto> GetMortalitasStatistikAsync(Guid? kandangId = null, string period = "monthly");
        Task<OperasionalBreakdownDto> GetOperasionalBreakdownAsync(Guid? petugasId = null, string period = "monthly");
        Task<FinancialAnalysisDto> GetFinancialAnalysisAsync(DateTime startDate, DateTime endDate);
    }
}

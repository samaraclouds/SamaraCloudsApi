using SamaraCloudsApi.Models;

namespace SamaraCloudsApi.Services
{
    public interface IChartOfAccountService
    {
        Task<IEnumerable<ChartOfAccountViewDto>> ViewAllAsync(
            int customerId,
            int? branchId = null,
            string? search = null,
            DateTime? dateFrom = null,
            DateTime? dateTo = null);

        Task<int> CreateAsync(ChartOfAccountCreateDto dto);

        Task UpdateAsync(ChartOfAccountUpdateDto dto);

        Task DeleteAsync(int accountId, int deletedBy);
    }
}

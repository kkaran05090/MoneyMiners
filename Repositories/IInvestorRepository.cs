using MoneyMiners.ViewModels.Admin;

namespace MoneyMiners.Repositories
{
    public interface IInvestorRepository
    {
        Task<InvestorsPageViewModel> GetAllAsync(
            string? status,
            string? search,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default);
    }
}
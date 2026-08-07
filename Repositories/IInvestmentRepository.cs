using MoneyMiners.Models;

namespace MoneyMiners.Repositories
{
    public interface IInvestmentRepository
    {
        Task<CreateInvestmentResult> CreateAsync(
            CreateInvestmentCommand command,
            CancellationToken cancellationToken = default);

        Task<List<InvestorActiveInvestment>>
            GetActiveByInvestorAccountIdAsync(
                long investorAccountId,
                CancellationToken cancellationToken = default);

        Task<List<InvestorInvestmentHistoryItem>>
            GetHistoryByInvestorAccountIdAsync(
                long investorAccountId,
                CancellationToken cancellationToken = default);

        Task ChangeStatusAsync(
            ChangeInvestmentStatusCommand command,
            CancellationToken cancellationToken = default);
    }
}
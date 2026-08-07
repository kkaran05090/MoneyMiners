using MoneyMiners.Models;

namespace MoneyMiners.Repositories
{
    public interface IInvestorAccountRepository
    {
        Task<InvestorRegistrationResult> RegisterAsync(
            InvestorRegistrationCommand command,
            CancellationToken cancellationToken = default);

        Task<InvestorAccount?> GetByLoginAsync(
            string loginIdentifier,
            CancellationToken cancellationToken = default);

        Task<InvestorLookupResult?> GetByInvestorCodeAsync(
            string investorCode,
            CancellationToken cancellationToken = default);

        Task<InvestorLoginAttemptResult> RecordLoginAttemptAsync(
            long investorAccountId,
            bool succeeded,
            CancellationToken cancellationToken = default);

        Task<InvestorPasswordResetResult> ResetPasswordAsync(
           InvestorPasswordResetCommand command,
           CancellationToken cancellationToken = default);
    }
}
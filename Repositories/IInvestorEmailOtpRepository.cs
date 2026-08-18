using MoneyMiners.Models;

namespace MoneyMiners.Repositories
{
    public interface IInvestorEmailOtpRepository
    {
        Task<InvestorEmailOtpChallengeResult> CreateAsync(
            string emailAddress,
            InvestorOtpPurpose purpose,
            byte[] otpHash,
            DateTime expiresAtUtc,
            CancellationToken cancellationToken = default);

        Task<InvestorEmailOtpVerificationResult> VerifyAsync(
            long investorEmailOtpChallengeId,
            string emailAddress,
            InvestorOtpPurpose purpose,
            byte[] otpHash,
            CancellationToken cancellationToken = default);
    }
}
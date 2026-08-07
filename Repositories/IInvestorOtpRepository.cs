using MoneyMiners.Models;

namespace MoneyMiners.Repositories
{
    public interface IInvestorOtpRepository
    {
        Task<InvestorOtpChallengeResult> CreateAsync(
            string phoneNumber,
            InvestorOtpPurpose purpose,
            byte[] otpHash,
            DateTime expiresAtUtc,
            CancellationToken cancellationToken = default);

        Task<InvestorOtpVerificationResult> VerifyAsync(
            long investorOtpChallengeId,
            string phoneNumber,
            InvestorOtpPurpose purpose,
            byte[] otpHash,
            CancellationToken cancellationToken = default);
    }
}
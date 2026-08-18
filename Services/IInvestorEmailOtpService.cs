using MoneyMiners.Models;

namespace MoneyMiners.Services
{
    public interface IInvestorEmailOtpService
    {
        Task<InvestorEmailOtpChallengeResult> SendAsync(
            string emailAddress,
            InvestorOtpPurpose purpose,
            CancellationToken cancellationToken = default);

        Task<InvestorEmailOtpVerificationResult> VerifyAsync(
            long investorEmailOtpChallengeId,
            string emailAddress,
            InvestorOtpPurpose purpose,
            string otpCode,
            CancellationToken cancellationToken = default);
    }
}
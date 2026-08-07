using MoneyMiners.Models;

namespace MoneyMiners.Services
{
    public interface IInvestorOtpService
    {
        Task<InvestorOtpChallengeResult> SendAsync(
            string phoneNumber,
            InvestorOtpPurpose purpose,
            CancellationToken cancellationToken = default);

        Task<InvestorOtpVerificationResult> VerifyAsync(
            long investorOtpChallengeId,
            string phoneNumber,
            InvestorOtpPurpose purpose,
            string otpCode,
            CancellationToken cancellationToken = default);
    }
}
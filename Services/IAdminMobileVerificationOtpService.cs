using MoneyMiners.Models;

namespace MoneyMiners.Services
{
    public interface IAdminMobileVerificationOtpService
    {
        Task<AdminMobileVerificationOtpChallengeResult> SendAsync(
            long adminUserId,
            string phoneNumber,
            CancellationToken cancellationToken = default);

        Task<AdminMobileVerificationOtpVerificationResult> VerifyAsync(
            long challengeId,
            long adminUserId,
            string phoneNumber,
            string otpCode,
            CancellationToken cancellationToken = default);
    }
}
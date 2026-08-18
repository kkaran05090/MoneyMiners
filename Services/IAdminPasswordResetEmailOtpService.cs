using MoneyMiners.Models;

namespace MoneyMiners.Services
{
    public interface IAdminPasswordResetEmailOtpService
    {
        Task<AdminPasswordResetEmailOtpChallengeResult> SendAsync(
            long adminUserId,
            string emailAddress,
            CancellationToken cancellationToken = default);

        Task<AdminPasswordResetEmailOtpVerificationResult> VerifyAsync(
            long challengeId,
            long adminUserId,
            string emailAddress,
            string otpCode,
            CancellationToken cancellationToken = default);
    }
}
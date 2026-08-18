using MoneyMiners.Models;

namespace MoneyMiners.Services
{
    public interface IAdminPasswordResetOtpService
    {
        Task<AdminPasswordResetOtpChallengeResult> SendAsync(
            long adminUserId,
            string phoneNumber,
            CancellationToken cancellationToken = default);

        Task<AdminPasswordResetOtpVerificationResult> VerifyAsync(
            long challengeId,
            long adminUserId,
            string phoneNumber,
            string otpCode,
            CancellationToken cancellationToken = default);
    }
}
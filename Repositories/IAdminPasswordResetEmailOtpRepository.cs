using MoneyMiners.Models;

namespace MoneyMiners.Repositories
{
    public interface IAdminPasswordResetEmailOtpRepository
    {
        Task<AdminPasswordResetEmailOtpChallengeResult> CreateAsync(
            long adminUserId,
            string emailAddress,
            byte[] otpHash,
            DateTime expiresAtUtc,
            CancellationToken cancellationToken = default);

        Task<AdminPasswordResetEmailOtpVerificationResult> VerifyAsync(
            long adminPasswordResetEmailOtpChallengeId,
            long adminUserId,
            string emailAddress,
            byte[] otpHash,
            CancellationToken cancellationToken = default);
    }
}
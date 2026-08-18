using MoneyMiners.Models;

namespace MoneyMiners.Repositories
{
    public interface IAdminMobileVerificationOtpRepository
    {
        Task<AdminMobileVerificationOtpChallengeResult> CreateAsync(
            long adminUserId,
            string phoneNumber,
            byte[] otpHash,
            DateTime expiresAtUtc,
            CancellationToken cancellationToken = default);

        Task<AdminMobileVerificationOtpVerificationResult> VerifyAsync(
            long challengeId,
            long adminUserId,
            string phoneNumber,
            byte[] otpHash,
            CancellationToken cancellationToken = default);
    }
}
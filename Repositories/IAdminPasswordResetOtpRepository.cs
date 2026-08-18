using MoneyMiners.Models;

namespace MoneyMiners.Repositories
{
	public interface IAdminPasswordResetOtpRepository
	{
		Task<AdminPasswordResetOtpChallengeResult> CreateAsync(
			long adminUserId,
			string phoneNumber,
			byte[] otpHash,
			DateTime expiresAtUtc,
			CancellationToken cancellationToken = default);

		Task<AdminPasswordResetOtpVerificationResult> VerifyAsync(
			long adminPasswordResetOtpChallengeId,
			long adminUserId,
			string phoneNumber,
			byte[] otpHash,
			CancellationToken cancellationToken = default);
	}
}
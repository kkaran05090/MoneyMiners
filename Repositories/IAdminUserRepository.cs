using MoneyMiners.Models;
using MoneyMiners.ViewModels.Admin;

namespace MoneyMiners.Repositories
{
    public interface IAdminUserRepository
    {
        Task<List<AdminUserListItemViewModel>> GetAllAsync(
            CancellationToken cancellationToken = default);

        Task<bool> HasAnyAsync(
            CancellationToken cancellationToken = default);

        Task<AdminUser?> GetByLoginAsync(
            string loginIdentifier,
            CancellationToken cancellationToken = default);

        Task<AdminUser?> GetByIdAsync(
            long adminUserId,
            CancellationToken cancellationToken = default);

        Task<AdminUser?> GetByPhoneAsync(
            string phoneNumber,
            CancellationToken cancellationToken = default);

        Task RecordLoginAttemptAsync(
            long adminUserId,
            bool isSuccessful,
            int maxFailedAttempts = 5,
            int lockoutMinutes = 15,
            CancellationToken cancellationToken = default);

        Task<long> CreateFirstAsync(
            string username,
            string email,
            string passwordHash,
            CancellationToken cancellationToken = default);

        Task<long> CreateAsync(
            string username,
            string email,
            string phoneNumber,
            string passwordHash,
            string role = "Admin",
            CancellationToken cancellationToken = default);

        // Existing Mobile OTP password reset
        Task CompletePasswordResetAsync(
            long challengeId,
            long adminUserId,
            string phoneNumber,
            string passwordHash,
            CancellationToken cancellationToken = default);

        // Current Email OTP password reset
        Task CompletePasswordResetEmailAsync(
            long challengeId,
            long adminUserId,
            string emailAddress,
            string passwordHash,
            CancellationToken cancellationToken = default);
    }
}
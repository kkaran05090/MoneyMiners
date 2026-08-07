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
            string passwordHash,
            string role = "Admin",
            CancellationToken cancellationToken = default);
    }
}
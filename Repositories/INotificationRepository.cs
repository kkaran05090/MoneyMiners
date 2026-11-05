
using MoneyMiners.Models;

namespace MoneyMiners.Repositories
{
    public interface INotificationRepository
    {
        Task<long> CreateAsync(
            Notification notification,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<Notification>> GetAllAsync(
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<Notification>> GetPublishedAsync(
            CancellationToken cancellationToken = default);

        Task PublishAsync(
            long notificationId,
            CancellationToken cancellationToken = default);

        Task SoftDeleteAsync(
            long notificationId,
            CancellationToken cancellationToken = default);

        Task<int> GetUnreadCountAsync(
            long investorAccountId,
            CancellationToken cancellationToken = default);

        Task MarkAsReadAsync(
            long notificationId,
            long investorAccountId,
            CancellationToken cancellationToken = default);

        // Get already-read notification IDs
        Task<IReadOnlySet<long>> GetReadNotificationIdsAsync(
            long investorAccountId,
            CancellationToken cancellationToken = default);
    }
}

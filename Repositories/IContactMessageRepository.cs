using MoneyMiners.Models;
using MoneyMiners.ViewModels.Admin;

namespace MoneyMiners.Repositories
{
    public interface IContactMessageRepository
    {
        Task<long> CreateAsync(
            ContactMessage contactMessage,
            CancellationToken cancellationToken = default);

        Task<ContactMessagesPageViewModel> GetAllAsync(
            string? status,
            string? search,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default);

        Task UpdateStatusAsync(
            long contactMessageId,
            string status,
            byte[] rowVersion,
            CancellationToken cancellationToken = default);

        Task SoftDeleteAsync(
            long contactMessageId,
            byte[] rowVersion,
            CancellationToken cancellationToken = default);
    }
}
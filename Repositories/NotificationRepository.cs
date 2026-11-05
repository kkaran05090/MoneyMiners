
using Microsoft.Data.SqlClient;
using MoneyMiners.Models;
using System.Data;

namespace MoneyMiners.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly string _connectionString;

        public NotificationRepository(IConfiguration configuration)
        {
            _connectionString =
                configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException(
                    "DefaultConnection is not configured.");
        }

        // ===================================
        // Create Notification
        // ===================================

        public async Task<long> CreateAsync(
            Notification notification,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(notification);

            if (string.IsNullOrWhiteSpace(notification.Title) ||
                string.IsNullOrWhiteSpace(notification.Message))
            {
                throw new ArgumentException(
                    "Title and message are required.");
            }

            const string sql = """
                INSERT INTO dbo.Notifications
                (Title, Message, IsPublished, IsDeleted,
                 CreatedAtUtc, PublishedAtUtc)
                OUTPUT INSERTED.NotificationID
                VALUES
                (@Title, @Message, 0, 0,
                 SYSUTCDATETIME(), NULL);
                """;

            await using var connection =
                new SqlConnection(_connectionString);

            await using var command =
                new SqlCommand(sql, connection);

            command.Parameters.Add(
                "@Title", SqlDbType.NVarChar, -1
            ).Value = notification.Title;

            command.Parameters.Add(
                "@Message", SqlDbType.NVarChar, -1
            ).Value = notification.Message;

            await connection.OpenAsync(cancellationToken);

            var result = await command.ExecuteScalarAsync(
                cancellationToken);

            return Convert.ToInt64(
                result ?? throw new InvalidOperationException(
                    "Notification ID was not returned."));
        }

        // ===================================
        // Get All Notifications
        // ===================================

        public Task<IReadOnlyList<Notification>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            return GetNotificationsAsync(
                false, cancellationToken);
        }

        // ===================================
        // Get Published Notifications
        // ===================================

        public Task<IReadOnlyList<Notification>> GetPublishedAsync(
            CancellationToken cancellationToken = default)
        {
            return GetNotificationsAsync(
                true, cancellationToken);
        }

        // ===================================
        // Shared Notification Query
        // ===================================

        private async Task<IReadOnlyList<Notification>>
            GetNotificationsAsync(
                bool publishedOnly,
                CancellationToken cancellationToken)
        {
            var sql = """
                SELECT NotificationID, Title, Message,
                       IsPublished, IsDeleted,
                       CreatedAtUtc, PublishedAtUtc
                FROM dbo.Notifications
                WHERE IsDeleted = 0
                """
                + (publishedOnly
                    ? " AND IsPublished = 1 "
                    : " ")
                + " ORDER BY CreatedAtUtc DESC, NotificationID DESC;";

            var notifications = new List<Notification>();

            await using var connection =
                new SqlConnection(_connectionString);

            await using var command =
                new SqlCommand(sql, connection);

            await connection.OpenAsync(cancellationToken);

            await using var reader =
                await command.ExecuteReaderAsync(
                    cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                notifications.Add(new Notification
                {
                    NotificationID =
                        reader.GetInt64(
                            reader.GetOrdinal("NotificationID")),

                    Title =
                        reader.GetString(
                            reader.GetOrdinal("Title")),

                    Message =
                        reader.GetString(
                            reader.GetOrdinal("Message")),

                    IsPublished =
                        reader.GetBoolean(
                            reader.GetOrdinal("IsPublished")),

                    IsDeleted =
                        reader.GetBoolean(
                            reader.GetOrdinal("IsDeleted")),

                    CreatedAtUtc =
                        reader.GetDateTime(
                            reader.GetOrdinal("CreatedAtUtc")),

                    PublishedAtUtc =
                        reader.IsDBNull(
                            reader.GetOrdinal("PublishedAtUtc"))
                            ? null
                            : reader.GetDateTime(
                                reader.GetOrdinal("PublishedAtUtc"))
                });
            }

            return notifications;
        }

        // ===================================
        // Publish Notification
        // ===================================

        public async Task PublishAsync(
            long notificationId,
            CancellationToken cancellationToken = default)
        {
            const string sql = """
                UPDATE dbo.Notifications
                SET IsPublished = 1,
                    PublishedAtUtc =
                        COALESCE(PublishedAtUtc, SYSUTCDATETIME())
                WHERE NotificationID = @NotificationID
                  AND IsDeleted = 0;
                """;

            await ExecuteNotificationCommandAsync(
                sql, notificationId, cancellationToken);
        }

        // ===================================
        // Soft Delete Notification
        // ===================================

        public async Task SoftDeleteAsync(
            long notificationId,
            CancellationToken cancellationToken = default)
        {
            const string sql = """
                UPDATE dbo.Notifications
                SET IsDeleted = 1
                WHERE NotificationID = @NotificationID;
                """;

            await ExecuteNotificationCommandAsync(
                sql, notificationId, cancellationToken);
        }

        // ===================================
        // Execute Notification Command
        // ===================================

        private async Task ExecuteNotificationCommandAsync(
            string sql,
            long notificationId,
            CancellationToken cancellationToken)
        {
            await using var connection =
                new SqlConnection(_connectionString);

            await using var command =
                new SqlCommand(sql, connection);

            command.Parameters.Add(
                "@NotificationID", SqlDbType.BigInt
            ).Value = notificationId;

            await connection.OpenAsync(cancellationToken);

            await command.ExecuteNonQueryAsync(
                cancellationToken);
        }

        // ===================================
        // Get Unread Notification Count
        // ===================================

        public async Task<int> GetUnreadCountAsync(
            long investorAccountId,
            CancellationToken cancellationToken = default)
        {
            const string sql = """
                SELECT COUNT(*)
                FROM dbo.Notifications n
                WHERE n.IsPublished = 1
                  AND n.IsDeleted = 0
                  AND NOT EXISTS
                  (
                      SELECT 1
                      FROM dbo.InvestorNotificationReads r
                      WHERE r.NotificationID = n.NotificationID
                        AND r.InvestorAccountID = @InvestorAccountID
                  );
                """;

            await using var connection =
                new SqlConnection(_connectionString);

            await using var command =
                new SqlCommand(sql, connection);

            command.Parameters.Add(
                "@InvestorAccountID", SqlDbType.BigInt
            ).Value = investorAccountId;

            await connection.OpenAsync(cancellationToken);

            var result = await command.ExecuteScalarAsync(
                cancellationToken);

            return Convert.ToInt32(result);
        }

        // ===================================
        // Mark Notification as Read
        // ===================================

        public async Task MarkAsReadAsync(
            long notificationId,
            long investorAccountId,
            CancellationToken cancellationToken = default)
        {
            const string sql = """
                INSERT INTO dbo.InvestorNotificationReads
                (NotificationID, InvestorAccountID, ReadAtUtc)
                SELECT n.NotificationID,
                       @InvestorAccountID,
                       SYSUTCDATETIME()
                FROM dbo.Notifications n
                WHERE n.NotificationID = @NotificationID
                  AND n.IsPublished = 1
                  AND n.IsDeleted = 0
                  AND NOT EXISTS
                  (
                      SELECT 1
                      FROM dbo.InvestorNotificationReads r
                           WITH (UPDLOCK, HOLDLOCK)
                      WHERE r.NotificationID = @NotificationID
                        AND r.InvestorAccountID = @InvestorAccountID
                  );
                """;

            await using var connection =
                new SqlConnection(_connectionString);

            await using var command =
                new SqlCommand(sql, connection);

            command.Parameters.Add(
                "@NotificationID", SqlDbType.BigInt
            ).Value = notificationId;

            command.Parameters.Add(
                "@InvestorAccountID", SqlDbType.BigInt
            ).Value = investorAccountId;

            await connection.OpenAsync(cancellationToken);

            await command.ExecuteNonQueryAsync(
                cancellationToken);
        }

        // ===================================
        // Get Already-Read Notification IDs
        // ===================================

        public async Task<IReadOnlySet<long>> GetReadNotificationIdsAsync(
            long investorAccountId,
            CancellationToken cancellationToken = default)
        {
            const string sql = """
                SELECT r.NotificationID
                FROM dbo.InvestorNotificationReads r
                INNER JOIN dbo.Notifications n
                    ON n.NotificationID = r.NotificationID
                WHERE r.InvestorAccountID = @InvestorAccountID
                  AND n.IsPublished = 1
                  AND n.IsDeleted = 0;
                """;

            var readNotificationIds = new HashSet<long>();

            await using var connection =
                new SqlConnection(_connectionString);

            await using var command =
                new SqlCommand(sql, connection);

            command.Parameters.Add(
                "@InvestorAccountID", SqlDbType.BigInt
            ).Value = investorAccountId;

            await connection.OpenAsync(cancellationToken);

            await using var reader =
                await command.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                readNotificationIds.Add(
                    reader.GetInt64(
                        reader.GetOrdinal("NotificationID")));
            }

            return readNotificationIds;
        }
    }
}

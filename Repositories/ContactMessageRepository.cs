using Microsoft.Data.SqlClient;
using MoneyMiners.Models;
using MoneyMiners.ViewModels.Admin;
using System.Data;

namespace MoneyMiners.Repositories
{
    public class ContactMessageRepository
        : IContactMessageRepository
    {
        private readonly string _connectionString;

        public ContactMessageRepository(
            IConfiguration configuration)
        {
            _connectionString =
                configuration.GetConnectionString(
                    "DefaultConnection")
                ?? throw new InvalidOperationException(
                    "DefaultConnection is not configured.");
        }

        public async Task<long> CreateAsync(
            ContactMessage contactMessage,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(contactMessage);

            await using var connection =
                new SqlConnection(_connectionString);

            await using var command =
                new SqlCommand(
                    "dbo.usp_ContactMessages_Create",
                    connection);

            command.CommandType =
                CommandType.StoredProcedure;

            command.Parameters.Add(
                "@FullName",
                SqlDbType.NVarChar,
                100
            ).Value = contactMessage.FullName;

            command.Parameters.Add(
                "@Email",
                SqlDbType.NVarChar,
                256
            ).Value = contactMessage.Email;

            command.Parameters.Add(
                "@PhoneNumber",
                SqlDbType.VarChar,
                20
            ).Value = contactMessage.PhoneNumber;

            command.Parameters.Add(
                "@Subject",
                SqlDbType.NVarChar,
                100
            ).Value = contactMessage.Subject;

            command.Parameters.Add(
                "@Message",
                SqlDbType.NVarChar,
                2000
            ).Value = contactMessage.Message;

            await connection.OpenAsync(cancellationToken);

            await using var reader =
                await command.ExecuteReaderAsync(
                    cancellationToken);

            if (!await reader.ReadAsync(cancellationToken))
            {
                throw new InvalidOperationException(
                    "Stored procedure did not return a result.");
            }

            return reader.GetInt64(
                reader.GetOrdinal("ContactMessageID"));
        }

        public async Task<ContactMessagesPageViewModel> GetAllAsync(
            string? status,
            string? search,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var pageModel =
                new ContactMessagesPageViewModel
                {
                    Status = status,
                    Search = search,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };

            await using var connection =
                new SqlConnection(_connectionString);

            await using var command =
                new SqlCommand(
                    "dbo.usp_ContactMessages_GetAll",
                    connection);

            command.CommandType =
                CommandType.StoredProcedure;

            command.Parameters.Add(
                "@Status",
                SqlDbType.VarChar,
                20
            ).Value =
                string.IsNullOrWhiteSpace(status)
                    ? DBNull.Value
                    : status;

            command.Parameters.Add(
                "@Search",
                SqlDbType.NVarChar,
                200
            ).Value =
                string.IsNullOrWhiteSpace(search)
                    ? DBNull.Value
                    : search;

            command.Parameters.Add(
                "@PageNumber",
                SqlDbType.Int
            ).Value = pageNumber;

            command.Parameters.Add(
                "@PageSize",
                SqlDbType.Int
            ).Value = pageSize;

            await connection.OpenAsync(cancellationToken);

            await using var reader =
                await command.ExecuteReaderAsync(
                    cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                var message =
                    new ContactMessageAdminViewModel
                    {
                        ContactMessageID =
                            reader.GetInt64(
                                reader.GetOrdinal(
                                    "ContactMessageID")),

                        FullName =
                            reader.GetString(
                                reader.GetOrdinal("FullName")),

                        Email =
                            reader.GetString(
                                reader.GetOrdinal("Email")),

                        PhoneNumber =
                            reader.GetString(
                                reader.GetOrdinal(
                                    "PhoneNumber")),

                        Subject =
                            reader.GetString(
                                reader.GetOrdinal("Subject")),

                        Message =
                            reader.GetString(
                                reader.GetOrdinal("Message")),

                        Status =
                            reader.GetString(
                                reader.GetOrdinal("Status")),

                        CreatedAt =
                            reader.GetDateTime(
                                reader.GetOrdinal("CreatedAt")),

                        UpdatedAt =
                            reader.IsDBNull(
                                reader.GetOrdinal("UpdatedAt"))
                                ? null
                                : reader.GetDateTime(
                                    reader.GetOrdinal(
                                        "UpdatedAt")),

                        RowVersion =
                            reader.GetFieldValue<byte[]>(
                                reader.GetOrdinal(
                                    "RowVersion")),

                        TotalRecords =
                            reader.GetInt32(
                                reader.GetOrdinal(
                                    "TotalRecords"))
                    };

                pageModel.Messages.Add(message);
            }

            if (pageModel.Messages.Count > 0)
            {
                pageModel.TotalRecords =
                    pageModel.Messages[0].TotalRecords;
            }

            return pageModel;
        }

        public async Task UpdateStatusAsync(
            long contactMessageId,
            string status,
            byte[] rowVersion,
            CancellationToken cancellationToken = default)
        {
            await using var connection =
                new SqlConnection(_connectionString);

            await using var command =
                new SqlCommand(
                    "dbo.usp_ContactMessages_UpdateStatus",
                    connection);

            command.CommandType =
                CommandType.StoredProcedure;

            command.Parameters.Add(
                "@ContactMessageID",
                SqlDbType.BigInt
            ).Value = contactMessageId;

            command.Parameters.Add(
                "@Status",
                SqlDbType.VarChar,
                20
            ).Value = status;

            command.Parameters.Add(
                "@RowVersion",
                SqlDbType.Binary,
                8
            ).Value = rowVersion;

            await connection.OpenAsync(cancellationToken);

            await command.ExecuteNonQueryAsync(
                cancellationToken);
        }

        public async Task SoftDeleteAsync(
            long contactMessageId,
            byte[] rowVersion,
            CancellationToken cancellationToken = default)
        {
            await using var connection =
                new SqlConnection(_connectionString);

            await using var command =
                new SqlCommand(
                    "dbo.usp_ContactMessages_SoftDelete",
                    connection);

            command.CommandType =
                CommandType.StoredProcedure;

            command.Parameters.Add(
                "@ContactMessageID",
                SqlDbType.BigInt
            ).Value = contactMessageId;

            command.Parameters.Add(
                "@RowVersion",
                SqlDbType.Binary,
                8
            ).Value = rowVersion;

            await connection.OpenAsync(cancellationToken);

            await command.ExecuteNonQueryAsync(
                cancellationToken);
        }
    }
}
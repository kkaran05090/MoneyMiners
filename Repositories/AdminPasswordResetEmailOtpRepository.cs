using Microsoft.Data.SqlClient;
using MoneyMiners.Models;
using System.Data;

namespace MoneyMiners.Repositories
{
    public sealed class AdminPasswordResetEmailOtpRepository
        : IAdminPasswordResetEmailOtpRepository
    {
        private readonly string _connectionString;

        public AdminPasswordResetEmailOtpRepository(
            IConfiguration configuration)
        {
            _connectionString =
                configuration.GetConnectionString(
                    "DefaultConnection")
                ?? throw new InvalidOperationException(
                    "DefaultConnection was not found.");
        }


        public async Task<AdminPasswordResetEmailOtpChallengeResult> CreateAsync(
            long adminUserId,
            string emailAddress,
            byte[] otpHash,
            DateTime expiresAtUtc,
            CancellationToken cancellationToken = default)
        {
            ValidateOtpHash(
                otpHash);

            await using var connection =
                new SqlConnection(
                    _connectionString);

            await using var command =
                new SqlCommand(
                    "dbo.usp_AdminPasswordResetEmailOtp_Create",
                    connection);

            command.CommandType =
                CommandType.StoredProcedure;

            command.Parameters.Add(
                "@AdminUserID",
                SqlDbType.BigInt).Value =
                adminUserId;

            command.Parameters.Add(
                "@EmailAddress",
                SqlDbType.NVarChar,
                256).Value =
                emailAddress
                    .Trim()
                    .ToLowerInvariant();

            command.Parameters.Add(
                "@OtpHash",
                SqlDbType.Binary,
                32).Value =
                otpHash;

            command.Parameters.Add(
                "@ExpiresAtUtc",
                SqlDbType.DateTime2).Value =
                expiresAtUtc;

            await connection.OpenAsync(
                cancellationToken);

            await using var reader =
                await command.ExecuteReaderAsync(
                    CommandBehavior.SingleRow,
                    cancellationToken);

            if (!await reader.ReadAsync(
                    cancellationToken))
            {
                throw new InvalidOperationException(
                    "Admin email OTP creation did not return a result.");
            }

            return new AdminPasswordResetEmailOtpChallengeResult
            {
                AdminPasswordResetEmailOtpChallengeID =
                    reader.GetInt64(
                        reader.GetOrdinal(
                            "AdminPasswordResetEmailOtpChallengeID")),

                AdminUserID =
                    reader.GetInt64(
                        reader.GetOrdinal(
                            "AdminUserID")),

                EmailAddress =
                    reader.GetString(
                        reader.GetOrdinal(
                            "EmailAddress")),

                ExpiresAtUtc =
                    reader.GetDateTime(
                        reader.GetOrdinal(
                            "ExpiresAtUtc"))
            };
        }


        public async Task<AdminPasswordResetEmailOtpVerificationResult> VerifyAsync(
            long adminPasswordResetEmailOtpChallengeId,
            long adminUserId,
            string emailAddress,
            byte[] otpHash,
            CancellationToken cancellationToken = default)
        {
            ValidateOtpHash(
                otpHash);

            await using var connection =
                new SqlConnection(
                    _connectionString);

            await using var command =
                new SqlCommand(
                    "dbo.usp_AdminPasswordResetEmailOtp_Verify",
                    connection);

            command.CommandType =
                CommandType.StoredProcedure;

            command.Parameters.Add(
                "@AdminPasswordResetEmailOtpChallengeID",
                SqlDbType.BigInt).Value =
                adminPasswordResetEmailOtpChallengeId;

            command.Parameters.Add(
                "@AdminUserID",
                SqlDbType.BigInt).Value =
                adminUserId;

            command.Parameters.Add(
                "@EmailAddress",
                SqlDbType.NVarChar,
                256).Value =
                emailAddress
                    .Trim()
                    .ToLowerInvariant();

            command.Parameters.Add(
                "@OtpHash",
                SqlDbType.Binary,
                32).Value =
                otpHash;

            await connection.OpenAsync(
                cancellationToken);

            await using var reader =
                await command.ExecuteReaderAsync(
                    CommandBehavior.SingleRow,
                    cancellationToken);

            if (!await reader.ReadAsync(
                    cancellationToken))
            {
                throw new InvalidOperationException(
                    "Admin email OTP verification did not return a result.");
            }

            var verifiedAtOrdinal =
                reader.GetOrdinal(
                    "VerifiedAtUtc");

            return new AdminPasswordResetEmailOtpVerificationResult
            {
                AdminPasswordResetEmailOtpChallengeID =
                    reader.GetInt64(
                        reader.GetOrdinal(
                            "AdminPasswordResetEmailOtpChallengeID")),

                AdminUserID =
                    reader.GetInt64(
                        reader.GetOrdinal(
                            "AdminUserID")),

                EmailAddress =
                    reader.GetString(
                        reader.GetOrdinal(
                            "EmailAddress")),

                IsVerified =
                    reader.GetBoolean(
                        reader.GetOrdinal(
                            "IsVerified")),

                VerifiedAtUtc =
                    reader.IsDBNull(
                        verifiedAtOrdinal)
                        ? null
                        : reader.GetDateTime(
                            verifiedAtOrdinal)
            };
        }


        private static void ValidateOtpHash(
            byte[] otpHash)
        {
            ArgumentNullException.ThrowIfNull(
                otpHash);

            if (otpHash.Length != 32)
            {
                throw new ArgumentException(
                    "OTP hash must contain exactly 32 bytes.",
                    nameof(otpHash));
            }
        }
    }
}
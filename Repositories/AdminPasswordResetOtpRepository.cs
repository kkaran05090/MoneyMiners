using Microsoft.Data.SqlClient;
using MoneyMiners.Models;
using System.Data;

namespace MoneyMiners.Repositories
{
    public sealed class AdminPasswordResetOtpRepository
        : IAdminPasswordResetOtpRepository
    {
        private readonly string _connectionString;

        public AdminPasswordResetOtpRepository(
            IConfiguration configuration)
        {
            _connectionString =
                configuration.GetConnectionString(
                    "DefaultConnection")
                ?? throw new InvalidOperationException(
                    "DefaultConnection was not found.");
        }


        public async Task<AdminPasswordResetOtpChallengeResult> CreateAsync(
            long adminUserId,
            string phoneNumber,
            byte[] otpHash,
            DateTime expiresAtUtc,
            CancellationToken cancellationToken = default)
        {
            ValidateOtpHash(otpHash);

            await using var connection =
                new SqlConnection(_connectionString);

            await using var command =
                new SqlCommand(
                    "dbo.usp_AdminPasswordResetOtp_Create",
                    connection);

            command.CommandType =
                CommandType.StoredProcedure;

            command.Parameters.Add(
                "@AdminUserID",
                SqlDbType.BigInt).Value =
                adminUserId;

            command.Parameters.Add(
                "@PhoneNumber",
                SqlDbType.VarChar,
                15).Value =
                phoneNumber.Trim();

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
                    "Admin OTP creation did not return a result.");
            }

            return new AdminPasswordResetOtpChallengeResult
            {
                AdminPasswordResetOtpChallengeID =
                    reader.GetInt64(
                        reader.GetOrdinal(
                            "AdminPasswordResetOtpChallengeID")),

                AdminUserID =
                    reader.GetInt64(
                        reader.GetOrdinal(
                            "AdminUserID")),

                PhoneNumber =
                    reader.GetString(
                        reader.GetOrdinal(
                            "PhoneNumber")),

                ExpiresAtUtc =
                    reader.GetDateTime(
                        reader.GetOrdinal(
                            "ExpiresAtUtc"))
            };
        }


        public async Task<AdminPasswordResetOtpVerificationResult> VerifyAsync(
            long adminPasswordResetOtpChallengeId,
            long adminUserId,
            string phoneNumber,
            byte[] otpHash,
            CancellationToken cancellationToken = default)
        {
            ValidateOtpHash(otpHash);

            await using var connection =
                new SqlConnection(_connectionString);

            await using var command =
                new SqlCommand(
                    "dbo.usp_AdminPasswordResetOtp_Verify",
                    connection);

            command.CommandType =
                CommandType.StoredProcedure;

            command.Parameters.Add(
                "@AdminPasswordResetOtpChallengeID",
                SqlDbType.BigInt).Value =
                adminPasswordResetOtpChallengeId;

            command.Parameters.Add(
                "@AdminUserID",
                SqlDbType.BigInt).Value =
                adminUserId;

            command.Parameters.Add(
                "@PhoneNumber",
                SqlDbType.VarChar,
                15).Value =
                phoneNumber.Trim();

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
                    "Admin OTP verification did not return a result.");
            }

            var verifiedAtOrdinal =
                reader.GetOrdinal(
                    "VerifiedAtUtc");

            return new AdminPasswordResetOtpVerificationResult
            {
                AdminPasswordResetOtpChallengeID =
                    reader.GetInt64(
                        reader.GetOrdinal(
                            "AdminPasswordResetOtpChallengeID")),

                AdminUserID =
                    reader.GetInt64(
                        reader.GetOrdinal(
                            "AdminUserID")),

                PhoneNumber =
                    reader.GetString(
                        reader.GetOrdinal(
                            "PhoneNumber")),

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
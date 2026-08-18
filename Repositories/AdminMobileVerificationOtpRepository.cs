using Microsoft.Data.SqlClient;
using MoneyMiners.Models;
using System.Data;

namespace MoneyMiners.Repositories
{
    public sealed class AdminMobileVerificationOtpRepository
        : IAdminMobileVerificationOtpRepository
    {
        private readonly string _connectionString;


        public AdminMobileVerificationOtpRepository(
            IConfiguration configuration)
        {
            _connectionString =
                configuration.GetConnectionString(
                    "DefaultConnection")
                ?? throw new InvalidOperationException(
                    "DefaultConnection was not found.");
        }


        public async Task<AdminMobileVerificationOtpChallengeResult> CreateAsync(
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
                    "dbo.usp_AdminMobileVerificationOtp_Create",
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
                    "Admin mobile verification OTP creation did not return a result.");
            }


            return new AdminMobileVerificationOtpChallengeResult
            {
                AdminMobileVerificationOtpChallengeID =
                    reader.GetInt64(
                        reader.GetOrdinal(
                            "AdminMobileVerificationOtpChallengeID")),

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


        public async Task<AdminMobileVerificationOtpVerificationResult> VerifyAsync(
            long challengeId,
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
                    "dbo.usp_AdminMobileVerificationOtp_Verify",
                    connection);

            command.CommandType =
                CommandType.StoredProcedure;

            command.Parameters.Add(
                "@ChallengeID",
                SqlDbType.BigInt).Value =
                challengeId;

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
                    "Admin mobile verification OTP verification did not return a result.");
            }


            var verifiedAtOrdinal =
                reader.GetOrdinal(
                    "VerifiedAtUtc");


            return new AdminMobileVerificationOtpVerificationResult
            {
                AdminMobileVerificationOtpChallengeID =
                    reader.GetInt64(
                        reader.GetOrdinal(
                            "AdminMobileVerificationOtpChallengeID")),

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


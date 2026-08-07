using Microsoft.Data.SqlClient;
using MoneyMiners.Models;
using System.Data;

namespace MoneyMiners.Repositories
{
    public sealed class InvestorOtpRepository
        : IInvestorOtpRepository
    {
        private readonly string _connectionString;

        public InvestorOtpRepository(
            IConfiguration configuration)
        {
            _connectionString =
                configuration.GetConnectionString(
                    "DefaultConnection")
                ?? throw new InvalidOperationException(
                    "DefaultConnection was not found.");
        }

        public async Task<InvestorOtpChallengeResult> CreateAsync(
            string phoneNumber,
            InvestorOtpPurpose purpose,
            byte[] otpHash,
            DateTime expiresAtUtc,
            CancellationToken cancellationToken = default)
        {
            ValidateOtpHash(otpHash);

            await using var connection =
                new SqlConnection(_connectionString);

            await using var command =
                new SqlCommand(
                    "dbo.usp_InvestorOtp_Create",
                    connection);

            command.CommandType =
                CommandType.StoredProcedure;

            command.Parameters.Add(
                "@PhoneNumber",
                SqlDbType.VarChar,
                15).Value =
                phoneNumber.Trim();

            command.Parameters.Add(
                "@Purpose",
                SqlDbType.NVarChar,
                30).Value =
                GetPurposeValue(purpose);

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
                    "OTP creation did not return a result.");
            }

            return new InvestorOtpChallengeResult
            {
                InvestorOtpChallengeID =
                    reader.GetInt64(
                        reader.GetOrdinal(
                            "InvestorOtpChallengeID")),

                PhoneNumber =
                    reader.GetString(
                        reader.GetOrdinal(
                            "PhoneNumber")),

                Purpose =
                    reader.GetString(
                        reader.GetOrdinal(
                            "Purpose")),

                ExpiresAtUtc =
                    reader.GetDateTime(
                        reader.GetOrdinal(
                            "ExpiresAtUtc"))
            };
        }

        public async Task<InvestorOtpVerificationResult> VerifyAsync(
            long investorOtpChallengeId,
            string phoneNumber,
            InvestorOtpPurpose purpose,
            byte[] otpHash,
            CancellationToken cancellationToken = default)
        {
            ValidateOtpHash(otpHash);

            await using var connection =
                new SqlConnection(_connectionString);

            await using var command =
                new SqlCommand(
                    "dbo.usp_InvestorOtp_Verify",
                    connection);

            command.CommandType =
                CommandType.StoredProcedure;

            command.Parameters.Add(
                "@InvestorOtpChallengeID",
                SqlDbType.BigInt).Value =
                investorOtpChallengeId;

            command.Parameters.Add(
                "@PhoneNumber",
                SqlDbType.VarChar,
                15).Value =
                phoneNumber.Trim();

            command.Parameters.Add(
                "@Purpose",
                SqlDbType.NVarChar,
                30).Value =
                GetPurposeValue(purpose);

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
                    "OTP verification did not return a result.");
            }

            var verifiedAtOrdinal =
                reader.GetOrdinal(
                    "VerifiedAtUtc");

            return new InvestorOtpVerificationResult
            {
                InvestorOtpChallengeID =
                    reader.GetInt64(
                        reader.GetOrdinal(
                            "InvestorOtpChallengeID")),

                PhoneNumber =
                    reader.GetString(
                        reader.GetOrdinal(
                            "PhoneNumber")),

                Purpose =
                    reader.GetString(
                        reader.GetOrdinal(
                            "Purpose")),

                IsVerified =
                    reader.GetBoolean(
                        reader.GetOrdinal(
                            "IsVerified")),

                VerifiedAtUtc =
                    reader.IsDBNull(verifiedAtOrdinal)
                        ? null
                        : reader.GetDateTime(
                            verifiedAtOrdinal)
            };
        }

        private static string GetPurposeValue(
            InvestorOtpPurpose purpose)
        {
            return purpose switch
            {
                InvestorOtpPurpose.Registration =>
                    "Registration",

                InvestorOtpPurpose.PasswordReset =>
                    "PasswordReset",

                _ => throw new ArgumentOutOfRangeException(
                    nameof(purpose),
                    purpose,
                    "Unsupported OTP purpose.")
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

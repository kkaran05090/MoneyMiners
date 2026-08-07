using Microsoft.Data.SqlClient;
using MoneyMiners.Models;
using System.Data;

namespace MoneyMiners.Repositories
{
    public sealed class InvestorAccountRepository
        : IInvestorAccountRepository
    {
        private readonly string _connectionString;

        public InvestorAccountRepository(
            IConfiguration configuration)
        {
            _connectionString =
                configuration.GetConnectionString(
                    "DefaultConnection")
                ?? throw new InvalidOperationException(
                    "DefaultConnection was not found.");
        }

        public async Task<InvestorRegistrationResult> RegisterAsync(
            InvestorRegistrationCommand registration,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(registration);

            await using var connection =
                new SqlConnection(_connectionString);

            await using var command =
                new SqlCommand(
                    "dbo.usp_InvestorAccounts_Register",
                    connection);

            command.CommandType =
                CommandType.StoredProcedure;

            command.Parameters.Add(
              "@InvestorOtpChallengeID",
              SqlDbType.BigInt).Value =
              registration.InvestorOtpChallengeID;

            AddRequiredString(
                command,
                "@FirstName",
                SqlDbType.NVarChar,
                60,
                registration.FirstName);

            AddNullableString(
                command,
                "@LastName",
                SqlDbType.NVarChar,
                60,
                registration.LastName);

            AddNullableString(
                command,
                "@FatherName",
                SqlDbType.NVarChar,
                100,
                registration.FatherName);

            AddRequiredString(
                command,
                "@PhoneNumber",
                SqlDbType.VarChar,
                15,
                registration.PhoneNumber);

            AddNullableString(
                command,
                "@Email",
                SqlDbType.NVarChar,
                256,
                registration.Email);

            AddNullableString(
                command,
                "@AddressLine1",
                SqlDbType.NVarChar,
                200,
                registration.AddressLine1);

            AddNullableString(
                command,
                "@AddressLine2",
                SqlDbType.NVarChar,
                200,
                registration.AddressLine2);

            AddNullableString(
                command,
                "@City",
                SqlDbType.NVarChar,
                100,
                registration.City);

            AddNullableString(
                command,
                "@State",
                SqlDbType.NVarChar,
                100,
                registration.State);

            AddRequiredString(
                command,
                "@Country",
                SqlDbType.NVarChar,
                100,
                registration.Country);

            AddNullableString(
                command,
                "@PostalCode",
                SqlDbType.VarChar,
                10,
                registration.PostalCode);

            command.Parameters.Add(
                "@AadhaarCipherText",
                SqlDbType.VarBinary,
                512).Value =
                registration.AadhaarCipherText;

            command.Parameters.Add(
                "@AadhaarHash",
                SqlDbType.Binary,
                32).Value =
                registration.AadhaarHash;

            AddRequiredString(
                command,
                "@AadhaarLast4",
                SqlDbType.Char,
                4,
                registration.AadhaarLast4);

            command.Parameters.Add(
                "@PANCipherText",
                SqlDbType.VarBinary,
                512).Value =
                registration.PANCipherText is null
                    ? DBNull.Value
                    : registration.PANCipherText;

            command.Parameters.Add(
                "@PANHash",
                SqlDbType.Binary,
                32).Value =
                registration.PANHash is null
                    ? DBNull.Value
                    : registration.PANHash;

            AddNullableString(
                command,
                "@PANLast4",
                SqlDbType.Char,
                4,
                registration.PANLast4);

            AddRequiredString(
                command,
                "@PasswordHash",
                SqlDbType.NVarChar,
                500,
                registration.PasswordHash);

            //command.Parameters.Add(
            //    "@IsMobileVerified",
            //    SqlDbType.Bit).Value =
            //    registration.IsMobileVerified;

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
                    "Investor registration did not return a result.");
            }

            return new InvestorRegistrationResult
            {
                InvestorProfileID =
                    reader.GetInt32(
                        reader.GetOrdinal(
                            "InvestorProfileID")),

                InvestorAccountID =
                    reader.GetInt64(
                        reader.GetOrdinal(
                            "InvestorAccountID")),

                InvestorCode =
                    reader.GetString(
                        reader.GetOrdinal(
                            "InvestorCode")),

                FirstName =
                    reader.GetString(
                        reader.GetOrdinal(
                            "FirstName")),

                LastName =
                    GetNullableString(
                        reader,
                        "LastName"),

                PhoneNumber =
                    reader.GetString(
                        reader.GetOrdinal(
                            "PhoneNumber"))
            };
        }


        public async Task<InvestorAccount?> GetByLoginAsync(
        string loginIdentifier,
        CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(loginIdentifier))
            {
                throw new ArgumentException(
                    "Investor ID or mobile number is required.",
                    nameof(loginIdentifier));
            }

            await using var connection =
                new SqlConnection(_connectionString);

            await using var command =
                new SqlCommand(
                    "dbo.usp_InvestorAccounts_GetByLogin",
                    connection);

            command.CommandType =
                CommandType.StoredProcedure;

            command.Parameters.Add(
                "@LoginIdentifier",
                SqlDbType.NVarChar,
                100).Value =
                loginIdentifier.Trim();

            await connection.OpenAsync(
                cancellationToken);

            await using var reader =
                await command.ExecuteReaderAsync(
                    CommandBehavior.SingleRow,
                    cancellationToken);

            if (!await reader.ReadAsync(
                    cancellationToken))
            {
                return null;
            }

            var lastNameOrdinal =
                reader.GetOrdinal("LastName");

            var lockoutEndOrdinal =
                reader.GetOrdinal("LockoutEndUtc");

            var lastLoginOrdinal =
                reader.GetOrdinal("LastLoginAtUtc");

            var passwordChangedOrdinal =
                reader.GetOrdinal("PasswordChangedAtUtc");

            return new InvestorAccount
            {
                InvestorAccountID =
                    reader.GetInt64(
                        reader.GetOrdinal(
                            "InvestorAccountID")),

                InvestorProfileID =
                    reader.GetInt32(
                        reader.GetOrdinal(
                            "InvestorProfileID")),

                InvestorCode =
                    reader.GetString(
                        reader.GetOrdinal(
                            "InvestorCode")),

                PhoneNumber =
                    reader.GetString(
                        reader.GetOrdinal(
                            "PhoneNumber")),

                FirstName =
                    reader.GetString(
                        reader.GetOrdinal(
                            "FirstName")),

                LastName =
                    reader.IsDBNull(lastNameOrdinal)
                        ? null
                        : reader.GetString(lastNameOrdinal),

                PasswordHash =
                    reader.GetString(
                        reader.GetOrdinal(
                            "PasswordHash")),

                IsMobileVerified =
                    reader.GetBoolean(
                        reader.GetOrdinal(
                            "IsMobileVerified")),

                IsActive =
                    reader.GetBoolean(
                        reader.GetOrdinal(
                            "IsActive")),

                FailedLoginCount =
                    reader.GetInt32(
                        reader.GetOrdinal(
                            "FailedLoginCount")),

                LockoutEndUtc =
                    reader.IsDBNull(lockoutEndOrdinal)
                        ? null
                        : reader.GetDateTime(
                            lockoutEndOrdinal),

                LastLoginAtUtc =
                    reader.IsDBNull(lastLoginOrdinal)
                        ? null
                        : reader.GetDateTime(
                            lastLoginOrdinal),

                PasswordChangedAtUtc =
                    reader.IsDBNull(passwordChangedOrdinal)
                        ? null
                        : reader.GetDateTime(
                            passwordChangedOrdinal),

                SecurityStamp =
                    reader.GetGuid(
                        reader.GetOrdinal(
                            "SecurityStamp"))
            };
        }


        public async Task<InvestorLookupResult?> GetByInvestorCodeAsync(
        string investorCode,
        CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(investorCode))
            {
                throw new ArgumentException(
                    "Investor ID is required.",
                    nameof(investorCode));
            }

            await using var connection =
                new SqlConnection(_connectionString);

            await using var command =
                new SqlCommand(
                    "dbo.usp_InvestorAccounts_GetByInvestorCode",
                    connection);

            command.CommandType =
                CommandType.StoredProcedure;

            AddRequiredString(
                command,
                "@InvestorCode",
                SqlDbType.NVarChar,
                60,
                investorCode.Trim());

            await connection.OpenAsync(
                cancellationToken);

            await using var reader =
                await command.ExecuteReaderAsync(
                    CommandBehavior.SingleRow,
                    cancellationToken);

            if (!await reader.ReadAsync(
                    cancellationToken))
            {
                return null;
            }

            return new InvestorLookupResult
            {
                InvestorAccountID =
                    reader.GetInt64(
                        reader.GetOrdinal("InvestorAccountID")),

                InvestorProfileID =
                    reader.GetInt32(
                        reader.GetOrdinal("InvestorProfileID")),

                InvestorCode =
                    reader.GetString(
                        reader.GetOrdinal("InvestorCode")),

                FirstName =
                    reader.GetString(
                        reader.GetOrdinal("FirstName")),

                LastName =
                    reader.IsDBNull(
                        reader.GetOrdinal("LastName"))
                        ? null
                        : reader.GetString(
                            reader.GetOrdinal("LastName")),

                PhoneNumber =
                    reader.GetString(
                        reader.GetOrdinal("PhoneNumber")),

                Email =
                    reader.IsDBNull(
                        reader.GetOrdinal("Email"))
                        ? null
                        : reader.GetString(
                            reader.GetOrdinal("Email")),

                AadhaarLast4 =
                    reader.GetString(
                        reader.GetOrdinal("AadhaarLast4")),

                IsActive =
                    reader.GetBoolean(
                        reader.GetOrdinal("IsActive")),

                IsMobileVerified =
                    reader.GetBoolean(
                        reader.GetOrdinal("IsMobileVerified"))
            };
        }

        public async Task<InvestorLoginAttemptResult>
            RecordLoginAttemptAsync(
                long investorAccountId,
                bool succeeded,
                CancellationToken cancellationToken = default)
        {
            if (investorAccountId <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(investorAccountId),
                    "Invalid investor account.");
            }

            await using var connection =
                new SqlConnection(_connectionString);

            await using var command =
                new SqlCommand(
                    "dbo.usp_InvestorAccounts_RecordLoginAttempt",
                    connection);

            command.CommandType =
                CommandType.StoredProcedure;

            command.Parameters.Add(
                "@InvestorAccountID",
                SqlDbType.BigInt).Value =
                investorAccountId;

            command.Parameters.Add(
                "@Succeeded",
                SqlDbType.Bit).Value =
                succeeded;

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
                    "Login attempt did not return a result.");
            }

            var lockoutEndOrdinal =
                reader.GetOrdinal("LockoutEndUtc");

            var lastLoginOrdinal =
                reader.GetOrdinal("LastLoginAtUtc");

            return new InvestorLoginAttemptResult
            {
                InvestorAccountID =
                    reader.GetInt64(
                        reader.GetOrdinal(
                            "InvestorAccountID")),

                FailedLoginCount =
                    reader.GetInt32(
                        reader.GetOrdinal(
                            "FailedLoginCount")),

                LockoutEndUtc =
                    reader.IsDBNull(lockoutEndOrdinal)
                        ? null
                        : reader.GetDateTime(
                            lockoutEndOrdinal),

                LastLoginAtUtc =
                    reader.IsDBNull(lastLoginOrdinal)
                        ? null
                        : reader.GetDateTime(
                            lastLoginOrdinal)
            };
        }


        public async Task<InvestorPasswordResetResult> ResetPasswordAsync(
    InvestorPasswordResetCommand resetCommand,
    CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(
                resetCommand);

            if (resetCommand.InvestorOtpChallengeID <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(resetCommand.InvestorOtpChallengeID),
                    "Invalid OTP challenge.");
            }

            if (string.IsNullOrWhiteSpace(
                    resetCommand.PhoneNumber))
            {
                throw new ArgumentException(
                    "Mobile number is required.",
                    nameof(resetCommand.PhoneNumber));
            }

            if (string.IsNullOrWhiteSpace(
                    resetCommand.PasswordHash))
            {
                throw new ArgumentException(
                    "Password hash is required.",
                    nameof(resetCommand.PasswordHash));
            }

            await using var connection =
                new SqlConnection(_connectionString);

            await using var command =
                new SqlCommand(
                    "dbo.usp_InvestorAccounts_ResetPassword",
                    connection);

            command.CommandType =
                CommandType.StoredProcedure;

            command.Parameters.Add(
                "@InvestorOtpChallengeID",
                SqlDbType.BigInt).Value =
                resetCommand.InvestorOtpChallengeID;

            AddRequiredString(
                command,
                "@PhoneNumber",
                SqlDbType.VarChar,
                15,
                resetCommand.PhoneNumber);

            AddRequiredString(
                command,
                "@PasswordHash",
                SqlDbType.NVarChar,
                500,
                resetCommand.PasswordHash);

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
                    "Password reset did not return a result.");
            }

            return new InvestorPasswordResetResult
            {
                InvestorAccountID =
                    reader.GetInt64(
                        reader.GetOrdinal(
                            "InvestorAccountID")),

                InvestorProfileID =
                    reader.GetInt32(
                        reader.GetOrdinal(
                            "InvestorProfileID")),

                InvestorCode =
                    reader.GetString(
                        reader.GetOrdinal(
                            "InvestorCode")),

                PhoneNumber =
                    reader.GetString(
                        reader.GetOrdinal(
                            "PhoneNumber")),

                PasswordChangedAtUtc =
                    reader.GetDateTime(
                        reader.GetOrdinal(
                            "PasswordChangedAtUtc"))
            };
        }


        private static void AddRequiredString(
            SqlCommand command,
            string parameterName,
            SqlDbType databaseType,
            int size,
            string value)
        {
            command.Parameters.Add(
                parameterName,
                databaseType,
                size).Value =
                value.Trim();
        }

        private static void AddNullableString(
            SqlCommand command,
            string parameterName,
            SqlDbType databaseType,
            int size,
            string? value)
        {
            command.Parameters.Add(
                parameterName,
                databaseType,
                size).Value =
                string.IsNullOrWhiteSpace(value)
                    ? DBNull.Value
                    : value.Trim();
        }

        private static string? GetNullableString(
            SqlDataReader reader,
            string columnName)
        {
            var ordinal =
                reader.GetOrdinal(columnName);

            return reader.IsDBNull(ordinal)
                ? null
                : reader.GetString(ordinal);
        }
    }
}

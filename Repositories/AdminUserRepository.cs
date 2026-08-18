using Microsoft.Data.SqlClient;
using MoneyMiners.Models;
using System.Data;
using MoneyMiners.ViewModels.Admin;


namespace MoneyMiners.Repositories
{
    public sealed class AdminUserRepository
        : IAdminUserRepository
    {
        private readonly string _connectionString;

        public AdminUserRepository(
            IConfiguration configuration)
        {
            _connectionString =
                configuration.GetConnectionString(
                    "DefaultConnection")
                ?? throw new InvalidOperationException(
                    "DefaultConnection was not found.");
        }

        public async Task<List<AdminUserListItemViewModel>> GetAllAsync(
    CancellationToken cancellationToken = default)
        {
            var adminUsers =
                new List<AdminUserListItemViewModel>();

            await using var connection =
                new SqlConnection(_connectionString);

            await using var command =
                new SqlCommand(
                    "dbo.usp_AdminUsers_GetAll",
                    connection);

            command.CommandType =
                CommandType.StoredProcedure;

            await connection.OpenAsync(
                cancellationToken);

            await using var reader =
                await command.ExecuteReaderAsync(
                    cancellationToken);

            while (await reader.ReadAsync(
                       cancellationToken))
            {
                adminUsers.Add(
                    new AdminUserListItemViewModel
                    {
                        AdminUserID =
                            reader.GetInt64(
                                reader.GetOrdinal(
                                    "AdminUserID")),

                        Username =
                            reader.GetString(
                                reader.GetOrdinal(
                                    "Username")),

                        Email =
                            reader.GetString(
                                reader.GetOrdinal(
                                    "Email")),

                        Role =
                            reader.GetString(
                                reader.GetOrdinal(
                                    "Role")),

                        IsActive =
                            reader.GetBoolean(
                                reader.GetOrdinal(
                                    "IsActive")),

                        FailedLoginCount =
                            reader.GetInt32(
                                reader.GetOrdinal(
                                    "FailedLoginCount")),

                        LockoutEndUtc =
                            GetNullableDateTime(
                                reader,
                                "LockoutEndUtc"),

                        LastLoginAtUtc =
                            GetNullableDateTime(
                                reader,
                                "LastLoginAtUtc"),

                        CreatedAtUtc =
                            reader.GetDateTime(
                                reader.GetOrdinal(
                                    "CreatedAtUtc")),

                        UpdatedAtUtc =
                            GetNullableDateTime(
                                reader,
                                "UpdatedAtUtc"),

                        RowVersion =
                            reader.GetFieldValue<byte[]>(
                                reader.GetOrdinal(
                                    "RowVersion"))
                    });
            }

            return adminUsers;
        }

        public async Task<bool> HasAnyAsync(
            CancellationToken cancellationToken = default)
        {
            await using var connection =
                new SqlConnection(_connectionString);

            await using var command =
                new SqlCommand(
                    "dbo.usp_AdminUsers_HasAny",
                    connection);

            command.CommandType =
                CommandType.StoredProcedure;

            await connection.OpenAsync(
                cancellationToken);

            var result =
                await command.ExecuteScalarAsync(
                    cancellationToken);

            return result is not null
                && result != DBNull.Value
                && Convert.ToBoolean(result);
        }

        public async Task<AdminUser?> GetByLoginAsync(
            string loginIdentifier,
            CancellationToken cancellationToken = default)
        {
            await using var connection =
                new SqlConnection(_connectionString);

            await using var command =
                new SqlCommand(
                    "dbo.usp_AdminUsers_GetByLogin",
                    connection);

            command.CommandType =
                CommandType.StoredProcedure;

            command.Parameters.Add(
                new SqlParameter(
                    "@LoginIdentifier",
                    SqlDbType.NVarChar,
                    256)
                {
                    Value = loginIdentifier
                });

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

            return new AdminUser
            {
                AdminUserID =
                    reader.GetInt64(
                        reader.GetOrdinal(
                            "AdminUserID")),

                Username =
                    reader.GetString(
                        reader.GetOrdinal(
                            "Username")),

                Email =
                    reader.GetString(
                        reader.GetOrdinal(
                            "Email")),

                PasswordHash =
                    reader.GetString(
                        reader.GetOrdinal(
                            "PasswordHash")),

                Role =
                    reader.GetString(
                        reader.GetOrdinal(
                            "Role")),

                PhoneNumber =
                    reader.IsDBNull(
                         reader.GetOrdinal(
                            "PhoneNumber"))
                         ? null
                        : reader.GetString(
                        reader.GetOrdinal(
                             "PhoneNumber")),

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
                    GetNullableDateTime(
                        reader,
                        "LockoutEndUtc"),

                LastLoginAtUtc =
                    GetNullableDateTime(
                        reader,
                        "LastLoginAtUtc"),

                PasswordChangedAtUtc =
                    GetNullableDateTime(
                        reader,
                        "PasswordChangedAtUtc"),

                SecurityStamp =
                    reader.GetGuid(
                        reader.GetOrdinal(
                            "SecurityStamp")),

                CreatedAtUtc =
                    reader.GetDateTime(
                        reader.GetOrdinal(
                            "CreatedAtUtc")),

                UpdatedAtUtc =
                    GetNullableDateTime(
                        reader,
                        "UpdatedAtUtc"),

                RowVersion =
                    reader.GetFieldValue<byte[]>(
                        reader.GetOrdinal(
                            "RowVersion"))
            };
        }

        public async Task<AdminUser?> GetByPhoneAsync(
        string phoneNumber,
        CancellationToken cancellationToken = default)
        {
            await using var connection =
                new SqlConnection(_connectionString);

            await using var command =
                new SqlCommand(
                    "dbo.usp_AdminUsers_GetByPhone",
                    connection);

            command.CommandType =
                CommandType.StoredProcedure;

            command.Parameters.Add(
                new SqlParameter(
                    "@PhoneNumber",
                    SqlDbType.VarChar,
                    15)
                {
                    Value = phoneNumber.Trim()
                });

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

            return new AdminUser
            {
                AdminUserID =
                    reader.GetInt64(
                        reader.GetOrdinal(
                            "AdminUserID")),

                Username =
                    reader.GetString(
                        reader.GetOrdinal(
                            "Username")),

                Email =
                    reader.GetString(
                        reader.GetOrdinal(
                            "Email")),

                PasswordHash =
                    reader.GetString(
                        reader.GetOrdinal(
                            "PasswordHash")),

                Role =
                    reader.GetString(
                        reader.GetOrdinal(
                            "Role")),

                PhoneNumber =
                    reader.IsDBNull(
                        reader.GetOrdinal(
                            "PhoneNumber"))
                        ? null
                        : reader.GetString(
                            reader.GetOrdinal(
                                "PhoneNumber")),

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
                    GetNullableDateTime(
                        reader,
                        "LockoutEndUtc"),

                LastLoginAtUtc =
                    GetNullableDateTime(
                        reader,
                        "LastLoginAtUtc"),

                PasswordChangedAtUtc =
                    GetNullableDateTime(
                        reader,
                        "PasswordChangedAtUtc"),

                SecurityStamp =
                    reader.GetGuid(
                        reader.GetOrdinal(
                            "SecurityStamp")),

                CreatedAtUtc =
                    reader.GetDateTime(
                        reader.GetOrdinal(
                            "CreatedAtUtc")),

                UpdatedAtUtc =
                    GetNullableDateTime(
                        reader,
                        "UpdatedAtUtc"),

                RowVersion =
                    reader.GetFieldValue<byte[]>(
                        reader.GetOrdinal(
                            "RowVersion"))
            };
        }


        public async Task<AdminUser?> GetByIdAsync(
        long adminUserId,
        CancellationToken cancellationToken = default)
        {
            await using var connection =
                new SqlConnection(_connectionString);

            await using var command =
                new SqlCommand(
                    "dbo.usp_AdminUsers_GetById",
                    connection);

            command.CommandType =
                CommandType.StoredProcedure;

            command.Parameters.Add(
                new SqlParameter(
                    "@AdminUserID",
                    SqlDbType.BigInt)
                {
                    Value = adminUserId
                });

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

            return new AdminUser
            {
                AdminUserID =
                    reader.GetInt64(
                        reader.GetOrdinal(
                            "AdminUserID")),

                Username =
                    reader.GetString(
                        reader.GetOrdinal(
                            "Username")),

                Email =
                    reader.GetString(
                        reader.GetOrdinal(
                            "Email")),

                PasswordHash =
                    reader.GetString(
                        reader.GetOrdinal(
                            "PasswordHash")),

                Role =
                    reader.GetString(
                        reader.GetOrdinal(
                            "Role")),

                PhoneNumber =
                    reader.IsDBNull(
                        reader.GetOrdinal(
                            "PhoneNumber"))
                        ? null
                        : reader.GetString(
                            reader.GetOrdinal(
                                "PhoneNumber")),

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
                    GetNullableDateTime(
                        reader,
                        "LockoutEndUtc"),

                LastLoginAtUtc =
                    GetNullableDateTime(
                        reader,
                        "LastLoginAtUtc"),

                PasswordChangedAtUtc =
                    GetNullableDateTime(
                        reader,
                        "PasswordChangedAtUtc"),

                SecurityStamp =
                    reader.GetGuid(
                        reader.GetOrdinal(
                            "SecurityStamp")),

                CreatedAtUtc =
                    reader.GetDateTime(
                        reader.GetOrdinal(
                            "CreatedAtUtc")),

                UpdatedAtUtc =
                    GetNullableDateTime(
                        reader,
                        "UpdatedAtUtc"),

                RowVersion =
                    reader.GetFieldValue<byte[]>(
                        reader.GetOrdinal(
                            "RowVersion"))
            };
        }

        public async Task RecordLoginAttemptAsync(
            long adminUserId,
            bool isSuccessful,
            int maxFailedAttempts = 5,
            int lockoutMinutes = 15,
            CancellationToken cancellationToken = default)
        {
            await using var connection =
                new SqlConnection(_connectionString);

            await using var command =
                new SqlCommand(
                    "dbo.usp_AdminUsers_RecordLoginAttempt",
                    connection);

            command.CommandType =
                CommandType.StoredProcedure;

            command.Parameters.Add(
                new SqlParameter(
                    "@AdminUserID",
                    SqlDbType.BigInt)
                {
                    Value = adminUserId
                });

            command.Parameters.Add(
                new SqlParameter(
                    "@IsSuccessful",
                    SqlDbType.Bit)
                {
                    Value = isSuccessful
                });

            command.Parameters.Add(
                new SqlParameter(
                    "@MaxFailedAttempts",
                    SqlDbType.Int)
                {
                    Value = maxFailedAttempts
                });

            command.Parameters.Add(
                new SqlParameter(
                    "@LockoutMinutes",
                    SqlDbType.Int)
                {
                    Value = lockoutMinutes
                });

            await connection.OpenAsync(
                cancellationToken);

            await command.ExecuteNonQueryAsync(
                cancellationToken);
        }

        public async Task<long> CreateFirstAsync(
        string username,
        string email,
        string passwordHash,
        CancellationToken cancellationToken = default)
        {
            await using var connection =
                new SqlConnection(_connectionString);

            await using var command =
                new SqlCommand(
                    "dbo.usp_AdminUsers_CreateFirst",
                    connection);

            command.CommandType =
                CommandType.StoredProcedure;

            command.Parameters.Add(
                new SqlParameter(
                    "@Username",
                    SqlDbType.NVarChar,
                    50)
                {
                    Value = username
                });

            command.Parameters.Add(
                new SqlParameter(
                    "@Email",
                    SqlDbType.NVarChar,
                    256)
                {
                    Value = email
                });

            command.Parameters.Add(
                new SqlParameter(
                    "@PasswordHash",
                    SqlDbType.NVarChar,
                    500)
                {
                    Value = passwordHash
                });

            await connection.OpenAsync(
                cancellationToken);

            var result =
                await command.ExecuteScalarAsync(
                    cancellationToken);

            if (result is null ||
                result == DBNull.Value)
            {
                throw new InvalidOperationException(
                    "Initial admin account could not be created.");
            }

            return Convert.ToInt64(result);
        }

        public async Task<long> CreateAsync(
        string username,
        string email,
        string phoneNumber,
        string passwordHash,
        string role = "Admin",
        CancellationToken cancellationToken = default)
        {
            await using var connection =
                new SqlConnection(_connectionString);

            await using var command =
                new SqlCommand(
                    "dbo.usp_AdminUsers_Create",
                    connection);

            command.CommandType =
                CommandType.StoredProcedure;

            command.Parameters.Add(
                new SqlParameter(
                    "@Username",
                    SqlDbType.NVarChar,
                    50)
                {
                    Value = username
                });

            command.Parameters.Add(
                new SqlParameter(
                    "@Email",
                    SqlDbType.NVarChar,
                    256)
                {
                    Value = email
                });

            command.Parameters.Add(
                new SqlParameter(
                    "@PhoneNumber",
                    SqlDbType.VarChar,
                    15)
                {
                    Value = phoneNumber
                });

            command.Parameters.Add(
                new SqlParameter(
                    "@PasswordHash",
                    SqlDbType.NVarChar,
                    500)
                {
                    Value = passwordHash
                });

            command.Parameters.Add(
                new SqlParameter(
                    "@Role",
                    SqlDbType.NVarChar,
                    30)
                {
                    Value = role
                });

            await connection.OpenAsync(
                cancellationToken);

            var result =
                await command.ExecuteScalarAsync(
                    cancellationToken);

            if (result is null ||
                result == DBNull.Value)
            {
                throw new InvalidOperationException(
                    "Admin user could not be created.");
            }

            return Convert.ToInt64(result);
        }


        public async Task CompletePasswordResetAsync(
        long challengeId,
        long adminUserId,
        string phoneNumber,
        string passwordHash,
        CancellationToken cancellationToken = default)
        {
            if (challengeId <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(challengeId));
            }

            if (adminUserId <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(adminUserId));
            }

            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                throw new ArgumentException(
                    "Mobile number is required.",
                    nameof(phoneNumber));
            }

            if (string.IsNullOrWhiteSpace(passwordHash))
            {
                throw new ArgumentException(
                    "Password hash is required.",
                    nameof(passwordHash));
            }

            await using var connection =
                new SqlConnection(_connectionString);

            await using var command =
                new SqlCommand(
                    "dbo.usp_AdminPasswordReset_Complete",
                    connection);

            command.CommandType =
                CommandType.StoredProcedure;

            command.Parameters.Add(
                new SqlParameter(
                    "@AdminPasswordResetOtpChallengeID",
                    SqlDbType.BigInt)
                {
                    Value = challengeId
                });

            command.Parameters.Add(
                new SqlParameter(
                    "@AdminUserID",
                    SqlDbType.BigInt)
                {
                    Value = adminUserId
                });

            command.Parameters.Add(
                new SqlParameter(
                    "@PhoneNumber",
                    SqlDbType.VarChar,
                    15)
                {
                    Value = phoneNumber.Trim()
                });

            command.Parameters.Add(
                new SqlParameter(
                    "@PasswordHash",
                    SqlDbType.NVarChar,
                    500)
                {
                    Value = passwordHash
                });

            await connection.OpenAsync(
                cancellationToken);

            await command.ExecuteNonQueryAsync(
                cancellationToken);
        }


        public async Task CompletePasswordResetEmailAsync(
            long challengeId,
            long adminUserId,
            string emailAddress,
            string passwordHash,
            CancellationToken cancellationToken = default)
        {
            if (challengeId <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(challengeId));
            }

            if (adminUserId <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(adminUserId));
            }

            if (string.IsNullOrWhiteSpace(emailAddress))
            {
                throw new ArgumentException(
                    "Email address is required.",
                    nameof(emailAddress));
            }

            if (string.IsNullOrWhiteSpace(passwordHash))
            {
                throw new ArgumentException(
                    "Password hash is required.",
                    nameof(passwordHash));
            }

            var normalizedEmailAddress =
                emailAddress
                    .Trim()
                    .ToLowerInvariant();

            await using var connection =
                new SqlConnection(_connectionString);

            await using var command =
                new SqlCommand(
                    "dbo.usp_AdminPasswordResetEmail_Complete",
                    connection);

            command.CommandType =
                CommandType.StoredProcedure;

            command.Parameters.Add(
                new SqlParameter(
                    "@AdminPasswordResetEmailOtpChallengeID",
                    SqlDbType.BigInt)
                {
                    Value = challengeId
                });

            command.Parameters.Add(
                new SqlParameter(
                    "@AdminUserID",
                    SqlDbType.BigInt)
                {
                    Value = adminUserId
                });

            command.Parameters.Add(
                new SqlParameter(
                    "@EmailAddress",
                    SqlDbType.NVarChar,
                    256)
                {
                    Value = normalizedEmailAddress
                });

            command.Parameters.Add(
                new SqlParameter(
                    "@PasswordHash",
                    SqlDbType.NVarChar,
                    500)
                {
                    Value = passwordHash
                });

            await connection.OpenAsync(
                cancellationToken);

            await command.ExecuteNonQueryAsync(
                cancellationToken);
        }

        private static DateTime? GetNullableDateTime(
            SqlDataReader reader,
            string columnName)
        {
            var ordinal =
                reader.GetOrdinal(columnName);

            return reader.IsDBNull(ordinal)
                ? null
                : reader.GetDateTime(ordinal);
        }
    }
}
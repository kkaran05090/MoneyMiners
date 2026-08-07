using Microsoft.Data.SqlClient;
using MoneyMiners.ViewModels.Admin;
using System.Data;

namespace MoneyMiners.Repositories
{
    public sealed class InvestorRepository
        : IInvestorRepository
    {
        private readonly string _connectionString;

        public InvestorRepository(
            IConfiguration configuration)
        {
            _connectionString =
                configuration.GetConnectionString(
                    "DefaultConnection")
                ?? throw new InvalidOperationException(
                    "DefaultConnection was not found.");
        }

        public async Task<InvestorsPageViewModel> GetAllAsync(
            string? status,
            string? search,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            pageNumber =
                Math.Max(pageNumber, 1);

            pageSize =
                Math.Clamp(pageSize, 1, 100);

            status =
                string.IsNullOrWhiteSpace(status)
                    ? null
                    : status.Trim();

            search =
                string.IsNullOrWhiteSpace(search)
                    ? null
                    : search.Trim();

            var pageModel =
                new InvestorsPageViewModel
                {
                    Status = status,
                    Search = search,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };

            await using var connection =
                new SqlConnection(
                    _connectionString);

            await using var command =
                new SqlCommand(
                    "dbo.usp_InvestorAccounts_GetManagementPage",
                    connection);

            command.CommandType =
                CommandType.StoredProcedure;

            command.Parameters.Add(
                "@Status",
                SqlDbType.NVarChar,
                20).Value =
                status == null
                    ? DBNull.Value
                    : status;

            command.Parameters.Add(
                "@Search",
                SqlDbType.NVarChar,
                200).Value =
                search == null
                    ? DBNull.Value
                    : search;

            command.Parameters.Add(
                "@PageNumber",
                SqlDbType.Int).Value =
                pageNumber;

            command.Parameters.Add(
                "@PageSize",
                SqlDbType.Int).Value =
                pageSize;

            await connection.OpenAsync(
                cancellationToken);

            await using var reader =
                await command.ExecuteReaderAsync(
                    cancellationToken);

            /*
             * First result set:
             * Investor records
             */
            while (await reader.ReadAsync(
                       cancellationToken))
            {
                var investor =
                    new InvestorListItemViewModel
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

                        FirstName =
                            reader.GetString(
                                reader.GetOrdinal(
                                    "FirstName")),

                        LastName =
                            GetNullableString(
                                reader,
                                "LastName"),

                        FatherName =
                            GetNullableString(
                                reader,
                                "FatherName"),

                        PhoneNumber =
                            reader.GetString(
                                reader.GetOrdinal(
                                    "PhoneNumber")),

                        Email =
                            GetNullableString(
                                reader,
                                "Email"),

                        City =
                            GetNullableString(
                                reader,
                                "City"),

                        State =
                            GetNullableString(
                                reader,
                                "State"),

                        Country =
                            reader.GetString(
                                reader.GetOrdinal(
                                    "Country")),

                        AadhaarLast4 =
                            reader.GetString(
                                reader.GetOrdinal(
                                    "AadhaarLast4"))
                                .Trim(),

                        PANLast4 =
                            GetNullableString(
                                reader,
                                "PANLast4")?
                                .Trim(),

                        IsMobileVerified =
                            reader.GetBoolean(
                                reader.GetOrdinal(
                                    "IsMobileVerified")),

                        IsActive =
                            reader.GetBoolean(
                                reader.GetOrdinal(
                                    "IsActive")),

                        ActivePlansCount =
                            reader.GetInt32(
                                reader.GetOrdinal(
                                    "ActivePlansCount")),

                        TotalActiveInvestment =
                            reader.GetDecimal(
                                reader.GetOrdinal(
                                    "TotalActiveInvestment")),

                        TotalInvestmentCount =
                            reader.GetInt32(
                                reader.GetOrdinal(
                                    "TotalInvestmentCount")),

                        LastLoginAtUtc =
                            GetNullableDateTime(
                                reader,
                                "LastLoginAtUtc"),

                        CreatedAtUtc =
                            reader.GetDateTime(
                                reader.GetOrdinal(
                                    "CreatedAtUtc"))
                    };

                pageModel.Investors.Add(
                    investor);
            }

            /*
             * Second result set:
             * Total number of matching investors
             */
            if (await reader.NextResultAsync(
                    cancellationToken) &&
                await reader.ReadAsync(
                    cancellationToken))
            {
                pageModel.TotalRecords =
                    reader.GetInt32(
                        reader.GetOrdinal(
                            "TotalRecords"));
            }

            return pageModel;
        }

        private static string? GetNullableString(
            SqlDataReader reader,
            string columnName)
        {
            var ordinal =
                reader.GetOrdinal(
                    columnName);

            return reader.IsDBNull(ordinal)
                ? null
                : reader.GetString(ordinal);
        }

        private static DateTime? GetNullableDateTime(
            SqlDataReader reader,
            string columnName)
        {
            var ordinal =
                reader.GetOrdinal(
                    columnName);

            return reader.IsDBNull(ordinal)
                ? null
                : reader.GetDateTime(ordinal);
        }
    }
}
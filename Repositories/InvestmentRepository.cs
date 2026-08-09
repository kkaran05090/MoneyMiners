using Microsoft.Data.SqlClient;
using MoneyMiners.Models;
using System.Data;

namespace MoneyMiners.Repositories
{
    public sealed class InvestmentRepository
        : IInvestmentRepository
    {
        private readonly string _connectionString;

        public InvestmentRepository(
            IConfiguration configuration)
        {
            _connectionString =
                configuration.GetConnectionString(
                    "DefaultConnection")
                ?? throw new InvalidOperationException(
                    "DefaultConnection was not found.");
        }

        public async Task<CreateInvestmentResult> CreateAsync(
            CreateInvestmentCommand investment,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(investment);

            await using var connection =
                new SqlConnection(_connectionString);

            await using var command =
                new SqlCommand(
                    "dbo.usp_Investments_Create",
                    connection);

            command.CommandType =
                CommandType.StoredProcedure;

            command.Parameters.Add(
                "@InvestorAccountID",
                SqlDbType.BigInt).Value =
                investment.InvestorAccountID;

            command.Parameters.Add(
                "@PlanName",
                SqlDbType.VarChar,
                20).Value =
                investment.PlanName.Trim();

            var amountParameter =
                command.Parameters.Add(
                    "@InvestedAmount",
                    SqlDbType.Decimal);

            amountParameter.Precision = 18;
            amountParameter.Scale = 2;
            amountParameter.Value =
                investment.InvestedAmount;

            command.Parameters.Add(
                "@StartDate",
                SqlDbType.Date).Value =
                investment.StartDate.Date;

            command.Parameters.Add(
                "@EndDate",
                SqlDbType.Date).Value =
                investment.EndDate.Date;

            command.Parameters.Add(
                "@DurationMonths",
                SqlDbType.SmallInt).Value =
                investment.DurationMonths;

            command.Parameters.Add(
                "@PaymentReference",
                SqlDbType.NVarChar,
                100).Value =
                string.IsNullOrWhiteSpace(
                    investment.PaymentReference)
                    ? DBNull.Value
                    : investment.PaymentReference.Trim();

            command.Parameters.Add(
                "@Remarks",
                SqlDbType.NVarChar,
                500).Value =
                string.IsNullOrWhiteSpace(
                    investment.Remarks)
                    ? DBNull.Value
                    : investment.Remarks.Trim();

            command.Parameters.Add(
                "@CreatedByAdminUserID",
                SqlDbType.Int).Value =
                investment.CreatedByAdminUserID.HasValue
                    ? investment.CreatedByAdminUserID.Value
                    : DBNull.Value;

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
                    "Investment creation did not return a result.");
            }

            return new CreateInvestmentResult
            {
                InvestmentID =
                    reader.GetInt64(
                        reader.GetOrdinal(
                            "InvestmentID")),

                InvestmentCode =
                    reader.GetString(
                        reader.GetOrdinal(
                            "InvestmentCode")),

                InvestorAccountID =
                    reader.GetInt64(
                        reader.GetOrdinal(
                            "InvestorAccountID")),

                PlanName =
                    reader.GetString(
                        reader.GetOrdinal(
                            "PlanName")),

                InvestedAmount =
                    reader.GetDecimal(
                        reader.GetOrdinal(
                            "InvestedAmount")),

                StartDate =
                    reader.GetDateTime(
                        reader.GetOrdinal(
                            "StartDate")),

                EndDate =
                    reader.GetDateTime(
                        reader.GetOrdinal(
                            "EndDate")),

                DurationMonths =
                    reader.GetInt16(
                        reader.GetOrdinal(
                            "DurationMonths")),

                Status =
                    reader.GetString(
                        reader.GetOrdinal(
                            "Status")),

                CreatedAtUtc =
                    reader.GetDateTime(
                        reader.GetOrdinal(
                            "CreatedAtUtc"))
            };
        }

        public async Task<List<InvestorActiveInvestment>>
            GetActiveByInvestorAccountIdAsync(
                long investorAccountId,
                CancellationToken cancellationToken = default)
        {
            if (investorAccountId <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(investorAccountId));
            }

            var investments =
                new List<InvestorActiveInvestment>();

            await using var connection =
                new SqlConnection(_connectionString);

            await using var command =
                new SqlCommand(
                    "dbo.usp_Investments_GetActiveByInvestorAccountID",
                    connection);

            command.CommandType =
                CommandType.StoredProcedure;

            command.Parameters.Add(
                "@InvestorAccountID",
                SqlDbType.BigInt).Value =
                investorAccountId;

            await connection.OpenAsync(
                cancellationToken);

            await using var reader =
                await command.ExecuteReaderAsync(
                    cancellationToken);

            var investmentIdOrdinal =
                reader.GetOrdinal("InvestmentID");

            var investmentCodeOrdinal =
                reader.GetOrdinal("InvestmentCode");

            var investorAccountIdOrdinal =
                reader.GetOrdinal("InvestorAccountID");

            var planNameOrdinal =
                reader.GetOrdinal("PlanName");

            var investedAmountOrdinal =
                reader.GetOrdinal("InvestedAmount");

            var startDateOrdinal =
                reader.GetOrdinal("StartDate");

            var endDateOrdinal =
                reader.GetOrdinal("EndDate");

            var durationMonthsOrdinal =
                reader.GetOrdinal("DurationMonths");

            var statusOrdinal =
                reader.GetOrdinal("Status");

            var paymentReferenceOrdinal =
                reader.GetOrdinal("PaymentReference");

            var remarksOrdinal =
                reader.GetOrdinal("Remarks");

            var createdAtUtcOrdinal =
                reader.GetOrdinal("CreatedAtUtc");

            while (await reader.ReadAsync(
                       cancellationToken))
            {
                investments.Add(
                    new InvestorActiveInvestment
                    {
                        InvestmentID =
                            reader.GetInt64(
                                investmentIdOrdinal),

                        InvestmentCode =
                            reader.GetString(
                                investmentCodeOrdinal),

                        InvestorAccountID =
                            reader.GetInt64(
                                investorAccountIdOrdinal),

                        PlanName =
                            reader.GetString(
                                planNameOrdinal),

                        InvestedAmount =
                            reader.GetDecimal(
                                investedAmountOrdinal),

                        StartDate =
                            reader.GetDateTime(
                                startDateOrdinal),

                        EndDate =
                            reader.GetDateTime(
                                endDateOrdinal),

                        DurationMonths =
                            reader.GetInt16(
                                durationMonthsOrdinal),

                        Status =
                            reader.GetString(
                                statusOrdinal),

                        PaymentReference =
                            reader.IsDBNull(
                                paymentReferenceOrdinal)
                                ? null
                                : reader.GetString(
                                    paymentReferenceOrdinal),

                        Remarks =
                            reader.IsDBNull(
                                remarksOrdinal)
                                ? null
                                : reader.GetString(
                                    remarksOrdinal),

                        CreatedAtUtc =
                            reader.GetDateTime(
                                createdAtUtcOrdinal)
                    });
            }

            return investments;
        }

        public async Task<List<InvestorInvestmentHistoryItem>>
            GetHistoryByInvestorAccountIdAsync(
                long investorAccountId,
                CancellationToken cancellationToken = default)
        {
            if (investorAccountId <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(investorAccountId));
            }

            var investments =
                new List<InvestorInvestmentHistoryItem>();

            await using var connection =
                new SqlConnection(_connectionString);

            await using var command =
                new SqlCommand(
                    "dbo.usp_Investments_GetHistoryByInvestorAccountID",
                    connection);

            command.CommandType =
                CommandType.StoredProcedure;

            command.Parameters.Add(
                "@InvestorAccountID",
                SqlDbType.BigInt).Value =
                investorAccountId;

            await connection.OpenAsync(
                cancellationToken);

            await using var reader =
                await command.ExecuteReaderAsync(
                    cancellationToken);

            var investmentIdOrdinal =
                reader.GetOrdinal("InvestmentID");

            var investmentCodeOrdinal =
                reader.GetOrdinal("InvestmentCode");

            var investorAccountIdOrdinal =
                reader.GetOrdinal("InvestorAccountID");

            var planNameOrdinal =
                reader.GetOrdinal("PlanName");

            var investedAmountOrdinal =
                reader.GetOrdinal("InvestedAmount");

            var startDateOrdinal =
                reader.GetOrdinal("StartDate");

            var endDateOrdinal =
                reader.GetOrdinal("EndDate");

            var durationMonthsOrdinal =
                reader.GetOrdinal("DurationMonths");

            var statusOrdinal =
                reader.GetOrdinal("Status");

            var paymentReferenceOrdinal =
                reader.GetOrdinal("PaymentReference");

            var remarksOrdinal =
                reader.GetOrdinal("Remarks");

            var createdAtUtcOrdinal =
                reader.GetOrdinal("CreatedAtUtc");

            var updatedAtUtcOrdinal =
                reader.GetOrdinal("UpdatedAtUtc");

            var closedAtUtcOrdinal =
                reader.GetOrdinal("ClosedAtUtc");

            while (await reader.ReadAsync(
                       cancellationToken))
            {
                investments.Add(
                    new InvestorInvestmentHistoryItem
                    {
                        InvestmentID =
                            reader.GetInt64(
                                investmentIdOrdinal),

                        InvestmentCode =
                            reader.GetString(
                                investmentCodeOrdinal),

                        InvestorAccountID =
                            reader.GetInt64(
                                investorAccountIdOrdinal),

                        PlanName =
                            reader.GetString(
                                planNameOrdinal),

                        InvestedAmount =
                            reader.GetDecimal(
                                investedAmountOrdinal),

                        StartDate =
                            reader.GetDateTime(
                                startDateOrdinal),

                        EndDate =
                            reader.GetDateTime(
                                endDateOrdinal),

                        DurationMonths =
                            reader.GetInt16(
                                durationMonthsOrdinal),

                        Status =
                            reader.GetString(
                                statusOrdinal),

                        PaymentReference =
                            reader.IsDBNull(
                                paymentReferenceOrdinal)
                                ? null
                                : reader.GetString(
                                    paymentReferenceOrdinal),

                        Remarks =
                            reader.IsDBNull(
                                remarksOrdinal)
                                ? null
                                : reader.GetString(
                                    remarksOrdinal),

                        CreatedAtUtc =
                            reader.GetDateTime(
                                createdAtUtcOrdinal),

                        UpdatedAtUtc =
                            reader.IsDBNull(
                                updatedAtUtcOrdinal)
                                ? null
                                : reader.GetDateTime(
                                    updatedAtUtcOrdinal),

                        ClosedAtUtc =
                            reader.IsDBNull(
                                closedAtUtcOrdinal)
                                ? null
                                : reader.GetDateTime(
                                    closedAtUtcOrdinal)
                    });
            }

            return investments;
        }

        public async Task<List<AdminInvestorInvestmentItem>>
        GetByInvestorAccountIdAsync(
        long investorAccountId,
        CancellationToken cancellationToken = default)
        {
            if (investorAccountId <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(investorAccountId));
            }

            var investments =
                new List<AdminInvestorInvestmentItem>();

            await using var connection =
                new SqlConnection(_connectionString);

            await using var command =
                new SqlCommand(
                    "dbo.usp_Investments_GetByInvestorAccountID",
                    connection);

            command.CommandType =
                CommandType.StoredProcedure;

            command.Parameters.Add(
                "@InvestorAccountID",
                SqlDbType.BigInt).Value =
                investorAccountId;

            await connection.OpenAsync(
                cancellationToken);

            await using var reader =
                await command.ExecuteReaderAsync(
                    cancellationToken);

            var investmentIdOrdinal =
                reader.GetOrdinal("InvestmentID");

            var investmentCodeOrdinal =
                reader.GetOrdinal("InvestmentCode");

            var investorAccountIdOrdinal =
                reader.GetOrdinal("InvestorAccountID");

            var planNameOrdinal =
                reader.GetOrdinal("PlanName");

            var investedAmountOrdinal =
                reader.GetOrdinal("InvestedAmount");

            var startDateOrdinal =
                reader.GetOrdinal("StartDate");

            var endDateOrdinal =
                reader.GetOrdinal("EndDate");

            var durationMonthsOrdinal =
                reader.GetOrdinal("DurationMonths");

            var statusOrdinal =
                reader.GetOrdinal("Status");

            var paymentReferenceOrdinal =
                reader.GetOrdinal("PaymentReference");

            var remarksOrdinal =
                reader.GetOrdinal("Remarks");

            var createdByAdminUserIdOrdinal =
                reader.GetOrdinal("CreatedByAdminUserID");

            var createdAtUtcOrdinal =
                reader.GetOrdinal("CreatedAtUtc");

            var updatedAtUtcOrdinal =
                reader.GetOrdinal("UpdatedAtUtc");

            var closedAtUtcOrdinal =
                reader.GetOrdinal("ClosedAtUtc");

            while (await reader.ReadAsync(
                       cancellationToken))
            {
                investments.Add(
                    new AdminInvestorInvestmentItem
                    {
                        InvestmentID =
                            reader.GetInt64(
                                investmentIdOrdinal),

                        InvestmentCode =
                            reader.GetString(
                                investmentCodeOrdinal),

                        InvestorAccountID =
                            reader.GetInt64(
                                investorAccountIdOrdinal),

                        PlanName =
                            reader.GetString(
                                planNameOrdinal),

                        InvestedAmount =
                            reader.GetDecimal(
                                investedAmountOrdinal),

                        StartDate =
                            reader.GetDateTime(
                                startDateOrdinal),

                        EndDate =
                            reader.GetDateTime(
                                endDateOrdinal),

                        DurationMonths =
                            reader.GetInt16(
                                durationMonthsOrdinal),

                        Status =
                            reader.GetString(
                                statusOrdinal),

                        PaymentReference =
                            reader.IsDBNull(
                                paymentReferenceOrdinal)
                                ? null
                                : reader.GetString(
                                    paymentReferenceOrdinal),

                        Remarks =
                            reader.IsDBNull(
                                remarksOrdinal)
                                ? null
                                : reader.GetString(
                                    remarksOrdinal),

                        CreatedByAdminUserID =
                            reader.IsDBNull(
                                createdByAdminUserIdOrdinal)
                                ? null
                                : reader.GetInt32(
                                    createdByAdminUserIdOrdinal),

                        CreatedAtUtc =
                            reader.GetDateTime(
                                createdAtUtcOrdinal),

                        UpdatedAtUtc =
                            reader.IsDBNull(
                                updatedAtUtcOrdinal)
                                ? null
                                : reader.GetDateTime(
                                    updatedAtUtcOrdinal),

                        ClosedAtUtc =
                            reader.IsDBNull(
                                closedAtUtcOrdinal)
                                ? null
                                : reader.GetDateTime(
                                    closedAtUtcOrdinal)
                    });
            }

            return investments;
        }



        public async Task ChangeStatusAsync(
     ChangeInvestmentStatusCommand investment,
     CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(investment);

            if (investment.InvestmentID <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(investment.InvestmentID));
            }

            if (investment.InvestorAccountID <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(investment.InvestorAccountID));
            }

            if (string.IsNullOrWhiteSpace(investment.NewStatus))
            {
                throw new ArgumentException(
                    "Investment status is required.",
                    nameof(investment.NewStatus));
            }

            if (investment.ChangedByAdminUserID <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(investment.ChangedByAdminUserID));
            }

            await using var connection =
                new SqlConnection(_connectionString);

            await using var command =
                new SqlCommand(
                    "dbo.usp_Investments_ChangeStatus",
                    connection);

            command.CommandType =
                CommandType.StoredProcedure;

            command.Parameters.Add(
                "@InvestmentID",
                SqlDbType.BigInt).Value =
                investment.InvestmentID;

            command.Parameters.Add(
                "@InvestorAccountID",
                SqlDbType.BigInt).Value =
                investment.InvestorAccountID;

            command.Parameters.Add(
                "@NewStatus",
                SqlDbType.VarChar,
                20).Value =
                investment.NewStatus.Trim();

            command.Parameters.Add(
                "@Remarks",
                SqlDbType.NVarChar,
                500).Value =
                string.IsNullOrWhiteSpace(
                    investment.Remarks)
                    ? DBNull.Value
                    : investment.Remarks.Trim();

            command.Parameters.Add(
                "@ChangedByAdminUserID",
                SqlDbType.Int).Value =
                investment.ChangedByAdminUserID;

            await connection.OpenAsync(
                cancellationToken);

            await command.ExecuteNonQueryAsync(
                cancellationToken);
        }
    }
}
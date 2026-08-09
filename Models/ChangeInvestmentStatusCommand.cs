namespace MoneyMiners.Models
{
    public sealed class ChangeInvestmentStatusCommand
    {
        public long InvestmentID { get; set; }

        public long InvestorAccountID { get; set; }

        public string NewStatus { get; set; } = string.Empty;

        public string? Remarks { get; set; }

        public int ChangedByAdminUserID { get; set; }
    }
}
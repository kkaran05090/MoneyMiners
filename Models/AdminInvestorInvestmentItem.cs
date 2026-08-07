namespace MoneyMiners.Models
{
    public sealed class AdminInvestorInvestmentItem
    {
        public long InvestmentID { get; set; }

        public string InvestmentCode { get; set; } =
            string.Empty;

        public long InvestorAccountID { get; set; }

        public string PlanName { get; set; } =
            string.Empty;

        public decimal InvestedAmount { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public short DurationMonths { get; set; }

        public string Status { get; set; } =
            string.Empty;

        public string? PaymentReference { get; set; }

        public string? Remarks { get; set; }

        public int? CreatedByAdminUserID { get; set; }

        public DateTime CreatedAtUtc { get; set; }

        public DateTime? UpdatedAtUtc { get; set; }

        public DateTime? ClosedAtUtc { get; set; }
    }
}
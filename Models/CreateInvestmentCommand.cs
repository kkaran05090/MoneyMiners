namespace MoneyMiners.Models
{
    public sealed class CreateInvestmentCommand
    {
        public long InvestorAccountID { get; set; }

        public string PlanName { get; set; } =
            string.Empty;

        public decimal InvestedAmount { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public short DurationMonths { get; set; }

        public string? PaymentReference { get; set; }

        public string? Remarks { get; set; }

        public int? CreatedByAdminUserID { get; set; }
    }
}
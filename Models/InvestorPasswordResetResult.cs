namespace MoneyMiners.Models
{
    public sealed class InvestorPasswordResetResult
    {
        public long InvestorAccountID { get; set; }

        public int InvestorProfileID { get; set; }

        public string InvestorCode { get; set; } =
            string.Empty;

        public string PhoneNumber { get; set; } =
            string.Empty;

        public DateTime PasswordChangedAtUtc { get; set; }
    }
}
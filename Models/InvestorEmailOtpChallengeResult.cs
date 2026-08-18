namespace MoneyMiners.Models
{
    public sealed class InvestorEmailOtpChallengeResult
    {
        public long InvestorEmailOtpChallengeID { get; set; }

        public string EmailAddress { get; set; } =
            string.Empty;

        public string Purpose { get; set; } =
            string.Empty;

        public DateTime ExpiresAtUtc { get; set; }
    }
}
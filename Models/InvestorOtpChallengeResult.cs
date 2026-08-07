namespace MoneyMiners.Models
{
    public sealed class InvestorOtpChallengeResult
    {
        public long InvestorOtpChallengeID { get; set; }

        public string PhoneNumber { get; set; } =
            string.Empty;

        public string Purpose { get; set; } =
            string.Empty;

        public DateTime ExpiresAtUtc { get; set; }
    }
}

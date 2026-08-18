namespace MoneyMiners.Models
{
    public sealed class InvestorEmailOtpVerificationResult
    {
        public long InvestorEmailOtpChallengeID { get; set; }

        public string EmailAddress { get; set; } =
            string.Empty;

        public string Purpose { get; set; } =
            string.Empty;

        public bool IsVerified { get; set; }

        public DateTime? VerifiedAtUtc { get; set; }
    }
}
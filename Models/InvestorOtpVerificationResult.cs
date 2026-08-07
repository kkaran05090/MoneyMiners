namespace MoneyMiners.Models
{
    public sealed class InvestorOtpVerificationResult
    {
        public long InvestorOtpChallengeID { get; set; }

        public string PhoneNumber { get; set; } =
            string.Empty;

        public string Purpose { get; set; } =
            string.Empty;

        public bool IsVerified { get; set; }

        public DateTime? VerifiedAtUtc { get; set; }
    }
}

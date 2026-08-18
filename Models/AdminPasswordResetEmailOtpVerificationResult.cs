namespace MoneyMiners.Models
{
    public sealed class AdminPasswordResetEmailOtpVerificationResult
    {
        public long AdminPasswordResetEmailOtpChallengeID { get; set; }

        public long AdminUserID { get; set; }

        public string EmailAddress { get; set; } =
            string.Empty;

        public bool IsVerified { get; set; }

        public DateTime? VerifiedAtUtc { get; set; }
    }
}
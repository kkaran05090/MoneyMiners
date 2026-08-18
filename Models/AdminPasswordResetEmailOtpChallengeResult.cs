namespace MoneyMiners.Models
{
    public sealed class AdminPasswordResetEmailOtpChallengeResult
    {
        public long AdminPasswordResetEmailOtpChallengeID { get; set; }

        public long AdminUserID { get; set; }

        public string EmailAddress { get; set; } =
            string.Empty;

        public DateTime ExpiresAtUtc { get; set; }
    }
}
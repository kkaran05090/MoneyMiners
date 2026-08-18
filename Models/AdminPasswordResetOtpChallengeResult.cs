namespace MoneyMiners.Models
{
    public sealed class AdminPasswordResetOtpChallengeResult
    {
        public long AdminPasswordResetOtpChallengeID { get; set; }

        public long AdminUserID { get; set; }

        public string PhoneNumber { get; set; } = string.Empty;

        public DateTime ExpiresAtUtc { get; set; }
    }
}
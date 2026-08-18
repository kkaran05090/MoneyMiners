namespace MoneyMiners.Models
{
    public sealed class AdminMobileVerificationOtpChallengeResult
    {
        public long AdminMobileVerificationOtpChallengeID { get; set; }

        public long AdminUserID { get; set; }

        public string PhoneNumber { get; set; }
            = string.Empty;

        public DateTime ExpiresAtUtc { get; set; }
    }
}
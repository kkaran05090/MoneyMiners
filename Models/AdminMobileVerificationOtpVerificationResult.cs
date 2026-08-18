namespace MoneyMiners.Models
{
    public sealed class AdminMobileVerificationOtpVerificationResult
    {
        public long AdminMobileVerificationOtpChallengeID { get; set; }

        public long AdminUserID { get; set; }

        public string PhoneNumber { get; set; }
            = string.Empty;

        public bool IsVerified { get; set; }

        public DateTime? VerifiedAtUtc { get; set; }
    }
}
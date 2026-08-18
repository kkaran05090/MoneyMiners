namespace MoneyMiners.Models
{
    public sealed class InvestorPasswordResetCommand
    {
        // Future Mobile OTP flow
        public long? InvestorOtpChallengeID { get; set; }

        public string? PhoneNumber { get; set; }


        // Current Email OTP flow
        public long? InvestorEmailOtpChallengeID { get; set; }

        public string? EmailAddress { get; set; }


        public string PasswordHash { get; set; } =
            string.Empty;
    }
}
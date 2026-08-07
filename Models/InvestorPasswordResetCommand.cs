namespace MoneyMiners.Models
{
    public sealed class InvestorPasswordResetCommand
    {
        public long InvestorOtpChallengeID { get; set; }

        public string PhoneNumber { get; set; } =
            string.Empty;

        public string PasswordHash { get; set; } =
            string.Empty;
    }
}
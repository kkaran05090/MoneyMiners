namespace MoneyMiners.Models
{
    public sealed class InvestorLoginAttemptResult
    {
        public long InvestorAccountID { get; set; }

        public int FailedLoginCount { get; set; }

        public DateTime? LockoutEndUtc { get; set; }

        public DateTime? LastLoginAtUtc { get; set; }

        public bool IsLockedOut =>
            LockoutEndUtc.HasValue &&
            LockoutEndUtc.Value > DateTime.UtcNow;
    }
}
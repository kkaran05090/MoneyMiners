namespace MoneyMiners.Models
{
    public sealed class InvestorAccount
    {
        public long InvestorAccountID { get; set; }

        public int InvestorProfileID { get; set; }

        public string InvestorCode { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        public string? Email { get; set; }

        public string FirstName { get; set; } = string.Empty;

        public string? LastName { get; set; }

        public string PasswordHash { get; set; } = string.Empty;

        public bool IsMobileVerified { get; set; }

        public bool IsEmailVerified { get; set; }

        public bool IsActive { get; set; }

        public int FailedLoginCount { get; set; }

        public DateTime? LockoutEndUtc { get; set; }

        public DateTime? LastLoginAtUtc { get; set; }

        public DateTime? PasswordChangedAtUtc { get; set; }

        public Guid SecurityStamp { get; set; }

        public string DisplayName =>
            string.Join(
                " ",
                new[] { FirstName, LastName }
                    .Where(value =>
                        !string.IsNullOrWhiteSpace(value)));
    }
}
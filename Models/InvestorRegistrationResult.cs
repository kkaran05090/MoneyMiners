namespace MoneyMiners.Models
{
    public sealed class InvestorRegistrationResult
    {
        public int InvestorProfileID { get; set; }

        public long InvestorAccountID { get; set; }

        public string InvestorCode { get; set; } = string.Empty;

        public string FirstName { get; set; } = string.Empty;

        public string? LastName { get; set; }

        public string PhoneNumber { get; set; } = string.Empty;

        public string DisplayName =>
            string.Join(
                " ",
                new[] { FirstName, LastName }
                    .Where(value =>
                        !string.IsNullOrWhiteSpace(value)));
    }
}

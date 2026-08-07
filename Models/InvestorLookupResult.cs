namespace MoneyMiners.Models
{
    public sealed class InvestorLookupResult
    {
        public long InvestorAccountID { get; set; }

        public int InvestorProfileID { get; set; }

        public string InvestorCode { get; set; } =
            string.Empty;

        public string FirstName { get; set; } =
            string.Empty;

        public string? LastName { get; set; }

        public string PhoneNumber { get; set; } =
            string.Empty;

        public string? Email { get; set; }

        public string AadhaarLast4 { get; set; } =
            string.Empty;

        public bool IsActive { get; set; }

        public bool IsMobileVerified { get; set; }

        public string DisplayName =>
            string.IsNullOrWhiteSpace(LastName)
                ? FirstName
                : $"{FirstName} {LastName}";
    }
}
namespace MoneyMiners.ViewModels.Admin
{
    public sealed class InvestorListItemViewModel
    {
        public long InvestorAccountID { get; set; }

        public int InvestorProfileID { get; set; }

        public string InvestorCode { get; set; } =
            string.Empty;

        public string FirstName { get; set; } =
            string.Empty;

        public string? LastName { get; set; }

        public string? FatherName { get; set; }

        public string PhoneNumber { get; set; } =
            string.Empty;

        public string? Email { get; set; }

        public string? City { get; set; }

        public string? State { get; set; }

        public string Country { get; set; } =
            string.Empty;

        public string AadhaarLast4 { get; set; } =
            string.Empty;

        public string? PANLast4 { get; set; }

        public bool IsMobileVerified { get; set; }

        public bool IsActive { get; set; }

        public int ActivePlansCount { get; set; }

        public decimal TotalActiveInvestment { get; set; }

        public int TotalInvestmentCount { get; set; }

        public DateTime? LastLoginAtUtc { get; set; }

        public DateTime CreatedAtUtc { get; set; }

        public string InvestorName =>
            string.IsNullOrWhiteSpace(LastName)
                ? FirstName
                : $"{FirstName} {LastName}";

        public string Status =>
            IsActive
                ? "Active"
                : "Inactive";

        public string MaskedAadhaar =>
            string.IsNullOrWhiteSpace(AadhaarLast4)
                ? "Not available"
                : $"XXXX-XXXX-{AadhaarLast4}";

        public string MaskedPAN =>
            string.IsNullOrWhiteSpace(PANLast4)
                ? "Not provided"
                : $"******{PANLast4}";
    }
}
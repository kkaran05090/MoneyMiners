namespace MoneyMiners.Models
{
    public sealed class InvestorProfile
    {
        public int InvestorProfileID { get; set; }

        public string FirstName { get; set; } = string.Empty;

        public string? LastName { get; set; }

        public string? FatherName { get; set; }

        public string PhoneNumber { get; set; } = string.Empty;

        public string? Email { get; set; }

        public string? AddressLine1 { get; set; }

        public string? AddressLine2 { get; set; }

        public string? City { get; set; }

        public string? State { get; set; }

        public string Country { get; set; } = "India";

        public string? PostalCode { get; set; }

        public byte[] AadhaarCipherText { get; set; } =
            Array.Empty<byte>();

        public byte[] AadhaarHash { get; set; } =
            Array.Empty<byte>();

        public string AadhaarLast4 { get; set; } = string.Empty;

        public byte[]? PANCipherText { get; set; }

        public byte[]? PANHash { get; set; }

        public string? PANLast4 { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAtUtc { get; set; }

        public DateTime UpdatedAtUtc { get; set; }

        public string DisplayName =>
            string.Join(
                " ",
                new[] { FirstName, LastName }
                    .Where(value =>
                        !string.IsNullOrWhiteSpace(value)));

        public string MaskedAadhaar =>
            string.IsNullOrWhiteSpace(AadhaarLast4)
                ? "Not provided"
                : $"********{AadhaarLast4}";

        public string MaskedPAN =>
            string.IsNullOrWhiteSpace(PANLast4)
                ? "Not provided"
                : $"******{PANLast4}";
    }
}

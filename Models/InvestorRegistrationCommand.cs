namespace MoneyMiners.Models
{
    public sealed class InvestorRegistrationCommand
    {
        public long InvestorOtpChallengeID { get; set; }

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

        public string AadhaarLast4 { get; set; } =
            string.Empty;

        public byte[]? PANCipherText { get; set; }

        public byte[]? PANHash { get; set; }

        public string? PANLast4 { get; set; }

        public string PasswordHash { get; set; } =
            string.Empty;
    }
}
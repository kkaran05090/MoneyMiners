namespace MoneyMiners.Models
{
    public sealed class InvestorRegistrationCommand
    {
        // Existing / future Mobile OTP flow
        public long? InvestorOtpChallengeID { get; set; }

        // Current Email OTP flow
        public long? InvestorEmailOtpChallengeID { get; set; }


        public string FirstName { get; set; } =
            string.Empty;

        public string? LastName { get; set; }

        public string? FatherName { get; set; }


        // Mobile number profile/contact ke liye
        // abhi bhi registration form me rahega.
        public string PhoneNumber { get; set; } =
            string.Empty;

        public string? Email { get; set; }


        public string? AddressLine1 { get; set; }

        public string? AddressLine2 { get; set; }

        public string? City { get; set; }

        public string? State { get; set; }

        public string Country { get; set; } =
            "India";

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
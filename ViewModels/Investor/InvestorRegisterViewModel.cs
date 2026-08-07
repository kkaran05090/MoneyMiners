using System.ComponentModel.DataAnnotations;

namespace MoneyMiners.ViewModels.Investor
{
    public sealed class InvestorRegisterViewModel
    {
        [Range(
           1,
           long.MaxValue,
           ErrorMessage = "Mobile verification is required.")]
        public long InvestorOtpChallengeID { get; set; }


        [Required(ErrorMessage = "First name is required.")]
        [StringLength(60)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [StringLength(60)]
        [Display(Name = "Last Name")]
        public string? LastName { get; set; }

        [StringLength(100)]
        [Display(Name = "Father's Name")]
        public string? FatherName { get; set; }

        [Required(ErrorMessage = "Mobile number is required.")]
        [RegularExpression(
            @"^[0-9]{10}$",
            ErrorMessage = "Enter a valid 10-digit mobile number.")]
        [Display(Name = "Mobile Number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        [StringLength(256)]
        [Display(Name = "Email Address")]
        public string? Email { get; set; }

        [StringLength(200)]
        [Display(Name = "Address Line 1")]
        public string? AddressLine1 { get; set; }

        [StringLength(200)]
        [Display(Name = "Address Line 2")]
        public string? AddressLine2 { get; set; }

        [StringLength(100)]
        public string? City { get; set; }

        [StringLength(100)]
        public string? State { get; set; }

        [Required]
        [StringLength(100)]
        public string Country { get; set; } = "India";

        [RegularExpression(
            @"^[0-9]{6}$",
            ErrorMessage = "Enter a valid 6-digit PIN code.")]
        [Display(Name = "PIN Code")]
        public string? PostalCode { get; set; }

        [Required(ErrorMessage = "Aadhaar number is required.")]
        [RegularExpression(
            @"^[0-9]{12}$",
            ErrorMessage = "Enter a valid 12-digit Aadhaar number.")]
        [Display(Name = "Aadhaar Number")]
        public string AadhaarNumber { get; set; } = string.Empty;

        [RegularExpression(
            @"^[A-Za-z]{5}[0-9]{4}[A-Za-z]$",
            ErrorMessage = "Enter a valid PAN number.")]
        [Display(Name = "PAN Number")]
        public string? PANNumber { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(
            100,
            MinimumLength = 8,
            ErrorMessage = "Password must contain at least 8 characters.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirm your password.")]
        [DataType(DataType.Password)]
        [Compare(
            nameof(Password),
            ErrorMessage = "Password and confirmation password do not match.")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Display(Name = "I accept the Terms and Privacy Policy")]
        public bool AcceptTerms { get; set; }
    }
}

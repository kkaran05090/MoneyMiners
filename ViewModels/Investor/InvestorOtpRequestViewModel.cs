using System.ComponentModel.DataAnnotations;

namespace MoneyMiners.ViewModels.Investor
{
    public sealed class InvestorOtpRequestViewModel
    {
        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        // Future SMS OTP ke liye rakha hai.
        [Display(Name = "Mobile Number")]
        public string? PhoneNumber { get; set; }
    }
}
using System.ComponentModel.DataAnnotations;

namespace MoneyMiners.ViewModels.Investor
{
    public sealed class InvestorForgotPasswordViewModel
    {
        // Current Email OTP flow
        [Required(
            ErrorMessage = "Registered email address is required.")]
        [EmailAddress(
            ErrorMessage = "Enter a valid email address.")]
        [StringLength(256)]
        [Display(Name = "Registered Email Address")]
        public string Email { get; set; } =
            string.Empty;


        // Future Mobile OTP flow
        [Display(Name = "Registered Mobile Number")]
        public string? PhoneNumber { get; set; }
    }
}
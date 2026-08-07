using System.ComponentModel.DataAnnotations;

namespace MoneyMiners.ViewModels.Investor
{
    public sealed class InvestorForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Registered mobile number is required.")]
        [RegularExpression(
            @"^[0-9]{10}$",
            ErrorMessage = "Enter a valid 10-digit mobile number.")]
        [Display(Name = "Registered Mobile Number")]
        public string PhoneNumber { get; set; } = string.Empty;
    }
}
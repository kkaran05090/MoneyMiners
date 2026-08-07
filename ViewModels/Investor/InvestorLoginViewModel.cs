using System.ComponentModel.DataAnnotations;

namespace MoneyMiners.ViewModels.Investor
{
    public sealed class InvestorLoginViewModel
    {
        [Required(
            ErrorMessage = "Investor ID or mobile number is required.")]
        [StringLength(100)]
        [Display(Name = "Investor ID or Mobile Number")]
        public string LoginIdentifier { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember Me")]
        public bool RememberMe { get; set; }

        public string? ReturnUrl { get; set; }
    }
}
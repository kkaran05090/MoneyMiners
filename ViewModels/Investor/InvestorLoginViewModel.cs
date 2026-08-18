using System.ComponentModel.DataAnnotations;

namespace MoneyMiners.ViewModels.Investor
{
    public sealed class InvestorLoginViewModel
    {
        [Required(
            ErrorMessage = "Investor ID or email address is required.")]
        [StringLength(256)]
        [Display(Name = "Investor ID or Email Address")]
        public string LoginIdentifier { get; set; } =
            string.Empty;

        [Required(
            ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } =
            string.Empty;

        [Display(Name = "Remember Me")]
        public bool RememberMe { get; set; }

        public string? ReturnUrl { get; set; }
    }
}
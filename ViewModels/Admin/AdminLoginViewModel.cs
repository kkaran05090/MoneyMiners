using System.ComponentModel.DataAnnotations;

namespace MoneyMiners.ViewModels.Admin
{
    public sealed class AdminLoginViewModel
    {
        [Required(
            ErrorMessage = "Username or email is required.")]
        [Display(Name = "Username or Email")]
        [StringLength(256)]
        public string LoginIdentifier { get; set; }
            = string.Empty;

        [Required(
            ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        [StringLength(128)]
        public string Password { get; set; }
            = string.Empty;

        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; }

        public string? ReturnUrl { get; set; }
    }
}
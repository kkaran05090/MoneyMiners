using System.ComponentModel.DataAnnotations;

namespace MoneyMiners.ViewModels.Admin
{
    public sealed class AdminSetupViewModel
    {
        [Required(
            ErrorMessage = "Username is required.")]
        [StringLength(
            50,
            MinimumLength = 3,
            ErrorMessage =
                "Username must be between 3 and 50 characters.")]
        [RegularExpression(
            @"^[a-zA-Z0-9._-]+$",
            ErrorMessage =
                "Username can contain letters, numbers, dots, underscores and hyphens only.")]
        public string Username { get; set; }
            = string.Empty;

        [Required(
            ErrorMessage = "Email address is required.")]
        [EmailAddress(
            ErrorMessage = "Enter a valid email address.")]
        [StringLength(256)]
        public string Email { get; set; }
            = string.Empty;

        [Required(
            ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        [StringLength(
            128,
            MinimumLength = 12,
            ErrorMessage =
                "Password must contain at least 12 characters.")]
        public string Password { get; set; }
            = string.Empty;

        [Required(
            ErrorMessage = "Confirm password is required.")]
        [DataType(DataType.Password)]
        [Compare(
            nameof(Password),
            ErrorMessage =
                "Password and confirmation password do not match.")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; }
            = string.Empty;
    }
}
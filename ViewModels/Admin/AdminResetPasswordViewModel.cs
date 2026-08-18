using System.ComponentModel.DataAnnotations;

namespace MoneyMiners.ViewModels.Admin
{
    public sealed class AdminResetPasswordViewModel
    {
        // Current Email OTP flow
        [Range(
            1,
            long.MaxValue,
            ErrorMessage = "Invalid password reset request.")]
        public long ChallengeID { get; set; }

        [Range(
            1,
            long.MaxValue,
            ErrorMessage = "Invalid administrator.")]
        public long AdminUserID { get; set; }

        [Required(
            ErrorMessage = "Email address is required.")]
        [EmailAddress(
            ErrorMessage = "Enter a valid email address.")]
        [Display(
            Name = "Email Address")]
        public string EmailAddress { get; set; } =
            string.Empty;


        // Future Mobile OTP flow
        public string? PhoneNumber { get; set; }


        [Required(
            ErrorMessage = "New password is required.")]
        [MinLength(
            8,
            ErrorMessage = "Password must contain at least 8 characters.")]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string NewPassword { get; set; } =
            string.Empty;

        [Required(
            ErrorMessage = "Please confirm the new password.")]
        [DataType(DataType.Password)]
        [Compare(
            nameof(NewPassword),
            ErrorMessage = "Passwords do not match.")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; } =
            string.Empty;
    }
}
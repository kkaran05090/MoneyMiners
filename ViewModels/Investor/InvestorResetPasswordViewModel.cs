using System.ComponentModel.DataAnnotations;

namespace MoneyMiners.ViewModels.Investor
{
    public sealed class InvestorResetPasswordViewModel
    {
        // Future Mobile OTP flow
        public long? InvestorOtpChallengeID { get; set; }

        public string? PhoneNumber { get; set; }


        // Current Email OTP flow
        [Range(
            1,
            long.MaxValue,
            ErrorMessage = "Invalid password reset request.")]
        public long InvestorEmailOtpChallengeID { get; set; }

        [Required(
            ErrorMessage = "Email address is required.")]
        [EmailAddress(
            ErrorMessage = "Enter a valid email address.")]
        public string EmailAddress { get; set; } =
            string.Empty;


        [Required(
            ErrorMessage = "New password is required.")]
        [StringLength(
            100,
            MinimumLength = 8,
            ErrorMessage = "Password must contain at least 8 characters.")]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string NewPassword { get; set; } =
            string.Empty;


        [Required(
            ErrorMessage = "Confirm password is required.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare(
            nameof(NewPassword),
            ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; } =
            string.Empty;
    }
}
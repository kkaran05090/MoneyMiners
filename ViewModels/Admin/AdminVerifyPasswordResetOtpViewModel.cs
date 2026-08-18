using System.ComponentModel.DataAnnotations;

namespace MoneyMiners.ViewModels.Admin
{
    public sealed class AdminVerifyPasswordResetOtpViewModel
    {
        // Current Email OTP flow
        [Range(
            1,
            long.MaxValue,
            ErrorMessage = "Invalid OTP request.")]
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
            ErrorMessage = "OTP is required.")]
        [RegularExpression(
            @"^[0-9]{6}$",
            ErrorMessage = "Enter a valid 6-digit OTP.")]
        [Display(
            Name = "OTP")]
        public string OtpCode { get; set; } =
            string.Empty;
    }
}
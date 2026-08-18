using System.ComponentModel.DataAnnotations;

namespace MoneyMiners.ViewModels.Investor
{
    public sealed class InvestorEmailOtpVerifyViewModel
    {
        [Range(
            1,
            long.MaxValue,
            ErrorMessage = "Invalid OTP request.")]
        public long InvestorEmailOtpChallengeID { get; set; }

        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress(
            ErrorMessage = "Enter a valid email address.")]
        [Display(Name = "Email Address")]
        public string EmailAddress { get; set; } =
            string.Empty;

        [Required(ErrorMessage = "OTP is required.")]
        [RegularExpression(
            @"^[0-9]{6}$",
            ErrorMessage = "Enter the valid 6-digit OTP.")]
        [Display(Name = "Enter OTP")]
        public string OtpCode { get; set; } =
            string.Empty;
    }
}
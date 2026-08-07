using System.ComponentModel.DataAnnotations;

namespace MoneyMiners.ViewModels.Investor
{
    public sealed class InvestorOtpVerifyViewModel
    {
        [Range(
            1,
            long.MaxValue,
            ErrorMessage = "Invalid OTP request.")]
        public long InvestorOtpChallengeID { get; set; }

        [Required(ErrorMessage = "Mobile number is required.")]
        [RegularExpression(
            @"^[0-9]{10}$",
            ErrorMessage = "Enter a valid 10-digit mobile number.")]
        [Display(Name = "Mobile Number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "OTP is required.")]
        [RegularExpression(
            @"^[0-9]{6}$",
            ErrorMessage = "Enter the valid 6-digit OTP.")]
        [Display(Name = "Enter OTP")]
        public string OtpCode { get; set; } = string.Empty;
    }
}
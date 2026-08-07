using System.ComponentModel.DataAnnotations;

namespace MoneyMiners.ViewModels.Investor
{
    public sealed class InvestorForgotPasswordOtpViewModel
    {
        [Required]
        public long InvestorOtpChallengeID { get; set; }

        [Required]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "OTP is required.")]
        [RegularExpression(
            @"^[0-9]{6}$",
            ErrorMessage = "Enter a valid 6-digit OTP.")]
        [Display(Name = "Enter OTP")]
        public string OtpCode { get; set; } = string.Empty;
    }
}
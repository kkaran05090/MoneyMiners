using System.ComponentModel.DataAnnotations;

namespace MoneyMiners.ViewModels.Admin
{
    public sealed class AdminMobileVerificationOtpViewModel
    {
        [Required]
        public long ChallengeID { get; set; }


        [Required(
            ErrorMessage = "OTP is required.")]
        [RegularExpression(
            @"^[0-9]{6}$",
            ErrorMessage = "Enter a valid 6-digit OTP.")]
        [Display(
            Name = "OTP")]
        public string OtpCode { get; set; }
            = string.Empty;
    }
}
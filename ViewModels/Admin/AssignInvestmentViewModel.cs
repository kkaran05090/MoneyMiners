using System.ComponentModel.DataAnnotations;

namespace MoneyMiners.ViewModels.Admin
{
    public sealed class AssignInvestmentViewModel
    {
        [Required(ErrorMessage = "Investor ID is required.")]
        [Display(Name = "Investor ID")]
        public string InvestorCode { get; set; } =
            string.Empty;

        public long InvestorAccountID { get; set; }

        public string? InvestorName { get; set; }

        public string? PhoneNumber { get; set; }

        public string? Email { get; set; }

        public string? AadhaarLast4 { get; set; }

        [Required(ErrorMessage = "Investment plan is required.")]
        [Display(Name = "Investment Plan")]
        public string PlanName { get; set; } =
            string.Empty;

        [Range(
            1,
            999999999999,
            ErrorMessage = "Enter a valid investment amount.")]
        [Display(Name = "Investment Amount")]
        public decimal InvestedAmount { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; } =
            DateTime.Today;

        [Range(
            1,
            120,
            ErrorMessage = "Duration must be between 1 and 120 months.")]
        [Display(Name = "Duration (Months)")]
        public short DurationMonths { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; } =
        DateTime.Today;

        [StringLength(100)]
        [Display(Name = "Payment Reference")]
        public string? PaymentReference { get; set; }

        [StringLength(500)]
        public string? Remarks { get; set; }
    }
}
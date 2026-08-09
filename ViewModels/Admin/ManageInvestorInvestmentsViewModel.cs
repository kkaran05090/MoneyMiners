using MoneyMiners.Models;

namespace MoneyMiners.ViewModels.Admin
{
    public sealed class ManageInvestorInvestmentsViewModel
    {
        public long InvestorAccountID { get; set; }

        public string InvestorCode { get; set; } =
            string.Empty;

        public string InvestorName { get; set; } =
            string.Empty;

        public string PhoneNumber { get; set; } =
            string.Empty;

        public string? Email { get; set; }

        public string AadhaarLast4 { get; set; } =
            string.Empty;

        public bool IsActive { get; set; }

        public bool IsMobileVerified { get; set; }

        public IReadOnlyList<AdminInvestorInvestmentItem>
            Investments
        { get; set; } =
                Array.Empty<AdminInvestorInvestmentItem>();

        public int ActiveInvestmentCount =>
            Investments.Count(
                investment =>
                    investment.Status == "Active");

        public decimal TotalActiveInvestment =>
            Investments
                .Where(
                    investment =>
                        investment.Status == "Active")
                .Sum(
                    investment =>
                        investment.InvestedAmount);
    }
}
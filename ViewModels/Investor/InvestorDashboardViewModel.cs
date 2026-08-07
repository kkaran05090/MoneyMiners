using MoneyMiners.Models;

namespace MoneyMiners.ViewModels.Investor
{
    public sealed class InvestorDashboardViewModel
    {
        public string InvestorCode { get; set; } =
            string.Empty;

        public string DisplayName { get; set; } =
            string.Empty;

        public IReadOnlyList<InvestorActiveInvestment>
            ActiveInvestments
        { get; set; } =
                Array.Empty<InvestorActiveInvestment>();

        public IReadOnlyList<InvestorInvestmentHistoryItem>
            InvestmentHistory
        { get; set; } =
                Array.Empty<InvestorInvestmentHistoryItem>();

        public int ActivePlansCount =>
            ActiveInvestments.Count;

        public decimal TotalInvestedAmount =>
            ActiveInvestments.Sum(
                investment =>
                    investment.InvestedAmount);
    }
}
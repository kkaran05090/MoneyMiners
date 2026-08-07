namespace MoneyMiners.ViewModels.Admin
{
    public sealed class InvestorsPageViewModel
    {
        public List<InvestorListItemViewModel> Investors { get; set; }
            = new();

        public string? Status { get; set; }

        public string? Search { get; set; }

        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 20;

        public int TotalRecords { get; set; }

        public int TotalPages =>
            TotalRecords == 0
                ? 1
                : (int)Math.Ceiling(
                    TotalRecords / (double)PageSize);
    }
}

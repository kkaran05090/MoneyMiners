using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoneyMiners.Repositories;
using MoneyMiners.ViewModels.Investor;
using System.Security.Claims;

namespace MoneyMiners.Controllers
{
    [Authorize(
        AuthenticationSchemes = "InvestorCookie",
        Roles = "Investor")]
    public sealed class InvestorDashboardController : Controller
    {
        private readonly IInvestmentRepository
            _investmentRepository;

        public InvestorDashboardController(
            IInvestmentRepository investmentRepository)
        {
            _investmentRepository =
                investmentRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            CancellationToken cancellationToken)
        {
            var investorAccountIdValue =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (!long.TryParse(
                    investorAccountIdValue,
                    out var investorAccountId) ||
                investorAccountId <= 0)
            {
                return Challenge(
                    "InvestorCookie");
            }

            var activeInvestmentsTask =
                _investmentRepository
                    .GetActiveByInvestorAccountIdAsync(
                        investorAccountId,
                        cancellationToken);

            var investmentHistoryTask =
                _investmentRepository
                    .GetHistoryByInvestorAccountIdAsync(
                        investorAccountId,
                        cancellationToken);

            await Task.WhenAll(
                activeInvestmentsTask,
                investmentHistoryTask);

            var model =
                new InvestorDashboardViewModel
                {
                    InvestorCode =
                        User.FindFirstValue(
                            "InvestorCode")
                        ?? string.Empty,

                    DisplayName =
                        User.Identity?.Name
                        ?? "Investor",

                    ActiveInvestments =
                        await activeInvestmentsTask,

                    InvestmentHistory =
                        await investmentHistoryTask
                };

            return View(model);
        }
    }
}
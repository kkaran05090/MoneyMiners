using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using MoneyMiners.Models;
using MoneyMiners.Repositories;
using MoneyMiners.ViewModels.Admin;
using System.Security.Claims;

namespace MoneyMiners.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    public sealed class AdminInvestmentsController : Controller
    {
        private readonly IInvestorAccountRepository
            _investorAccountRepository;

        private readonly IInvestmentRepository
            _investmentRepository;

        public AdminInvestmentsController(
            IInvestorAccountRepository investorAccountRepository,
            IInvestmentRepository investmentRepository)
        {
            _investorAccountRepository =
                investorAccountRepository;

            _investmentRepository =
                investmentRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Assign(
            string? investorCode,
            CancellationToken cancellationToken)
        {
            var model =
                new AssignInvestmentViewModel
                {
                    StartDate = DateTime.Today
                };

            if (string.IsNullOrWhiteSpace(investorCode))
            {
                return View(model);
            }

            var investor =
                await _investorAccountRepository
                    .GetByInvestorCodeAsync(
                        investorCode.Trim(),
                        cancellationToken);

            if (investor == null)
            {
                ViewData["InvestorSearchError"] =
                    "Investor ID was not found.";

                model.InvestorCode =
                    investorCode.Trim();

                return View(model);
            }

            if (!investor.IsActive ||
                !investor.IsMobileVerified)
            {
                ViewData["InvestorSearchError"] =
                    "Investor account is not active or verified.";

                model.InvestorCode =
                    investor.InvestorCode;

                return View(model);
            }

            FillInvestorDetails(
                model,
                investor);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            AssignInvestmentViewModel model,
            CancellationToken cancellationToken)
        {
            if (model.InvestorAccountID <= 0)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Please search and select a valid investor.");
            }

            if (string.IsNullOrWhiteSpace(
                    model.InvestorCode))
            {
                ModelState.AddModelError(
                    nameof(model.InvestorCode),
                    "Investor ID is required.");
            }

            if (model.PlanName != "Silver" &&
                model.PlanName != "Gold" &&
                model.PlanName != "Platinum")
            {
                ModelState.AddModelError(
                    nameof(model.PlanName),
                    "Please select a valid investment plan.");
            }

            if (model.EndDate < model.StartDate)
            {
                ModelState.AddModelError(
                    nameof(model.EndDate),
                    "End date cannot be before start date.");
            }

            InvestorLookupResult? investor = null;

            if (!string.IsNullOrWhiteSpace(
                    model.InvestorCode))
            {
                investor =
                    await _investorAccountRepository
                        .GetByInvestorCodeAsync(
                            model.InvestorCode.Trim(),
                            cancellationToken);
            }

            if (investor == null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Investor verification failed.");
            }
            else if (
                investor.InvestorAccountID !=
                model.InvestorAccountID)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Investor verification failed.");
            }
            else if (
                !investor.IsActive ||
                !investor.IsMobileVerified)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Investor account is not active or verified.");
            }
            else
            {
                FillInvestorDetails(
                    model,
                    investor);
            }

            if (!ModelState.IsValid)
            {
                return View(
                    "Assign",
                    model);
            }

            var adminUserIdValue =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (!int.TryParse(
                    adminUserIdValue,
                    out var adminUserId) ||
                adminUserId <= 0)
            {
                return Forbid();
            }

            try
            {
                var command =
                    new CreateInvestmentCommand
                    {
                        InvestorAccountID =
                            model.InvestorAccountID,

                        PlanName =
                            model.PlanName,

                        InvestedAmount =
                            model.InvestedAmount,

                        StartDate =
                            model.StartDate,

                        EndDate =
                            model.EndDate,

                        DurationMonths =
                            model.DurationMonths,

                        PaymentReference =
                            model.PaymentReference,

                        Remarks =
                            model.Remarks,

                        CreatedByAdminUserID =
                            adminUserId
                    };

                var result =
                    await _investmentRepository
                        .CreateAsync(
                            command,
                            cancellationToken);

                TempData["InvestmentSuccess"] =
                    "Investment " +
                    result.InvestmentCode +
                    " assigned successfully.";

                return RedirectToAction(
                    nameof(Assign),
                    new
                    {
                        investorCode =
                            model.InvestorCode
                    });
            }
            catch (SqlException exception)
            {
                string message;

                switch (exception.Number)
                {
                    case 53401:
                        message =
                            "Active verified investor account was not found.";
                        break;

                    case 53402:
                        message =
                            "Invalid investment plan.";
                        break;

                    case 53403:
                        message =
                            "Investment amount must be greater than zero.";
                        break;

                    case 53404:
                        message =
                            "Investment duration must be greater than zero.";
                        break;

                    case 53405:
                        message =
                            "Start date and end date are required.";
                        break;

                    case 53406:
                        message =
                            "End date cannot be before start date.";
                        break;

                    default:
                        message =
                            "Investment could not be created.";
                        break;
                }

                ModelState.AddModelError(
                    string.Empty,
                    message);

                return View(
                    "Assign",
                    model);
            }
        }

        private static void FillInvestorDetails(
            AssignInvestmentViewModel model,
            InvestorLookupResult investor)
        {
            model.InvestorAccountID =
                investor.InvestorAccountID;

            model.InvestorCode =
                investor.InvestorCode;

            model.InvestorName =
                investor.DisplayName;

            model.PhoneNumber =
                investor.PhoneNumber;

            model.Email =
                investor.Email;

            model.AadhaarLast4 =
                investor.AadhaarLast4;
        }
    }
}
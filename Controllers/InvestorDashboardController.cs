
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

        private readonly INotificationRepository
            _notificationRepository;

        public InvestorDashboardController(
            IInvestmentRepository investmentRepository,
            INotificationRepository notificationRepository)
        {
            _investmentRepository =
                investmentRepository;

            _notificationRepository =
                notificationRepository;
        }

        // ===================================
        // Investor Dashboard
        // ===================================

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
                return Challenge("InvestorCookie");
            }

            // Active Investments
            var activeInvestmentsTask =
                _investmentRepository
                    .GetActiveByInvestorAccountIdAsync(
                        investorAccountId,
                        cancellationToken);

            // Investment History
            var investmentHistoryTask =
                _investmentRepository
                    .GetHistoryByInvestorAccountIdAsync(
                        investorAccountId,
                        cancellationToken);

            // Published Notifications
            var publishedNotificationsTask =
                _notificationRepository
                    .GetPublishedAsync(
                        cancellationToken);

            // Unread Notification Count
            var unreadNotificationCountTask =
                _notificationRepository
                    .GetUnreadCountAsync(
                        investorAccountId,
                        cancellationToken);

            // Already Read Notification IDs
            var readNotificationIdsTask =
                _notificationRepository
                    .GetReadNotificationIdsAsync(
                        investorAccountId,
                        cancellationToken);

            // Wait for all database operations
            await Task.WhenAll(
                activeInvestmentsTask,
                investmentHistoryTask,
                publishedNotificationsTask,
                unreadNotificationCountTask,
                readNotificationIdsTask);

            // Prepare Dashboard ViewModel
            var model =
                new InvestorDashboardViewModel
                {
                    InvestorCode =
                        User.FindFirstValue("InvestorCode")
                        ?? string.Empty,

                    DisplayName =
                        User.Identity?.Name ?? "Investor",

                    ActiveInvestments =
                        await activeInvestmentsTask,

                    InvestmentHistory =
                        await investmentHistoryTask,

                    PublishedNotifications =
                        await publishedNotificationsTask,

                    UnreadNotificationCount =
                        await unreadNotificationCountTask,

                    ReadNotificationIds =
                        await readNotificationIdsTask
                };

            return View(model);
        }

        // ===================================
        // Mark Notification as Read
        // ===================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkNotificationRead(
            long notificationId,
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
                return Challenge("InvestorCookie");
            }

            if (notificationId <= 0)
            {
                return BadRequest(
                    "Invalid notification ID.");
            }

            await _notificationRepository.MarkAsReadAsync(
                notificationId,
                investorAccountId,
                cancellationToken);

            return RedirectToAction(nameof(Index));
        }
    }
}

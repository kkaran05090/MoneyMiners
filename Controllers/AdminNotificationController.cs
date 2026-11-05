
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using MoneyMiners.Models;
using MoneyMiners.Repositories;

namespace MoneyMiners.Controllers
{
    [Authorize(
        AuthenticationSchemes =
            CookieAuthenticationDefaults.AuthenticationScheme,
        Roles = "Admin,SuperAdmin")]
    public sealed class AdminNotificationController : Controller
    {
        private readonly INotificationRepository
            _notificationRepository;

        public AdminNotificationController(
            INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }

        // Display all notifications
        [HttpGet]
        public async Task<IActionResult> Index(
            CancellationToken cancellationToken)
        {
            var notifications =
                await _notificationRepository.GetAllAsync(
                    cancellationToken);

            return View(notifications);
        }

        // Create new notification
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            string title,
            string message,
            CancellationToken cancellationToken)
        {
            title = title?.Trim() ?? string.Empty;
            message = message?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(title) ||
                string.IsNullOrWhiteSpace(message))
            {
                TempData["AdminError"] =
                    "Title and message are required.";

                return RedirectToAction(nameof(Index));
            }

            var notification = new Notification
            {
                Title = title,
                Message = message,
                IsPublished = false,
                IsDeleted = false
            };

            await _notificationRepository.CreateAsync(
                notification,
                cancellationToken);

            TempData["AdminSuccess"] =
                "Notification created successfully.";

            return RedirectToAction(nameof(Index));
        }

        // Publish notification
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Publish(
            long notificationId,
            CancellationToken cancellationToken)
        {
            if (notificationId <= 0)
            {
                return BadRequest("Invalid notification ID.");
            }

            await _notificationRepository.PublishAsync(
                notificationId,
                cancellationToken);

            TempData["AdminSuccess"] =
                "Notification published successfully.";

            return RedirectToAction(nameof(Index));
        }

        // Soft delete notification
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(
            long notificationId,
            CancellationToken cancellationToken)
        {
            if (notificationId <= 0)
            {
                return BadRequest("Invalid notification ID.");
            }

            await _notificationRepository.SoftDeleteAsync(
                notificationId,
                cancellationToken);

            TempData["AdminSuccess"] =
                "Notification deleted successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}

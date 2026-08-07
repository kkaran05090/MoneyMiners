using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using MoneyMiners.Repositories;
using Microsoft.AspNetCore.Authorization;

namespace MoneyMiners.Controllers
{
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class AdminController : Controller
    {
        private readonly IContactMessageRepository
            _contactMessageRepository;

        private static readonly string[] AllowedStatuses =
        {
            "New",
            "InProgress",
            "Resolved",
            "Closed"
        };

        public AdminController(
            IContactMessageRepository contactMessageRepository)
        {
            _contactMessageRepository =
                contactMessageRepository;
        }

        [HttpGet]
        public IActionResult Dashboard()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            string? status,
            string? search,
            int pageNumber = 1,
            CancellationToken cancellationToken = default)
        {
            const int pageSize = 20;

            var model =
                await _contactMessageRepository.GetAllAsync(
                    status,
                    search,
                    pageNumber,
                    pageSize,
                    cancellationToken);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(
            long contactMessageId,
            string status,
            string rowVersion,
            string? currentStatus,
            string? search,
            int pageNumber = 1,
            CancellationToken cancellationToken = default)
        {
            if (!AllowedStatuses.Contains(
                    status,
                    StringComparer.OrdinalIgnoreCase))
            {
                TempData["AdminError"] =
                    "Invalid message status.";

                return RedirectToAction(
                    nameof(Index),
                    new
                    {
                        status = currentStatus,
                        search,
                        pageNumber
                    });
            }

            byte[] rowVersionBytes;

            try
            {
                rowVersionBytes =
                    Convert.FromBase64String(rowVersion);
            }
            catch (FormatException)
            {
                TempData["AdminError"] =
                    "Invalid record version. Please refresh the page.";

                return RedirectToAction(
                    nameof(Index),
                    new
                    {
                        status = currentStatus,
                        search,
                        pageNumber
                    });
            }

            try
            {
                await _contactMessageRepository.UpdateStatusAsync(
                    contactMessageId,
                    status,
                    rowVersionBytes,
                    cancellationToken);

                TempData["AdminSuccess"] =
                    "Message status updated successfully.";
            }
            catch (SqlException ex) when (ex.Number == 50021)
            {
                TempData["AdminError"] =
                    "This message was changed by another user. Please refresh the page.";
            }

            return RedirectToAction(
                nameof(Index),
                new
                {
                    status = currentStatus,
                    search,
                    pageNumber
                });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMessage(
            long contactMessageId,
            string rowVersion,
            string? status,
            string? search,
            int pageNumber = 1,
            CancellationToken cancellationToken = default)
        {
            byte[] rowVersionBytes;

            try
            {
                rowVersionBytes =
                    Convert.FromBase64String(rowVersion);
            }
            catch (FormatException)
            {
                TempData["AdminError"] =
                    "Invalid record version. Please refresh the page.";

                return RedirectToAction(
                    nameof(Index),
                    new
                    {
                        status,
                        search,
                        pageNumber
                    });
            }

            try
            {
                await _contactMessageRepository.SoftDeleteAsync(
                    contactMessageId,
                    rowVersionBytes,
                    cancellationToken);

                TempData["AdminSuccess"] =
                    "Message removed successfully.";
            }
            catch (SqlException ex) when (ex.Number == 50030)
            {
                TempData["AdminError"] =
                    "This message was changed or already removed. Please refresh the page.";
            }

            return RedirectToAction(
                nameof(Index),
                new
                {
                    status,
                    search,
                    pageNumber
                });
        }
    }
}
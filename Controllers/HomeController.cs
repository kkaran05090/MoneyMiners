using Microsoft.AspNetCore.Mvc;
using MoneyMiners.Models;
using MoneyMiners.Repositories;
using System.Diagnostics;

namespace MoneyMiners.Controllers
{
    public class HomeController : Controller
    {
        private readonly IContactMessageRepository
            _contactMessageRepository;

        public HomeController(
            IContactMessageRepository contactMessageRepository)
        {
            _contactMessageRepository = contactMessageRepository;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Contact(
            ContactMessage contactMessage,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                ViewData["ScrollToContact"] = true;

                return View("Index", contactMessage);
            }

            await _contactMessageRepository.CreateAsync(
                contactMessage,
                cancellationToken);

            TempData["ContactSuccess"] =
                "Thank you! Your message has been submitted successfully.";

            return RedirectToAction(
                nameof(Index),
                "Home",
                new { },
                "contact"
            );
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(
            Duration = 0,
            Location = ResponseCacheLocation.None,
            NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId =
                    Activity.Current?.Id ??
                    HttpContext.TraceIdentifier
            });
        }
    }
}
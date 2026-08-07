using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoneyMiners.Repositories;

namespace MoneyMiners.Controllers
{
    [Authorize(Roles = "Admin,SuperAdmin")]
    public sealed class InvestorsController : Controller
    {
        private readonly IInvestorRepository _investorRepository;

        public InvestorsController(
            IInvestorRepository investorRepository)
        {
            _investorRepository = investorRepository;
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
                await _investorRepository.GetAllAsync(
                    status,
                    search,
                    pageNumber,
                    pageSize,
                    cancellationToken);

            return View(model);
        }
    }
}
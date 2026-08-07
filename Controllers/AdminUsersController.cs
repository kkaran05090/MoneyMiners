using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using MoneyMiners.Models;
using MoneyMiners.Repositories;
using MoneyMiners.ViewModels.Admin;

namespace MoneyMiners.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    public sealed class AdminUsersController : Controller
    {
        private readonly IAdminUserRepository _adminUserRepository;
        private readonly IPasswordHasher<AdminUser> _passwordHasher;

        public AdminUsersController(
            IAdminUserRepository adminUserRepository,
            IPasswordHasher<AdminUser> passwordHasher)
        {
            _adminUserRepository = adminUserRepository;
            _passwordHasher = passwordHasher;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            CancellationToken cancellationToken)
        {
            var adminUsers =
                await _adminUserRepository.GetAllAsync(
                    cancellationToken);

            return View(adminUsers);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new AdminCreateViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            AdminCreateViewModel model,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var adminUser = new AdminUser
            {
                Username = model.Username.Trim(),
                Email = model.Email.Trim(),
                Role = "Admin",
                IsActive = true
            };

            var passwordHash =
                _passwordHasher.HashPassword(
                    adminUser,
                    model.Password);

            try
            {
                await _adminUserRepository.CreateAsync(
                    adminUser.Username,
                    adminUser.Email,
                    passwordHash,
                    "Admin",
                    cancellationToken);
            }
            catch (SqlException exception)
                when (exception.Number == 51024)
            {
                ModelState.AddModelError(
                    nameof(model.Username),
                    "This username already exists.");

                return View(model);
            }
            catch (SqlException exception)
                when (exception.Number == 51025)
            {
                ModelState.AddModelError(
                    nameof(model.Email),
                    "This email address already exists.");

                return View(model);
            }

            TempData["AdminUserSuccess"] =
                "New admin account created successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}
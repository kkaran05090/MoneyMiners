using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MoneyMiners.Models;
using MoneyMiners.Repositories;
using MoneyMiners.ViewModels.Admin;
using System.Security.Claims;
using Microsoft.Data.SqlClient;

namespace MoneyMiners.Controllers
{
    public sealed class AdminAccountController : Controller
    {
        private readonly IAdminUserRepository _adminUserRepository;
        private readonly IPasswordHasher<AdminUser> _passwordHasher;

        public AdminAccountController(
            IAdminUserRepository adminUserRepository,
            IPasswordHasher<AdminUser> passwordHasher)
        {
            _adminUserRepository = adminUserRepository;
            _passwordHasher = passwordHasher;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Setup(
    CancellationToken cancellationToken)
        {
            var hasAnyAdmin =
                await _adminUserRepository.HasAnyAsync(
                    cancellationToken);

            if (hasAnyAdmin)
            {
                return RedirectToAction(nameof(Login));
            }

            return View(new AdminSetupViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Setup(
            AdminSetupViewModel model,
            CancellationToken cancellationToken)
        {
            var hasAnyAdmin =
                await _adminUserRepository.HasAnyAsync(
                    cancellationToken);

            if (hasAnyAdmin)
            {
                return RedirectToAction(nameof(Login));
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var adminUser = new AdminUser
            {
                Username = model.Username.Trim(),
                Email = model.Email.Trim(),
                Role = "SuperAdmin",
                IsActive = true
            };

            var passwordHash =
                _passwordHasher.HashPassword(
                    adminUser,
                    model.Password);

            try
            {
                await _adminUserRepository.CreateFirstAsync(
                    adminUser.Username,
                    adminUser.Email,
                    passwordHash,
                    cancellationToken);
            }
            catch (SqlException exception)
                when (exception.Number == 51043)
            {
                return RedirectToAction(nameof(Login));
            }

            TempData["AdminSetupSuccess"] =
                "SuperAdmin account created successfully. Please sign in.";

            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Dashboard", "Admin");
            }

            return View(new AdminLoginViewModel
            {
                ReturnUrl = returnUrl
            });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
            AdminLoginViewModel model,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var adminUser =
                await _adminUserRepository.GetByLoginAsync(
                    model.LoginIdentifier,
                    cancellationToken);

            if (adminUser is null || !adminUser.IsActive)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Invalid username/email or password.");

                return View(model);
            }

            var currentUtc = DateTime.UtcNow;

            if (adminUser.LockoutEndUtc.HasValue &&
                adminUser.LockoutEndUtc.Value > currentUtc)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Account is temporarily locked. Please try again later.");

                return View(model);
            }

            var verificationResult =
                _passwordHasher.VerifyHashedPassword(
                    adminUser,
                    adminUser.PasswordHash,
                    model.Password);

            if (verificationResult ==
                PasswordVerificationResult.Failed)
            {
                await _adminUserRepository.RecordLoginAttemptAsync(
                    adminUser.AdminUserID,
                    false,
                    cancellationToken: cancellationToken);

                ModelState.AddModelError(
                    string.Empty,
                    "Invalid username/email or password.");

                return View(model);
            }

            await _adminUserRepository.RecordLoginAttemptAsync(
                adminUser.AdminUserID,
                true,
                cancellationToken: cancellationToken);

            var claims = new List<Claim>
            {
                new(
                    ClaimTypes.NameIdentifier,
                    adminUser.AdminUserID.ToString()),

                new(
                    ClaimTypes.Name,
                    adminUser.Username),

                new(
                    ClaimTypes.Email,
                    adminUser.Email),

                new(
                    ClaimTypes.Role,
                    adminUser.Role),

                new(
                    "SecurityStamp",
                    adminUser.SecurityStamp.ToString())
            };

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            var principal = new ClaimsPrincipal(identity);

            var authenticationProperties =
                new AuthenticationProperties
                {
                    IsPersistent = model.RememberMe,
                    AllowRefresh = true
                };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                authenticationProperties);

            if (!string.IsNullOrWhiteSpace(model.ReturnUrl) &&
                Url.IsLocalUrl(model.ReturnUrl))
            {
                return LocalRedirect(model.ReturnUrl);
            }

            return RedirectToAction("Dashboard", "Admin");
        }

        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
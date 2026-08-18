using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using MoneyMiners.Models;
using MoneyMiners.Repositories;
using MoneyMiners.Services;
using MoneyMiners.ViewModels.Admin;
using System.Security.Claims;

namespace MoneyMiners.Controllers
{
    public sealed class AdminAccountController : Controller
    {
        private readonly IAdminUserRepository
        _adminUserRepository;

        private readonly IPasswordHasher<AdminUser>
            _passwordHasher;

        private readonly IAdminPasswordResetOtpService
            _adminPasswordResetOtpService;

        private readonly IAdminPasswordResetEmailOtpService
           _adminPasswordResetEmailOtpService;

        private readonly IAdminMobileVerificationOtpService
            _adminMobileVerificationOtpService;


        public AdminAccountController(
     IAdminUserRepository adminUserRepository,
     IPasswordHasher<AdminUser> passwordHasher,
     IAdminPasswordResetOtpService adminPasswordResetOtpService,
     IAdminPasswordResetEmailOtpService adminPasswordResetEmailOtpService,
     IAdminMobileVerificationOtpService adminMobileVerificationOtpService)
        {
            _adminUserRepository =
                adminUserRepository;

            _passwordHasher =
                passwordHasher;

            _adminPasswordResetOtpService =
                adminPasswordResetOtpService;

            _adminPasswordResetEmailOtpService =
                adminPasswordResetEmailOtpService;

            _adminMobileVerificationOtpService =
                adminMobileVerificationOtpService;
        }


        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Setup(
            CancellationToken cancellationToken)
        {
            var hasAnyAdmin =
                await _adminUserRepository
                    .HasAnyAsync(
                        cancellationToken);

            if (hasAnyAdmin)
            {
                return RedirectToAction(
                    nameof(Login));
            }

            return View(
                new AdminSetupViewModel());
        }


        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Setup(
            AdminSetupViewModel model,
            CancellationToken cancellationToken)
        {
            var hasAnyAdmin =
                await _adminUserRepository
                    .HasAnyAsync(
                        cancellationToken);

            if (hasAnyAdmin)
            {
                return RedirectToAction(
                    nameof(Login));
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var adminUser =
                new AdminUser
                {
                    Username =
                        model.Username.Trim(),

                    Email =
                        model.Email.Trim(),

                    Role =
                        "SuperAdmin",

                    IsActive =
                        true
                };

            var passwordHash =
                _passwordHasher
                    .HashPassword(
                        adminUser,
                        model.Password);

            try
            {
                await _adminUserRepository
                    .CreateFirstAsync(
                        adminUser.Username,
                        adminUser.Email,
                        passwordHash,
                        cancellationToken);
            }
            catch (SqlException exception)
                when (exception.Number == 51043)
            {
                return RedirectToAction(
                    nameof(Login));
            }

            TempData["AdminSetupSuccess"] =
                "SuperAdmin account created successfully. Please sign in.";

            return RedirectToAction(
                nameof(Login));
        }


        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View(
                new AdminForgotPasswordViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(
     AdminForgotPasswordViewModel model,
     CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var emailAddress =
                model.Email
                    .Trim()
                    .ToLowerInvariant();

            var adminUser =
                await _adminUserRepository
                    .GetByLoginAsync(
                        emailAddress,
                        cancellationToken);

            if (adminUser is null ||
                !adminUser.IsActive ||
                string.IsNullOrWhiteSpace(
                    adminUser.Email) ||
                !string.Equals(
                    adminUser.Email.Trim(),
                    emailAddress,
                    StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Password reset request could not be processed.");

                return View(model);
            }

            try
            {
                var challenge =
                    await _adminPasswordResetEmailOtpService
                        .SendAsync(
                            adminUser.AdminUserID,
                            adminUser.Email,
                            cancellationToken);

                return RedirectToAction(
                    nameof(VerifyPasswordResetOtp),
                    new
                    {
                        challengeId =
                            challenge.AdminPasswordResetEmailOtpChallengeID,

                        adminUserId =
                            challenge.AdminUserID,

                        emailAddress =
                            challenge.EmailAddress
                    });
            }
            catch (SqlException exception)
            {
                var message =
                    exception.Number switch
                    {
                        52210 =>
                            "Please wait before requesting another OTP.",

                        52211 =>
                            "OTP request limit exceeded. Please try again later.",

                        _ =>
                            "Password reset request could not be processed."
                    };

                ModelState.AddModelError(
                    string.Empty,
                    message);

                return View(model);
            }
            catch (ArgumentException exception)
            {
                ModelState.AddModelError(
                    string.Empty,
                    exception.Message);

                return View(model);
            }
        }



        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(
            string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction(
                    "Dashboard",
                    "Admin");
            }

            return View(
                new AdminLoginViewModel
                {
                    ReturnUrl =
                        returnUrl
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
                await _adminUserRepository
                    .GetByLoginAsync(
                        model.LoginIdentifier,
                        cancellationToken);

            if (adminUser is null ||
                !adminUser.IsActive)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Invalid username/email or password.");

                return View(model);
            }

            var currentUtc =
                DateTime.UtcNow;

            if (adminUser.LockoutEndUtc.HasValue &&
                adminUser.LockoutEndUtc.Value >
                currentUtc)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Account is temporarily locked. Please try again later.");

                return View(model);
            }

            var verificationResult =
                _passwordHasher
                    .VerifyHashedPassword(
                        adminUser,
                        adminUser.PasswordHash,
                        model.Password);

            if (verificationResult ==
                PasswordVerificationResult.Failed)
            {
                await _adminUserRepository
                    .RecordLoginAttemptAsync(
                        adminUser.AdminUserID,
                        false,
                        cancellationToken:
                            cancellationToken);

                ModelState.AddModelError(
                    string.Empty,
                    "Invalid username/email or password.");

                return View(model);
            }

            await _adminUserRepository
                .RecordLoginAttemptAsync(
                    adminUser.AdminUserID,
                    true,
                    cancellationToken:
                        cancellationToken);

            var claims =
                new List<Claim>
                {
                    new(
                        ClaimTypes.NameIdentifier,
                        adminUser.AdminUserID
                            .ToString()),

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
                        adminUser.SecurityStamp
                            .ToString())
                };

            var identity =
                new ClaimsIdentity(
                    claims,
                    CookieAuthenticationDefaults
                        .AuthenticationScheme);

            var principal =
                new ClaimsPrincipal(
                    identity);

            var authenticationProperties =
                new AuthenticationProperties
                {
                    IsPersistent =
                        model.RememberMe,

                    AllowRefresh =
                        true
                };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults
                    .AuthenticationScheme,
                principal,
                authenticationProperties);

            if (!string.IsNullOrWhiteSpace(
                    model.ReturnUrl) &&
                Url.IsLocalUrl(
                    model.ReturnUrl))
            {
                return LocalRedirect(
                    model.ReturnUrl);
            }

            return RedirectToAction(
                "Dashboard",
                "Admin");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult VerifyPasswordResetOtp(
        long challengeId,
        long adminUserId,
        string emailAddress)
        {
            if (challengeId <= 0 ||
                adminUserId <= 0 ||
                string.IsNullOrWhiteSpace(emailAddress))
            {
                return RedirectToAction(
                    nameof(ForgotPassword));
            }

            return View(
                new AdminVerifyPasswordResetOtpViewModel
                {
                    ChallengeID =
                        challengeId,

                    AdminUserID =
                        adminUserId,

                    EmailAddress =
                        emailAddress
                            .Trim()
                            .ToLowerInvariant()
                });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyPasswordResetOtp(
     AdminVerifyPasswordResetOtpViewModel model,
     CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var result =
                    await _adminPasswordResetEmailOtpService
                        .VerifyAsync(
                            model.ChallengeID,
                            model.AdminUserID,
                            model.EmailAddress,
                            model.OtpCode,
                            cancellationToken);

                if (!result.IsVerified)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "OTP verification failed.");

                    return View(model);
                }

                TempData["AdminResetEmailChallengeID"] =
                    result.AdminPasswordResetEmailOtpChallengeID
                        .ToString();

                TempData["AdminResetUserID"] =
                    result.AdminUserID
                        .ToString();

                TempData["AdminResetEmailAddress"] =
                    result.EmailAddress;

                return RedirectToAction(
                    nameof(ResetPassword));
            }
            catch (SqlException exception)
            {
                var message =
                    exception.Number switch
                    {
                        52310 =>
                            "OTP verification request was not found.",

                        52311 =>
                            "This OTP has already been used.",

                        52312 =>
                            "OTP has expired. Please request a new OTP.",

                        52313 =>
                            "Maximum OTP attempts exceeded. Please request a new OTP.",

                        52314 =>
                            "The entered OTP is incorrect.",

                        _ =>
                            "OTP verification could not be completed."
                    };

                ModelState.AddModelError(
                    string.Empty,
                    message);

                return View(model);
            }
            catch (ArgumentException exception)
            {
                ModelState.AddModelError(
                    string.Empty,
                    exception.Message);

                return View(model);
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword()
        {
            var challengeIdValue =
                TempData["AdminResetEmailChallengeID"] as string;

            var adminUserIdValue =
                TempData["AdminResetUserID"] as string;

            var emailAddress =
                TempData["AdminResetEmailAddress"] as string;

            if (!long.TryParse(
                    challengeIdValue,
                    out var challengeId) ||
                challengeId <= 0 ||
                !long.TryParse(
                    adminUserIdValue,
                    out var adminUserId) ||
                adminUserId <= 0 ||
                string.IsNullOrWhiteSpace(
                    emailAddress))
            {
                return RedirectToAction(
                    nameof(ForgotPassword));
            }

            return View(
                new AdminResetPasswordViewModel
                {
                    ChallengeID =
                        challengeId,

                    AdminUserID =
                        adminUserId,

                    EmailAddress =
                        emailAddress
                            .Trim()
                            .ToLowerInvariant()
                });
        }


        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(
            AdminResetPasswordViewModel model,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var emailAddress =
                model.EmailAddress
                    .Trim()
                    .ToLowerInvariant();

            var adminUser =
                await _adminUserRepository
                    .GetByLoginAsync(
                        emailAddress,
                        cancellationToken);

            if (adminUser is null ||
                adminUser.AdminUserID != model.AdminUserID ||
                !adminUser.IsActive ||
                !string.Equals(
                    adminUser.Email.Trim(),
                    emailAddress,
                    StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Password reset request is invalid.");

                return View(model);
            }

            var passwordHash =
                _passwordHasher.HashPassword(
                    adminUser,
                    model.NewPassword);

            try
            {
                await _adminUserRepository
                    .CompletePasswordResetEmailAsync(
                        model.ChallengeID,
                        model.AdminUserID,
                        emailAddress,
                        passwordHash,
                        cancellationToken);

                TempData["AdminPasswordResetSuccess"] =
                    "Password reset successfully. Please sign in with your new password.";

                return RedirectToAction(
                    nameof(Login));
            }
            catch (SqlException exception)
            {
                var message =
                    exception.Number switch
                    {
                        52510 =>
                            "Password reset request was not found.",

                        52511 =>
                            "This password reset request has already been used.",

                        52512 =>
                            "Email OTP verification is required.",

                        52513 =>
                            "Password reset request has expired. Please start again.",

                        52514 =>
                            "Administrator account is not available for password reset.",

                        _ =>
                            "Password could not be reset."
                    };

                ModelState.AddModelError(
                    string.Empty,
                    message);

                return View(model);
            }
            catch (ArgumentException exception)
            {
                ModelState.AddModelError(
                    string.Empty,
                    exception.Message);

                return View(model);
            }
        }


        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> VerifyMobile(
        CancellationToken cancellationToken)
        {
            var adminUserIdValue =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (!long.TryParse(
                    adminUserIdValue,
                    out var adminUserId) ||
                adminUserId <= 0)
            {
                return RedirectToAction(
                    nameof(Login));
            }

            var adminUser =
                await _adminUserRepository
                    .GetByIdAsync(
                        adminUserId,
                        cancellationToken);

            if (adminUser is null ||
                !adminUser.IsActive)
            {
                return Forbid();
            }

            if (adminUser.IsMobileVerified)
            {
                TempData["AdminMobileVerificationSuccess"] =
                    "Your mobile number is already verified.";

                return RedirectToAction(
                    "Dashboard",
                    "Admin");
            }

            if (string.IsNullOrWhiteSpace(
                    adminUser.PhoneNumber))
            {
                return BadRequest(
                    "No mobile number is registered for this administrator.");
            }

            var phoneNumber =
                adminUser.PhoneNumber;

            var maskedPhoneNumber =
                phoneNumber.Length >= 4
                    ? new string(
                        '*',
                        phoneNumber.Length - 4)
                      + phoneNumber[^4..]
                    : phoneNumber;

            return View(
                new AdminMobileVerificationViewModel
                {
                    MaskedPhoneNumber =
                        maskedPhoneNumber
                });
        }

        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendMobileVerificationOtp(
        CancellationToken cancellationToken)
        {
            var adminUserIdValue =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (!long.TryParse(
                    adminUserIdValue,
                    out var adminUserId) ||
                adminUserId <= 0)
            {
                return RedirectToAction(
                    nameof(Login));
            }

            var adminUser =
                await _adminUserRepository
                    .GetByIdAsync(
                        adminUserId,
                        cancellationToken);

            if (adminUser is null ||
                !adminUser.IsActive)
            {
                return Forbid();
            }

            if (adminUser.IsMobileVerified)
            {
                TempData["AdminMobileVerificationSuccess"] =
                    "Your mobile number is already verified.";

                return RedirectToAction(
                    "Dashboard",
                    "Admin");
            }

            if (string.IsNullOrWhiteSpace(
                    adminUser.PhoneNumber))
            {
                TempData["AdminMobileVerificationError"] =
                    "No mobile number is registered for this administrator.";

                return RedirectToAction(
                    nameof(VerifyMobile));
            }

            try
            {
                var challenge =
                    await _adminMobileVerificationOtpService
                        .SendAsync(
                            adminUser.AdminUserID,
                            adminUser.PhoneNumber,
                            cancellationToken);

                return RedirectToAction(
                    nameof(VerifyMobileOtp),
                    new
                    {
                        challengeId =
                            challenge.AdminMobileVerificationOtpChallengeID
                    });
            }
            catch (SqlException exception)
            {
                var message =
                    exception.Number switch
                    {
                        52606 =>
                            "Your mobile number is already verified.",

                        52610 =>
                            "Please wait before requesting another OTP.",

                        52611 =>
                            "OTP request limit exceeded. Please try again later.",

                        _ =>
                            "Verification OTP could not be sent."
                    };

                TempData["AdminMobileVerificationError"] =
                    message;

                return RedirectToAction(
                    nameof(VerifyMobile));
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public IActionResult VerifyMobileOtp(
        long challengeId)
        {
            if (challengeId <= 0)
            {
                return RedirectToAction(
                    nameof(VerifyMobile));
            }

            return View(
                new AdminMobileVerificationOtpViewModel
                {
                    ChallengeID = challengeId
                });
        }


        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyMobileOtp(
        AdminMobileVerificationOtpViewModel model,
        CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var adminUserIdValue =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (!long.TryParse(
                    adminUserIdValue,
                    out var adminUserId) ||
                adminUserId <= 0)
            {
                return RedirectToAction(
                    nameof(Login));
            }

            var adminUser =
                await _adminUserRepository
                    .GetByIdAsync(
                        adminUserId,
                        cancellationToken);

            if (adminUser is null ||
                !adminUser.IsActive)
            {
                return Forbid();
            }

            if (adminUser.IsMobileVerified)
            {
                TempData["AdminMobileVerificationSuccess"] =
                    "Your mobile number is already verified.";

                return RedirectToAction(
                    "Dashboard",
                    "Admin");
            }

            if (string.IsNullOrWhiteSpace(
                    adminUser.PhoneNumber))
            {
                ModelState.AddModelError(
                    string.Empty,
                    "No mobile number is registered for this administrator.");

                return View(model);
            }

            try
            {
                var result =
                    await _adminMobileVerificationOtpService
                        .VerifyAsync(
                            model.ChallengeID,
                            adminUser.AdminUserID,
                            adminUser.PhoneNumber,
                            model.OtpCode,
                            cancellationToken);

                if (!result.IsVerified)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "Mobile verification failed.");

                    return View(model);
                }

                TempData["AdminMobileVerificationSuccess"] =
                    "Mobile number verified successfully.";

                return RedirectToAction(
                    "Dashboard",
                    "Admin");
            }
            catch (SqlException exception)
            {
                var message =
                    exception.Number switch
                    {
                        52710 =>
                            "OTP verification request was not found.",

                        52711 =>
                            "This OTP has already been used.",

                        52712 =>
                            "OTP has expired. Please request a new OTP.",

                        52713 =>
                            "Maximum OTP attempts exceeded. Please request a new OTP.",

                        52714 =>
                            "The entered OTP is incorrect.",

                        52715 =>
                            "Administrator account is not available.",

                        _ =>
                            "Mobile verification could not be completed."
                    };

                ModelState.AddModelError(
                    string.Empty,
                    message);

                return View(model);
            }
        }


        [HttpPost]
        [Authorize(
        Roles = "Admin,SuperAdmin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults
                    .AuthenticationScheme);

            return RedirectToAction(
                nameof(Login));
        }


        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
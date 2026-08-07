using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using MoneyMiners.Models;
using MoneyMiners.Repositories;
using MoneyMiners.Services;
using MoneyMiners.ViewModels.Investor;

namespace MoneyMiners.Controllers
{
    [AllowAnonymous]
    public sealed class InvestorAccountController : Controller
    {
        private const string OtpChallengeTempDataKey =
            "InvestorRegistrationOtpChallengeID";

        private const string PhoneNumberTempDataKey =
            "InvestorRegistrationPhoneNumber";

        private const string PasswordResetOtpChallengeTempDataKey =
            "InvestorPasswordResetOtpChallengeID";

        private const string PasswordResetPhoneNumberTempDataKey =
            "InvestorPasswordResetPhoneNumber";

        private readonly IInvestorAccountRepository
            _investorAccountRepository;

        private readonly IInvestorOtpService
            _investorOtpService;

        private readonly ISensitiveDataProtector
            _sensitiveDataProtector;

        private readonly IPasswordHasher<InvestorAccount>
            _passwordHasher;

        private readonly ILogger<InvestorAccountController>
            _logger;

        public InvestorAccountController(
            IInvestorAccountRepository investorAccountRepository,
            IInvestorOtpService investorOtpService,
            ISensitiveDataProtector sensitiveDataProtector,
            IPasswordHasher<InvestorAccount> passwordHasher,
            ILogger<InvestorAccountController> logger)
        {
            _investorAccountRepository =
                investorAccountRepository;

            _investorOtpService =
                investorOtpService;

            _sensitiveDataProtector =
                sensitiveDataProtector;

            _passwordHasher =
                passwordHasher;

            _logger =
                logger;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true &&
                User.IsInRole("Investor"))
            {
                return RedirectToAction(
                    "Index",
                    "InvestorDashboard");
            }

            return View(
                new InvestorLoginViewModel
                {
                    ReturnUrl = returnUrl
                });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
         InvestorLoginViewModel model,
         CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var account =
                    await _investorAccountRepository.GetByLoginAsync(
                        model.LoginIdentifier,
                        cancellationToken);

                if (account is null)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "Invalid Investor ID, mobile number or password.");

                    return View(model);
                }

                if (!account.IsActive)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "Your investor account is inactive. Please contact support.");

                    return View(model);
                }

                if (!account.IsMobileVerified)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "Your mobile number has not been verified.");

                    return View(model);
                }

                if (account.LockoutEndUtc.HasValue &&
                    account.LockoutEndUtc.Value > DateTime.UtcNow)
                {
                    var remainingMinutes =
                        Math.Max(
                            1,
                            (int)Math.Ceiling(
                                (account.LockoutEndUtc.Value - DateTime.UtcNow)
                                .TotalMinutes));

                    ModelState.AddModelError(
                        string.Empty,
                        $"Your account is temporarily locked. Try again after {remainingMinutes} minute(s).");

                    return View(model);
                }

                var verificationResult =
                    _passwordHasher.VerifyHashedPassword(
                        account,
                        account.PasswordHash,
                        model.Password);

                if (verificationResult ==
                    PasswordVerificationResult.Failed)
                {
                    var failedAttempt =
                        await _investorAccountRepository
                            .RecordLoginAttemptAsync(
                                account.InvestorAccountID,
                                false,
                                cancellationToken);

                    if (failedAttempt.IsLockedOut)
                    {
                        ModelState.AddModelError(
                            string.Empty,
                            "Too many incorrect attempts. Your account is locked for 15 minutes.");
                    }
                    else
                    {
                        var remainingAttempts =
                            Math.Max(
                                0,
                                5 - failedAttempt.FailedLoginCount);

                        ModelState.AddModelError(
                            string.Empty,
                            $"Invalid Investor ID, mobile number or password. {remainingAttempts} attempt(s) remaining.");
                    }

                    return View(model);
                }

                await _investorAccountRepository.RecordLoginAttemptAsync(
                    account.InvestorAccountID,
                    true,
                    cancellationToken);

                var claims =
                    new List<Claim>
                    {
                new(
                    ClaimTypes.NameIdentifier,
                    account.InvestorAccountID.ToString()),

                new(
                    ClaimTypes.Name,
                    account.DisplayName),

                new(
                    ClaimTypes.Role,
                    "Investor"),

                new(
                    "InvestorProfileID",
                    account.InvestorProfileID.ToString()),

                new(
                    "InvestorCode",
                    account.InvestorCode),

                new(
                    "PhoneNumber",
                    account.PhoneNumber),

                new(
                    "SecurityStamp",
                    account.SecurityStamp.ToString())
                    };

                var identity =
                    new ClaimsIdentity(
                        claims,
                        "InvestorCookie");

                var principal =
                    new ClaimsPrincipal(identity);

                var authenticationProperties =
                    new AuthenticationProperties
                    {
                        IsPersistent = model.RememberMe,
                        AllowRefresh = true
                    };

                if (model.RememberMe)
                {
                    authenticationProperties.ExpiresUtc =
                        DateTimeOffset.UtcNow.AddDays(14);
                }

                await HttpContext.SignInAsync(
                    "InvestorCookie",
                    principal,
                    authenticationProperties);

                if (!string.IsNullOrWhiteSpace(model.ReturnUrl) &&
                    Url.IsLocalUrl(model.ReturnUrl))
                {
                    return LocalRedirect(model.ReturnUrl);
                }

                return RedirectToAction(
                    "Index",
                    "InvestorDashboard");
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Investor login failed for identifier {LoginIdentifier}.",
                    model.LoginIdentifier);

                ModelState.AddModelError(
                    string.Empty,
                    "Login could not be completed. Please try again.");

                return View(model);
            }
        }


        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                "InvestorCookie");

            return RedirectToAction(
                "Index",
                "Home");
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View(
                new InvestorForgotPasswordViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(
         InvestorForgotPasswordViewModel model,
         CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var phoneNumber =
                    NormalizeDigits(model.PhoneNumber);

                var account =
                    await _investorAccountRepository.GetByLoginAsync(
                        phoneNumber,
                        cancellationToken);

                if (account is null || !account.IsActive)
                {
                    ModelState.AddModelError(
                        nameof(model.PhoneNumber),
                        "No active investor account was found with this mobile number.");

                    return View(model);
                }

                var challenge =
                    await _investorOtpService.SendAsync(
                        phoneNumber,
                        InvestorOtpPurpose.PasswordReset,
                        cancellationToken);

                TempData[PasswordResetOtpChallengeTempDataKey] =
                    challenge.InvestorOtpChallengeID.ToString();

                TempData[PasswordResetPhoneNumberTempDataKey] =
                    challenge.PhoneNumber;

                return RedirectToAction(
                    "VerifyForgotPasswordOtp");
            }
            catch (SqlException exception)
                when (exception.Number is 52010 or 52011)
            {
                ModelState.AddModelError(
                    string.Empty,
                    GetOtpRequestErrorMessage(exception));

                return View(model);
            }
            catch (ArgumentException exception)
            {
                ModelState.AddModelError(
                    nameof(model.PhoneNumber),
                    exception.Message);

                return View(model);
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Investor password reset OTP request failed.");

                ModelState.AddModelError(
                    string.Empty,
                    "OTP could not be sent. Please try again.");

                return View(model);
            }
        }

        [HttpGet]
        public IActionResult VerifyForgotPasswordOtp()
        {
            if (!TryReadPasswordResetTempData(
                    out var challengeId,
                    out var phoneNumber))
            {
                return RedirectToAction(
                    nameof(ForgotPassword));
            }

            return View(
                new InvestorForgotPasswordOtpViewModel
                {
                    InvestorOtpChallengeID =
                        challengeId,

                    PhoneNumber =
                        phoneNumber
                });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyForgotPasswordOtp(
        InvestorForgotPasswordOtpViewModel model,
        CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var result =
                    await _investorOtpService.VerifyAsync(
                        model.InvestorOtpChallengeID,
                        model.PhoneNumber,
                        InvestorOtpPurpose.PasswordReset,
                        model.OtpCode,
                        cancellationToken);

                if (!result.IsVerified)
                {
                    ModelState.AddModelError(
                        nameof(model.OtpCode),
                        "OTP verification failed.");

                    return View(model);
                }

                TempData[PasswordResetOtpChallengeTempDataKey] =
                    result.InvestorOtpChallengeID.ToString();

                TempData[PasswordResetPhoneNumberTempDataKey] =
                    result.PhoneNumber;

                return RedirectToAction(
                    "ResetPassword");
            }
            catch (SqlException exception)
                when (exception.Number is
                    52110 or
                    52111 or
                    52112 or
                    52113 or
                    52114)
            {
                ModelState.AddModelError(
                    nameof(model.OtpCode),
                    GetOtpVerificationErrorMessage(exception));

                return View(model);
            }
            catch (ArgumentException exception)
            {
                ModelState.AddModelError(
                    nameof(model.OtpCode),
                    exception.Message);

                return View(model);
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Investor password reset OTP verification failed.");

                ModelState.AddModelError(
                    string.Empty,
                    "OTP could not be verified. Please try again.");

                return View(model);
            }
        }

        [HttpGet]
        public IActionResult ResetPassword()
        {
            if (!TryReadPasswordResetTempData(
                    out var challengeId,
                    out var phoneNumber))
            {
                return RedirectToAction(
                    nameof(ForgotPassword));
            }

            return View(
                new InvestorResetPasswordViewModel
                {
                    InvestorOtpChallengeID =
                        challengeId,

                    PhoneNumber =
                        phoneNumber
                });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(
        InvestorResetPasswordViewModel model,
        CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var phoneNumber =
                    NormalizeDigits(model.PhoneNumber);

                var account =
                    await _investorAccountRepository.GetByLoginAsync(
                        phoneNumber,
                        cancellationToken);

                if (account is null || !account.IsActive)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "Investor account was not found.");

                    return View(model);
                }

                var passwordHash =
                    _passwordHasher.HashPassword(
                        account,
                        model.NewPassword);

                await _investorAccountRepository.ResetPasswordAsync(
                    new InvestorPasswordResetCommand
                    {
                        InvestorOtpChallengeID =
                            model.InvestorOtpChallengeID,

                        PhoneNumber =
                            phoneNumber,

                        PasswordHash =
                            passwordHash
                    },
                    cancellationToken);

                await HttpContext.SignOutAsync(
                    "InvestorCookie");

                TempData["InvestorPasswordResetSuccess"] =
                    "Password reset successful. Please login with your new password.";

                return RedirectToAction(
                    nameof(Login));
            }
            catch (SqlException exception)
                when (exception.Number is
                    53210 or
                    53211 or
                    53212 or
                    53213 or
                    53214)
            {
                var errorMessage =
                    exception.Number switch
                    {
                        53210 =>
                            "Invalid password reset request.",

                        53211 =>
                            "This OTP has already been used.",

                        53212 =>
                            "Mobile number has not been verified.",

                        53213 =>
                            "OTP verification has expired.",

                        53214 =>
                            "Investor account was not found.",

                        _ =>
                            "Password could not be reset."
                    };

                ModelState.AddModelError(
                    string.Empty,
                    errorMessage);

                return View(model);
            }
            catch (ArgumentException exception)
            {
                ModelState.AddModelError(
                    string.Empty,
                    exception.Message);

                return View(model);
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Investor password reset failed.");

                ModelState.AddModelError(
                    string.Empty,
                    "Password could not be reset. Please try again.");

                return View(model);
            }
        }



        // Step 1: Mobile number enter karke OTP request karega.
        [HttpGet]
        public IActionResult CreateAccount()
        {
            return View(
                new InvestorOtpRequestViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAccount(
            InvestorOtpRequestViewModel model,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var challenge =
                    await _investorOtpService.SendAsync(
                        model.PhoneNumber,
                        InvestorOtpPurpose.Registration,
                        cancellationToken);

                TempData[OtpChallengeTempDataKey] =
                    challenge.InvestorOtpChallengeID.ToString();

                TempData[PhoneNumberTempDataKey] =
                    challenge.PhoneNumber;

                return RedirectToAction(
                    nameof(VerifyRegistrationOtp));
            }
            catch (SqlException exception)
                when (exception.Number is 52010 or 52011)
            {
                ModelState.AddModelError(
                    string.Empty,
                    GetOtpRequestErrorMessage(exception));

                return View(model);
            }
            catch (ArgumentException exception)
            {
                ModelState.AddModelError(
                    nameof(model.PhoneNumber),
                    exception.Message);

                return View(model);
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Investor registration OTP request failed.");

                ModelState.AddModelError(
                    string.Empty,
                    "OTP could not be sent. Please try again.");

                return View(model);
            }
        }

        // Step 2: Mobile OTP verify karega.
        [HttpGet]
        public IActionResult VerifyRegistrationOtp()
        {
            if (!TryReadRegistrationTempData(
                    out var challengeId,
                    out var phoneNumber))
            {
                return RedirectToAction(
                    nameof(CreateAccount));
            }

            return View(
                new InvestorOtpVerifyViewModel
                {
                    InvestorOtpChallengeID =
                        challengeId,

                    PhoneNumber =
                        phoneNumber
                });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyRegistrationOtp(
            InvestorOtpVerifyViewModel model,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var result =
                    await _investorOtpService.VerifyAsync(
                        model.InvestorOtpChallengeID,
                        model.PhoneNumber,
                        InvestorOtpPurpose.Registration,
                        model.OtpCode,
                        cancellationToken);

                if (!result.IsVerified)
                {
                    ModelState.AddModelError(
                        nameof(model.OtpCode),
                        "OTP verification failed.");

                    return View(model);
                }

                TempData[OtpChallengeTempDataKey] =
                    result.InvestorOtpChallengeID.ToString();

                TempData[PhoneNumberTempDataKey] =
                    result.PhoneNumber;

                return RedirectToAction(
                    nameof(CompleteRegistration));
            }
            catch (SqlException exception)
                when (exception.Number is
                    52110 or
                    52111 or
                    52112 or
                    52113 or
                    52114)
            {
                ModelState.AddModelError(
                    nameof(model.OtpCode),
                    GetOtpVerificationErrorMessage(
                        exception));

                return View(model);
            }
            catch (ArgumentException exception)
            {
                ModelState.AddModelError(
                    nameof(model.OtpCode),
                    exception.Message);

                return View(model);
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Investor registration OTP verification failed.");

                ModelState.AddModelError(
                    string.Empty,
                    "OTP could not be verified. Please try again.");

                return View(model);
            }
        }

        // Step 3: Verified mobile ke baad complete profile form.
        [HttpGet]
        public IActionResult CompleteRegistration()
        {
            if (!TryReadRegistrationTempData(
                    out var challengeId,
                    out var phoneNumber))
            {
                return RedirectToAction(
                    nameof(CreateAccount));
            }

            return View(
                new InvestorRegisterViewModel
                {
                    InvestorOtpChallengeID =
                        challengeId,

                    PhoneNumber =
                        phoneNumber,

                    Country =
                        "India"
                });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteRegistration(
        InvestorRegisterViewModel model,
        CancellationToken cancellationToken)
        {
            if (!model.AcceptTerms)
            {
                ModelState.AddModelError(
                    nameof(model.AcceptTerms),
                    "You must accept the terms and privacy policy.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var phoneNumber =
                    NormalizeDigits(
                        model.PhoneNumber);

                var aadhaarNumber =
                    NormalizeDigits(
                        model.AadhaarNumber);

                var panNumber =
                    NormalizePan(
                        model.PANNumber);

                var accountForPasswordHash =
                    new InvestorAccount
                    {
                        InvestorCode =
                            string.Empty,

                        PhoneNumber =
                            phoneNumber,

                        FirstName =
                            model.FirstName.Trim(),

                        LastName =
                            NormalizeOptionalText(
                                model.LastName)
                    };

                var passwordHash =
                    _passwordHasher.HashPassword(
                        accountForPasswordHash,
                        model.Password);

                var registrationCommand =
                    new InvestorRegistrationCommand
                    {
                        InvestorOtpChallengeID =
                            model.InvestorOtpChallengeID,

                        FirstName =
                            model.FirstName.Trim(),

                        LastName =
                            NormalizeOptionalText(
                                model.LastName),

                        FatherName =
                            NormalizeOptionalText(
                                model.FatherName),

                        PhoneNumber =
                            phoneNumber,

                        Email =
                            NormalizeOptionalText(
                                model.Email),

                        AddressLine1 =
                            NormalizeOptionalText(
                                model.AddressLine1),

                        AddressLine2 =
                            NormalizeOptionalText(
                                model.AddressLine2),

                        City =
                            NormalizeOptionalText(
                                model.City),

                        State =
                            NormalizeOptionalText(
                                model.State),

                        Country =
                            string.IsNullOrWhiteSpace(
                                model.Country)
                                ? "India"
                                : model.Country.Trim(),

                        PostalCode =
                            NormalizeOptionalText(
                                model.PostalCode),

                        AadhaarCipherText =
                            _sensitiveDataProtector.Protect(
                                aadhaarNumber),

                        AadhaarHash =
                            _sensitiveDataProtector.ComputeHash(
                                $"AADHAAR|{aadhaarNumber}"),

                        AadhaarLast4 =
                            aadhaarNumber[^4..],

                        PANCipherText =
                            panNumber is null
                                ? null
                                : _sensitiveDataProtector.Protect(
                                    panNumber),

                        PANHash =
                            panNumber is null
                                ? null
                                : _sensitiveDataProtector.ComputeHash(
                                    $"PAN|{panNumber}"),

                        PANLast4 =
                            panNumber is null
                                ? null
                                : panNumber[^4..],

                        PasswordHash =
                            passwordHash
                    };

                var registrationResult =
                    await _investorAccountRepository.RegisterAsync(
                        registrationCommand,
                        cancellationToken);

                return View(
                    "RegistrationSuccess",
                    registrationResult);
            }
            catch (SqlException exception)
                when (exception.Number is >= 51010 and <= 51017)
            {
                ModelState.AddModelError(
                    string.Empty,
                    GetRegistrationErrorMessage(
                        exception));

                return View(model);
            }
            catch (ArgumentException exception)
            {
                ModelState.AddModelError(
                    string.Empty,
                    exception.Message);

                return View(model);
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Investor account registration failed.");

                ModelState.AddModelError(
                    string.Empty,
                    "Account could not be created. Please try again.");

                return View(model);
            }
        }

        private bool TryReadRegistrationTempData(
            out long challengeId,
            out string phoneNumber)
        {
            challengeId = 0;
            phoneNumber = string.Empty;

            var challengeValue =
                TempData[OtpChallengeTempDataKey];

            var phoneValue =
                TempData[PhoneNumberTempDataKey];

            if (challengeValue is null ||
                phoneValue is null)
            {
                return false;
            }

            if (!long.TryParse(
                    challengeValue.ToString(),
                    out challengeId))
            {
                return false;
            }

            phoneNumber =
                phoneValue.ToString()
                ?? string.Empty;

            return challengeId > 0 &&
                   !string.IsNullOrWhiteSpace(
                       phoneNumber);
        }


        private bool TryReadPasswordResetTempData(
    out long challengeId,
    out string phoneNumber)
        {
            challengeId = 0;
            phoneNumber = string.Empty;

            var challengeValue =
                TempData[PasswordResetOtpChallengeTempDataKey];

            var phoneValue =
                TempData[PasswordResetPhoneNumberTempDataKey];

            if (challengeValue is null ||
                phoneValue is null)
            {
                return false;
            }

            if (!long.TryParse(
                    challengeValue.ToString(),
                    out challengeId))
            {
                return false;
            }

            phoneNumber =
                phoneValue.ToString()
                ?? string.Empty;

            return challengeId > 0 &&
                   !string.IsNullOrWhiteSpace(phoneNumber);
        }

        private static string NormalizeDigits(
            string value)
        {
            return new string(
                value
                    .Where(char.IsDigit)
                    .ToArray());
        }

        private static string? NormalizePan(
            string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            return value
                .Trim()
                .Replace(" ", string.Empty)
                .ToUpperInvariant();
        }

        private static string? NormalizeOptionalText(
            string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }

        private static string GetOtpRequestErrorMessage(
            SqlException exception)
        {
            return exception.Number switch
            {
                52010 =>
                    "Please wait 60 seconds before requesting another OTP.",

                52011 =>
                    "Too many OTP requests. Please try again after one hour.",

                _ =>
                    "OTP could not be sent."
            };
        }

        private static string GetOtpVerificationErrorMessage(
            SqlException exception)
        {
            return exception.Number switch
            {
                52110 =>
                    "Invalid OTP verification request.",

                52111 =>
                    "This OTP has already been used.",

                52112 =>
                    "OTP has expired. Please request a new OTP.",

                52113 =>
                    "Maximum OTP attempts exceeded. Request a new OTP.",

                52114 =>
                    "The entered OTP is incorrect.",

                _ =>
                    "OTP verification failed."
            };
        }

        private static string GetRegistrationErrorMessage(
            SqlException exception)
        {
            return exception.Number switch
            {
                51010 =>
                    "An account already exists with this mobile number.",

                51011 =>
                    "An account already exists with this email address.",

                51012 =>
                    "An account already exists with this Aadhaar number.",

                51013 =>
                    "An account already exists with this PAN number.",

                51014 =>
                    "Invalid OTP verification request.",

                51015 =>
                    "This OTP has already been used.",

                51016 =>
                    "Mobile number has not been verified.",

                51017 =>
                    "OTP verification has expired. Please request a new OTP.",

                _ =>
                    "Account registration failed."
            };
        }
    }
}

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

        private const string EmailOtpChallengeTempDataKey =
            "InvestorRegistrationEmailOtpChallengeID";

        private const string EmailAddressTempDataKey =
            "InvestorRegistrationEmailAddress";

        private const string PasswordResetOtpChallengeTempDataKey =
            "InvestorPasswordResetOtpChallengeID";

        private const string PasswordResetPhoneNumberTempDataKey =
            "InvestorPasswordResetPhoneNumber";

        private const string PasswordResetEmailOtpChallengeTempDataKey =
            "InvestorPasswordResetEmailOtpChallengeID";

        private const string PasswordResetEmailAddressTempDataKey =
            "InvestorPasswordResetEmailAddress";

        private readonly IInvestorAccountRepository
            _investorAccountRepository;

        private readonly IInvestorOtpService
            _investorOtpService;

        private readonly IInvestorEmailOtpService
           _investorEmailOtpService;

        private readonly ISensitiveDataProtector
            _sensitiveDataProtector;

        private readonly IPasswordHasher<InvestorAccount>
            _passwordHasher;

        private readonly ILogger<InvestorAccountController>
            _logger;

        public InvestorAccountController(
        IInvestorAccountRepository investorAccountRepository,
        IInvestorOtpService investorOtpService,
        IInvestorEmailOtpService investorEmailOtpService,
        ISensitiveDataProtector sensitiveDataProtector,
        IPasswordHasher<InvestorAccount> passwordHasher,
        ILogger<InvestorAccountController> logger)
        {
            _investorAccountRepository =
                investorAccountRepository;

            _investorOtpService =
                investorOtpService;

            _investorEmailOtpService =
                investorEmailOtpService;

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
                        "Invalid Investor ID, email address or password.");

                    return View(model);
                }

                if (!account.IsActive)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "Your investor account is inactive. Please contact support.");

                    return View(model);
                }

                if (!account.IsEmailVerified)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "Your email address has not been verified.");

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
                            $"Invalid Investor ID, email address or password. {{remainingAttempts}} attempt(s) remaining.");
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
                     ClaimTypes.Email,
                     account.Email ?? string.Empty),

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
                var emailAddress =
                    model.Email
                        .Trim()
                        .ToLowerInvariant();

                var account =
                    await _investorAccountRepository.GetByLoginAsync(
                        emailAddress,
                        cancellationToken);

                if (account is null ||
                    !account.IsActive)
                {
                    ModelState.AddModelError(
                        nameof(model.Email),
                        "No active investor account was found with this email address.");

                    return View(model);
                }

                if (!account.IsEmailVerified)
                {
                    ModelState.AddModelError(
                        nameof(model.Email),
                        "This email address has not been verified.");

                    return View(model);
                }

                var challenge =
                    await _investorEmailOtpService.SendAsync(
                        emailAddress,
                        InvestorOtpPurpose.PasswordReset,
                        cancellationToken);

                TempData[PasswordResetEmailOtpChallengeTempDataKey] =
                    challenge.InvestorEmailOtpChallengeID.ToString();

                TempData[PasswordResetEmailAddressTempDataKey] =
                    challenge.EmailAddress;

                return RedirectToAction(
                    nameof(VerifyForgotPasswordOtp));
            }
            catch (SqlException exception)
                when (exception.Number is 52210 or 52211)
            {
                var errorMessage =
                    exception.Number switch
                    {
                        52210 =>
                            "Please wait 60 seconds before requesting another OTP.",

                        52211 =>
                            "Too many OTP requests. Please try again after one hour.",

                        _ =>
                            "OTP could not be sent."
                    };

                ModelState.AddModelError(
                    string.Empty,
                    errorMessage);

                return View(model);
            }
            catch (ArgumentException exception)
            {
                ModelState.AddModelError(
                    nameof(model.Email),
                    exception.Message);

                return View(model);
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Investor password reset email OTP request failed.");

                ModelState.AddModelError(
                    string.Empty,
                    "OTP could not be sent to your email. Please try again.");

                return View(model);
            }
        }

        [HttpGet]
        public IActionResult VerifyForgotPasswordOtp()
        {
            var challengeValue =
                TempData[PasswordResetEmailOtpChallengeTempDataKey];

            var emailValue =
                TempData[PasswordResetEmailAddressTempDataKey];

            if (challengeValue is null ||
                emailValue is null ||
                !long.TryParse(
                    challengeValue.ToString(),
                    out var challengeId) ||
                challengeId <= 0)
            {
                return RedirectToAction(
                    nameof(ForgotPassword));
            }

            var emailAddress =
                emailValue.ToString()
                ?? string.Empty;

            if (string.IsNullOrWhiteSpace(
                    emailAddress))
            {
                return RedirectToAction(
                    nameof(ForgotPassword));
            }

            return View(
                new InvestorForgotPasswordOtpViewModel
                {
                    InvestorEmailOtpChallengeID =
                        challengeId,

                    EmailAddress =
                        emailAddress
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
                    await _investorEmailOtpService.VerifyAsync(
                        model.InvestorEmailOtpChallengeID,
                        model.EmailAddress,
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

                TempData[PasswordResetEmailOtpChallengeTempDataKey] =
                    result.InvestorEmailOtpChallengeID.ToString();

                TempData[PasswordResetEmailAddressTempDataKey] =
                    result.EmailAddress;

                return RedirectToAction(
                    nameof(ResetPassword));
            }
            catch (SqlException exception)
                when (exception.Number is
                    52310 or
                    52311 or
                    52312 or
                    52313 or
                    52314)
            {
                var errorMessage =
                    exception.Number switch
                    {
                        52310 =>
                            "Invalid OTP verification request.",

                        52311 =>
                            "This OTP has already been used.",

                        52312 =>
                            "OTP has expired. Please request a new OTP.",

                        52313 =>
                            "Maximum OTP attempts exceeded. Request a new OTP.",

                        52314 =>
                            "The entered OTP is incorrect.",

                        _ =>
                            "OTP verification failed."
                    };

                ModelState.AddModelError(
                    nameof(model.OtpCode),
                    errorMessage);

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
                    "Investor password reset email OTP verification failed.");

                ModelState.AddModelError(
                    string.Empty,
                    "OTP could not be verified. Please try again.");

                return View(model);
            }
        }
        [HttpGet]
        public IActionResult ResetPassword()
        {
            var challengeValue =
                TempData[PasswordResetEmailOtpChallengeTempDataKey];

            var emailValue =
                TempData[PasswordResetEmailAddressTempDataKey];

            if (challengeValue is null ||
                emailValue is null ||
                !long.TryParse(
                    challengeValue.ToString(),
                    out var challengeId) ||
                challengeId <= 0)
            {
                return RedirectToAction(
                    nameof(ForgotPassword));
            }

            var emailAddress =
                emailValue.ToString()
                ?? string.Empty;

            if (string.IsNullOrWhiteSpace(
                    emailAddress))
            {
                return RedirectToAction(
                    nameof(ForgotPassword));
            }

            return View(
                new InvestorResetPasswordViewModel
                {
                    InvestorEmailOtpChallengeID =
                        challengeId,

                    EmailAddress =
                        emailAddress
                            .Trim()
                            .ToLowerInvariant()
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
                var emailAddress =
                    model.EmailAddress
                        .Trim()
                        .ToLowerInvariant();

                var account =
                    await _investorAccountRepository.GetByLoginAsync(
                        emailAddress,
                        cancellationToken);

                if (account is null ||
                    !account.IsActive)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "Investor account was not found.");

                    return View(model);
                }

                if (!account.IsEmailVerified)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "Email address has not been verified.");

                    return View(model);
                }

                var passwordHash =
                    _passwordHasher.HashPassword(
                        account,
                        model.NewPassword);

                await _investorAccountRepository.ResetPasswordAsync(
                    new InvestorPasswordResetCommand
                    {
                        // Current Email OTP flow
                        InvestorEmailOtpChallengeID =
                            model.InvestorEmailOtpChallengeID,

                        EmailAddress =
                            emailAddress,

                        // Future Mobile OTP flow
                        InvestorOtpChallengeID =
                            null,

                        PhoneNumber =
                            null,

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
                            "Invalid password reset verification request.",

                        53211 =>
                            "This OTP has already been used.",

                        53212 =>
                            "Email OTP has not been verified.",

                        53213 =>
                            "OTP verification has expired. Please request a new OTP.",

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
                    "Investor email password reset failed.");

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
                    await _investorEmailOtpService.SendAsync(
                        model.Email,
                        InvestorOtpPurpose.Registration,
                        cancellationToken);

                TempData[EmailOtpChallengeTempDataKey] =
                    challenge.InvestorEmailOtpChallengeID.ToString();

                TempData[EmailAddressTempDataKey] =
                    challenge.EmailAddress;

                return RedirectToAction(
                    nameof(VerifyRegistrationOtp));
            }
            catch (SqlException exception)
                when (exception.Number is 52210 or 52211)
            {
                var errorMessage =
                    exception.Number switch
                    {
                        52210 =>
                            "Please wait 60 seconds before requesting another OTP.",

                        52211 =>
                            "Too many OTP requests. Please try again after one hour.",

                        _ =>
                            "OTP could not be sent."
                    };

                ModelState.AddModelError(
                    string.Empty,
                    errorMessage);

                return View(model);
            }
            catch (ArgumentException exception)
            {
                ModelState.AddModelError(
                    nameof(model.Email),
                    exception.Message);

                return View(model);
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Investor registration email OTP request failed.");

                ModelState.AddModelError(
                    string.Empty,
                    "OTP could not be sent to your email. Please try again.");

                return View(model);
            }
        }

        // Step 2: Email OTP verify karega.
        [HttpGet]
        public IActionResult VerifyRegistrationOtp()
        {
            var challengeValue =
                TempData[EmailOtpChallengeTempDataKey];

            var emailValue =
                TempData[EmailAddressTempDataKey];

            if (challengeValue is null ||
                emailValue is null ||
                !long.TryParse(
                    challengeValue.ToString(),
                    out var challengeId) ||
                challengeId <= 0)
            {
                return RedirectToAction(
                    nameof(CreateAccount));
            }

            var emailAddress =
                emailValue.ToString()
                ?? string.Empty;

            if (string.IsNullOrWhiteSpace(
                    emailAddress))
            {
                return RedirectToAction(
                    nameof(CreateAccount));
            }

            return View(
                new InvestorEmailOtpVerifyViewModel
                {
                    InvestorEmailOtpChallengeID =
                        challengeId,

                    EmailAddress =
                        emailAddress
                });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyRegistrationOtp(
        InvestorEmailOtpVerifyViewModel model,
        CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var result =
                    await _investorEmailOtpService.VerifyAsync(
                        model.InvestorEmailOtpChallengeID,
                        model.EmailAddress,
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

                TempData[EmailOtpChallengeTempDataKey] =
                    result.InvestorEmailOtpChallengeID.ToString();

                TempData[EmailAddressTempDataKey] =
                    result.EmailAddress;

                return RedirectToAction(
                    nameof(CompleteRegistration));
            }
            catch (SqlException exception)
                when (exception.Number is
                    52310 or
                    52311 or
                    52312 or
                    52313 or
                    52314)
            {
                var errorMessage =
                    exception.Number switch
                    {
                        52310 =>
                            "Invalid OTP verification request.",

                        52311 =>
                            "This OTP has already been used.",

                        52312 =>
                            "OTP has expired. Please request a new OTP.",

                        52313 =>
                            "Maximum OTP attempts exceeded. Request a new OTP.",

                        52314 =>
                            "The entered OTP is incorrect.",

                        _ =>
                            "OTP verification failed."
                    };

                ModelState.AddModelError(
                    nameof(model.OtpCode),
                    errorMessage);

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
                    "Investor registration email OTP verification failed.");

                ModelState.AddModelError(
                    string.Empty,
                    "OTP could not be verified. Please try again.");

                return View(model);
            }
        }

        // Step 3: Verified email ke baad complete profile form.
        [HttpGet]
        public IActionResult CompleteRegistration()
        {
            var challengeValue =
                TempData[EmailOtpChallengeTempDataKey];

            var emailValue =
                TempData[EmailAddressTempDataKey];

            if (challengeValue is null ||
                emailValue is null ||
                !long.TryParse(
                    challengeValue.ToString(),
                    out var challengeId) ||
                challengeId <= 0)
            {
                return RedirectToAction(
                    nameof(CreateAccount));
            }

            var emailAddress =
                emailValue.ToString()
                ?? string.Empty;

            if (string.IsNullOrWhiteSpace(
                    emailAddress))
            {
                return RedirectToAction(
                    nameof(CreateAccount));
            }

            return View(
                new InvestorRegisterViewModel
                {
                    InvestorEmailOtpChallengeID =
                        challengeId,

                    Email =
                        emailAddress.Trim().ToLowerInvariant(),

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

                var emailAddress =
                    model.Email
                        .Trim()
                        .ToLowerInvariant();

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
                        // Current Email OTP flow
                        InvestorEmailOtpChallengeID =
                            model.InvestorEmailOtpChallengeID,

                        // Future Mobile OTP flow
                        InvestorOtpChallengeID =
                            null,

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
                            emailAddress,

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
                when (exception.Number is >= 51010 and <= 51021)
            {
                var errorMessage =
                    exception.Number switch
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
                            "Invalid mobile OTP verification request.",

                        51015 =>
                            "This mobile OTP has already been used.",

                        51016 =>
                            "Mobile number has not been verified.",

                        51017 =>
                            "Mobile OTP verification has expired.",

                        51018 =>
                            "Invalid email OTP verification request.",

                        51019 =>
                            "This email OTP has already been used.",

                        51020 =>
                            "Email address has not been verified.",

                        51021 =>
                            "Email OTP verification has expired.",

                        _ =>
                            "Account registration failed."
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

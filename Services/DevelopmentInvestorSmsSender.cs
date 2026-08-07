using MoneyMiners.Models;
using System.Diagnostics;

namespace MoneyMiners.Services
{
    public sealed class DevelopmentInvestorSmsSender
        : IInvestorSmsSender
    {
        private readonly ILogger<DevelopmentInvestorSmsSender> _logger;
        private readonly IWebHostEnvironment _environment;

        public DevelopmentInvestorSmsSender(
            ILogger<DevelopmentInvestorSmsSender> logger,
            IWebHostEnvironment environment)
        {
            _logger = logger;
            _environment = environment;
        }

        public Task SendOtpAsync(
            string phoneNumber,
            string otpCode,
            InvestorOtpPurpose purpose,
            TimeSpan validity,
            CancellationToken cancellationToken = default)
        {
            if (!_environment.IsDevelopment())
            {
                throw new InvalidOperationException(
                    "Development SMS sender cannot be used in production.");
            }

            var maskedPhoneNumber =
                phoneNumber.Length >= 4
                    ? $"******{phoneNumber[^4..]}"
                    : "****";

            _logger.LogWarning(
                """
                DEVELOPMENT OTP
                Mobile: {MaskedPhoneNumber}
                Purpose: {Purpose}
                OTP: {OtpCode}
                Valid for: {ValidityMinutes} minutes
                """,
                maskedPhoneNumber,
                purpose,
                otpCode,
                validity.TotalMinutes);
            Debug.WriteLine(
              $"DEVELOPMENT OTP: {otpCode}");

            return Task.CompletedTask;
        }
    }
}
using MoneyMiners.Models;
using MoneyMiners.Repositories;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;

namespace MoneyMiners.Services
{
    public sealed class InvestorEmailOtpService
        : IInvestorEmailOtpService
    {
        private static readonly TimeSpan OtpValidity =
            TimeSpan.FromMinutes(5);

        private readonly IInvestorEmailOtpRepository
            _emailOtpRepository;

        private readonly IEmailSender
            _emailSender;

        private readonly ISensitiveDataProtector
            _dataProtector;


        public InvestorEmailOtpService(
            IInvestorEmailOtpRepository emailOtpRepository,
            IEmailSender emailSender,
            ISensitiveDataProtector dataProtector)
        {
            _emailOtpRepository =
                emailOtpRepository;

            _emailSender =
                emailSender;

            _dataProtector =
                dataProtector;
        }


        public async Task<InvestorEmailOtpChallengeResult> SendAsync(
            string emailAddress,
            InvestorOtpPurpose purpose,
            CancellationToken cancellationToken = default)
        {
            var normalizedEmailAddress =
                NormalizeEmailAddress(
                    emailAddress);

            var otpCode =
                RandomNumberGenerator
                    .GetInt32(
                        100000,
                        1000000)
                    .ToString();

            var otpHash =
                ComputeOtpHash(
                    normalizedEmailAddress,
                    purpose,
                    otpCode);

            var expiresAtUtc =
                DateTime.UtcNow.Add(
                    OtpValidity);


            var challenge =
                await _emailOtpRepository
                    .CreateAsync(
                        normalizedEmailAddress,
                        purpose,
                        otpHash,
                        expiresAtUtc,
                        cancellationToken);


            await _emailSender
                .SendOtpAsync(
                    normalizedEmailAddress,
                    otpCode,
                    GetPurposeValue(
                        purpose),
                    OtpValidity,
                    cancellationToken);


            return challenge;
        }


        public async Task<InvestorEmailOtpVerificationResult> VerifyAsync(
            long investorEmailOtpChallengeId,
            string emailAddress,
            InvestorOtpPurpose purpose,
            string otpCode,
            CancellationToken cancellationToken = default)
        {
            if (investorEmailOtpChallengeId <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(investorEmailOtpChallengeId),
                    "Invalid OTP challenge.");
            }


            var normalizedEmailAddress =
                NormalizeEmailAddress(
                    emailAddress);

            var normalizedOtpCode =
                NormalizeOtpCode(
                    otpCode);


            var otpHash =
                ComputeOtpHash(
                    normalizedEmailAddress,
                    purpose,
                    normalizedOtpCode);


            return await _emailOtpRepository
                .VerifyAsync(
                    investorEmailOtpChallengeId,
                    normalizedEmailAddress,
                    purpose,
                    otpHash,
                    cancellationToken);
        }


        private byte[] ComputeOtpHash(
            string emailAddress,
            InvestorOtpPurpose purpose,
            string otpCode)
        {
            var purposeValue =
                GetPurposeValue(
                    purpose);

            var hashInput =
                $"InvestorEmailOTP|{purposeValue}|{emailAddress}|{otpCode}";


            return _dataProtector
                .ComputeHash(
                    hashInput);
        }


        private static string GetPurposeValue(
            InvestorOtpPurpose purpose)
        {
            return purpose switch
            {
                InvestorOtpPurpose.Registration =>
                    "Registration",

                InvestorOtpPurpose.PasswordReset =>
                    "PasswordReset",

                _ =>
                    throw new ArgumentOutOfRangeException(
                        nameof(purpose),
                        purpose,
                        "Unsupported OTP purpose.")
            };
        }


        private static string NormalizeEmailAddress(
            string emailAddress)
        {
            if (string.IsNullOrWhiteSpace(
                    emailAddress))
            {
                throw new ArgumentException(
                    "Email address is required.",
                    nameof(emailAddress));
            }


            var normalized =
                emailAddress
                    .Trim()
                    .ToLowerInvariant();


            var emailValidator =
                new EmailAddressAttribute();


            if (normalized.Length > 320 ||
                !emailValidator.IsValid(
                    normalized))
            {
                throw new ArgumentException(
                    "Enter a valid email address.",
                    nameof(emailAddress));
            }


            return normalized;
        }


        private static string NormalizeOtpCode(
            string otpCode)
        {
            if (string.IsNullOrWhiteSpace(
                    otpCode))
            {
                throw new ArgumentException(
                    "OTP is required.",
                    nameof(otpCode));
            }


            var normalized =
                otpCode.Trim();


            if (normalized.Length != 6 ||
                normalized.Any(
                    character =>
                        !char.IsDigit(
                            character)))
            {
                throw new ArgumentException(
                    "Enter a valid 6-digit OTP.",
                    nameof(otpCode));
            }


            return normalized;
        }
    }
}
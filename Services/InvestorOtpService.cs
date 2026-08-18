using MoneyMiners.Models;
using MoneyMiners.Repositories;
using System.Security.Cryptography;

namespace MoneyMiners.Services
{
    public sealed class InvestorOtpService
        : IInvestorOtpService
    {
        private static readonly TimeSpan OtpValidity =
            TimeSpan.FromMinutes(5);

        private readonly IInvestorOtpRepository _otpRepository;
        private readonly ISmsSender _smsSender;
        private readonly ISensitiveDataProtector _dataProtector;


        public InvestorOtpService(
            IInvestorOtpRepository otpRepository,
            ISmsSender smsSender,
            ISensitiveDataProtector dataProtector)
        {
            _otpRepository = otpRepository;
            _smsSender = smsSender;
            _dataProtector = dataProtector;
        }


        public async Task<InvestorOtpChallengeResult> SendAsync(
            string phoneNumber,
            InvestorOtpPurpose purpose,
            CancellationToken cancellationToken = default)
        {
            var normalizedPhoneNumber =
                NormalizePhoneNumber(phoneNumber);

            var otpCode =
                RandomNumberGenerator
                    .GetInt32(100000, 1000000)
                    .ToString();

            var otpHash =
                ComputeOtpHash(
                    normalizedPhoneNumber,
                    purpose,
                    otpCode);

            var expiresAtUtc =
                DateTime.UtcNow.Add(
                    OtpValidity);

            var challenge =
                await _otpRepository
                    .CreateAsync(
                        normalizedPhoneNumber,
                        purpose,
                        otpHash,
                        expiresAtUtc,
                        cancellationToken);

            await _smsSender
                .SendOtpAsync(
                    normalizedPhoneNumber,
                    otpCode,
                    GetPurposeValue(purpose),
                    OtpValidity,
                    cancellationToken);

            return challenge;
        }


        public async Task<InvestorOtpVerificationResult> VerifyAsync(
            long investorOtpChallengeId,
            string phoneNumber,
            InvestorOtpPurpose purpose,
            string otpCode,
            CancellationToken cancellationToken = default)
        {
            if (investorOtpChallengeId <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(investorOtpChallengeId),
                    "Invalid OTP challenge.");
            }

            var normalizedPhoneNumber =
                NormalizePhoneNumber(
                    phoneNumber);

            var normalizedOtpCode =
                NormalizeOtpCode(
                    otpCode);

            var otpHash =
                ComputeOtpHash(
                    normalizedPhoneNumber,
                    purpose,
                    normalizedOtpCode);

            return await _otpRepository
                .VerifyAsync(
                    investorOtpChallengeId,
                    normalizedPhoneNumber,
                    purpose,
                    otpHash,
                    cancellationToken);
        }


        private byte[] ComputeOtpHash(
            string phoneNumber,
            InvestorOtpPurpose purpose,
            string otpCode)
        {
            var purposeValue =
                GetPurposeValue(
                    purpose);

            var hashInput =
                $"InvestorOTP|{purposeValue}|{phoneNumber}|{otpCode}";

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


        private static string NormalizePhoneNumber(
            string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(
                    phoneNumber))
            {
                throw new ArgumentException(
                    "Mobile number is required.",
                    nameof(phoneNumber));
            }

            var normalized =
                new string(
                    phoneNumber
                        .Where(char.IsDigit)
                        .ToArray());

            if (normalized.Length != 10)
            {
                throw new ArgumentException(
                    "Enter a valid 10-digit mobile number.",
                    nameof(phoneNumber));
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
                        !char.IsDigit(character)))
            {
                throw new ArgumentException(
                    "Enter a valid 6-digit OTP.",
                    nameof(otpCode));
            }

            return normalized;
        }
    }
}
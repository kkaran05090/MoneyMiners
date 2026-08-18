using MoneyMiners.Models;
using MoneyMiners.Repositories;
using System.Security.Cryptography;

namespace MoneyMiners.Services
{
    public sealed class AdminPasswordResetOtpService
        : IAdminPasswordResetOtpService
    {
        private static readonly TimeSpan OtpValidity =
            TimeSpan.FromMinutes(5);

        private readonly IAdminPasswordResetOtpRepository
            _otpRepository;

        private readonly ISmsSender
            _smsSender;

        private readonly ISensitiveDataProtector
            _dataProtector;


        public AdminPasswordResetOtpService(
            IAdminPasswordResetOtpRepository otpRepository,
            ISmsSender smsSender,
            ISensitiveDataProtector dataProtector)
        {
            _otpRepository =
                otpRepository;

            _smsSender =
                smsSender;

            _dataProtector =
                dataProtector;
        }


        public async Task<AdminPasswordResetOtpChallengeResult> SendAsync(
            long adminUserId,
            string phoneNumber,
            CancellationToken cancellationToken = default)
        {
            if (adminUserId <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(adminUserId));
            }

            var normalizedPhoneNumber =
                NormalizePhoneNumber(
                    phoneNumber);

            var otpCode =
                RandomNumberGenerator
                    .GetInt32(100000, 1000000)
                    .ToString();

            var otpHash =
                ComputeOtpHash(
                    adminUserId,
                    normalizedPhoneNumber,
                    otpCode);

            var expiresAtUtc =
                DateTime.UtcNow.Add(
                    OtpValidity);

            var challenge =
                await _otpRepository
                    .CreateAsync(
                        adminUserId,
                        normalizedPhoneNumber,
                        otpHash,
                        expiresAtUtc,
                        cancellationToken);

            await _smsSender
                .SendOtpAsync(
                    normalizedPhoneNumber,
                    otpCode,
                    "AdminPasswordReset",
                    OtpValidity,
                    cancellationToken);

            return challenge;
        }


        public async Task<AdminPasswordResetOtpVerificationResult> VerifyAsync(
            long challengeId,
            long adminUserId,
            string phoneNumber,
            string otpCode,
            CancellationToken cancellationToken = default)
        {
            if (challengeId <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(challengeId));
            }

            if (adminUserId <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(adminUserId));
            }

            var normalizedPhoneNumber =
                NormalizePhoneNumber(
                    phoneNumber);

            var normalizedOtpCode =
                NormalizeOtpCode(
                    otpCode);

            var otpHash =
                ComputeOtpHash(
                    adminUserId,
                    normalizedPhoneNumber,
                    normalizedOtpCode);

            return await _otpRepository
                .VerifyAsync(
                    challengeId,
                    adminUserId,
                    normalizedPhoneNumber,
                    otpHash,
                    cancellationToken);
        }


        private byte[] ComputeOtpHash(
            long adminUserId,
            string phoneNumber,
            string otpCode)
        {
            var hashInput =
                $"AdminPasswordResetOTP|{adminUserId}|{phoneNumber}|{otpCode}";

            return _dataProtector
                .ComputeHash(
                    hashInput);
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
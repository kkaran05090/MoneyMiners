using MoneyMiners.Models;
using MoneyMiners.Repositories;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;

namespace MoneyMiners.Services
{
    public sealed class AdminPasswordResetEmailOtpService
        : IAdminPasswordResetEmailOtpService
    {
        private static readonly TimeSpan OtpValidity =
            TimeSpan.FromMinutes(5);

        private readonly IAdminPasswordResetEmailOtpRepository
            _otpRepository;

        private readonly IEmailSender
            _emailSender;

        private readonly ISensitiveDataProtector
            _dataProtector;


        public AdminPasswordResetEmailOtpService(
            IAdminPasswordResetEmailOtpRepository otpRepository,
            IEmailSender emailSender,
            ISensitiveDataProtector dataProtector)
        {
            _otpRepository =
                otpRepository;

            _emailSender =
                emailSender;

            _dataProtector =
                dataProtector;
        }


        public async Task<AdminPasswordResetEmailOtpChallengeResult> SendAsync(
            long adminUserId,
            string emailAddress,
            CancellationToken cancellationToken = default)
        {
            if (adminUserId <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(adminUserId));
            }

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
                    adminUserId,
                    normalizedEmailAddress,
                    otpCode);

            var expiresAtUtc =
                DateTime.UtcNow.Add(
                    OtpValidity);

            var challenge =
                await _otpRepository.CreateAsync(
                    adminUserId,
                    normalizedEmailAddress,
                    otpHash,
                    expiresAtUtc,
                    cancellationToken);

            await _emailSender.SendOtpAsync(
                normalizedEmailAddress,
                otpCode,
                "PasswordReset",
                OtpValidity,
                cancellationToken);

            return challenge;
        }


        public async Task<AdminPasswordResetEmailOtpVerificationResult> VerifyAsync(
            long challengeId,
            long adminUserId,
            string emailAddress,
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

            var normalizedEmailAddress =
                NormalizeEmailAddress(
                    emailAddress);

            var normalizedOtpCode =
                NormalizeOtpCode(
                    otpCode);

            var otpHash =
                ComputeOtpHash(
                    adminUserId,
                    normalizedEmailAddress,
                    normalizedOtpCode);

            return await _otpRepository.VerifyAsync(
                challengeId,
                adminUserId,
                normalizedEmailAddress,
                otpHash,
                cancellationToken);
        }


        private byte[] ComputeOtpHash(
            long adminUserId,
            string emailAddress,
            string otpCode)
        {
            var hashInput =
                $"AdminPasswordResetEmailOTP|{adminUserId}|{emailAddress}|{otpCode}";

            return _dataProtector.ComputeHash(
                hashInput);
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

            if (normalized.Length > 256 ||
                !new EmailAddressAttribute()
                    .IsValid(normalized))
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
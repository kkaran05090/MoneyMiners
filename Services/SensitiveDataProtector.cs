using Microsoft.AspNetCore.DataProtection;
using System.Security.Cryptography;
using System.Text;

namespace MoneyMiners.Services
{
    public sealed class SensitiveDataProtector
        : ISensitiveDataProtector
    {
        private readonly IDataProtector _dataProtector;
        private readonly byte[] _hashKey;

        public SensitiveDataProtector(
            IDataProtectionProvider dataProtectionProvider,
            IConfiguration configuration)
        {
            _dataProtector =
                dataProtectionProvider.CreateProtector(
                    "MoneyMiners.InvestorSensitiveData.v1");

            var configuredHashKey =
                configuration[
                    "Security:SensitiveDataHashKey"];

            if (string.IsNullOrWhiteSpace(
                configuredHashKey))
            {
                throw new InvalidOperationException(
                    "Security:SensitiveDataHashKey is missing.");
            }

            try
            {
                _hashKey =
                    Convert.FromBase64String(
                        configuredHashKey);
            }
            catch (FormatException exception)
            {
                throw new InvalidOperationException(
                    "Security:SensitiveDataHashKey must be a valid Base64 value.",
                    exception);
            }

            if (_hashKey.Length < 32)
            {
                throw new InvalidOperationException(
                    "SensitiveDataHashKey must contain at least 32 bytes.");
            }
        }

        public byte[] Protect(string plainText)
        {
            if (string.IsNullOrWhiteSpace(plainText))
            {
                throw new ArgumentException(
                    "Value cannot be empty.",
                    nameof(plainText));
            }

            var protectedText =
                _dataProtector.Protect(
                    plainText.Trim());

            return Encoding.UTF8.GetBytes(
                protectedText);
        }

        public string Unprotect(
            byte[] protectedData)
        {
            if (protectedData is null ||
                protectedData.Length == 0)
            {
                throw new ArgumentException(
                    "Protected data cannot be empty.",
                    nameof(protectedData));
            }

            var protectedText =
                Encoding.UTF8.GetString(
                    protectedData);

            return _dataProtector.Unprotect(
                protectedText);
        }

        public byte[] ComputeHash(
            string normalizedValue)
        {
            if (string.IsNullOrWhiteSpace(
                normalizedValue))
            {
                throw new ArgumentException(
                    "Value cannot be empty.",
                    nameof(normalizedValue));
            }

            using var hmac =
                new HMACSHA256(_hashKey);

            return hmac.ComputeHash(
                Encoding.UTF8.GetBytes(
                    normalizedValue.Trim()));
        }
    }
}

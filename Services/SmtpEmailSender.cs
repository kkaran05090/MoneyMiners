using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace MoneyMiners.Services
{
    public sealed class SmtpEmailSender : IEmailSender
    {
        private static readonly HttpClient HttpClient = new();

        private readonly IConfiguration _configuration;
        private readonly ILogger<SmtpEmailSender> _logger;

        public SmtpEmailSender(
            IConfiguration configuration,
            ILogger<SmtpEmailSender> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendOtpAsync(
            string emailAddress,
            string otpCode,
            string purpose,
            TimeSpan validity,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(emailAddress))
            {
                throw new ArgumentException(
                    "Email address is required.",
                    nameof(emailAddress));
            }

            if (string.IsNullOrWhiteSpace(otpCode))
            {
                throw new ArgumentException(
                    "OTP code is required.",
                    nameof(otpCode));
            }

            var apiKey =
                _configuration["Resend:ApiKey"];

            var senderEmail =
                _configuration["Resend:SenderEmail"];

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new InvalidOperationException(
                    "Resend API key is not configured.");
            }

            if (string.IsNullOrWhiteSpace(senderEmail))
            {
                throw new InvalidOperationException(
                    "Resend sender email is not configured.");
            }

            var subject =
                purpose == "PasswordReset"
                    ? "Money Miners - Password Reset OTP"
                    : "Money Miners - Email Verification OTP";

            var validityMinutes =
                Math.Max(
                    1,
                    (int)Math.Ceiling(
                        validity.TotalMinutes));

            var textBody =
                $"Your Money Miners OTP is {otpCode}. " +
                $"It is valid for {validityMinutes} minute(s). " +
                "Do not share this OTP with anyone.";

            var htmlBody =
                $"""
                <div style="font-family:Arial,sans-serif;
                            max-width:600px;
                            margin:auto;
                            padding:24px;">

                    <h2>Money Miners</h2>

                    <p>Your verification OTP is:</p>

                    <h1 style="letter-spacing:6px;">
                        {otpCode}
                    </h1>

                    <p>
                        This OTP is valid for
                        {validityMinutes} minute(s).
                    </p>

                    <p>
                        Do not share this OTP with anyone.
                    </p>

                </div>
                """;

            var payload = new
            {
                from = $"Money Miners <{senderEmail}>",
                to = new[] { emailAddress.Trim() },
                subject,
                text = textBody,
                html = htmlBody
            };

            var json =
                JsonSerializer.Serialize(payload);

            using var request =
                new HttpRequestMessage(
                    HttpMethod.Post,
                    "https://api.resend.com/emails");

            request.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    apiKey);

            request.Content =
                new StringContent(
                    json,
                    Encoding.UTF8,
                    "application/json");

            try
            {
                using var response =
                    await HttpClient.SendAsync(
                        request,
                        cancellationToken);

                var responseBody =
                    await response.Content.ReadAsStringAsync(
                        cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError(
                        "Resend email delivery failed. Status: {StatusCode}. Response: {Response}",
                        response.StatusCode,
                        responseBody);

                    throw new InvalidOperationException(
                        "OTP email could not be sent.");
                }
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Email OTP delivery failed.");

                throw;
            }
        }
    }
}
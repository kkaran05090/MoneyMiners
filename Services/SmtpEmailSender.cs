using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace MoneyMiners.Services
{
    public sealed class SmtpEmailSender : IEmailSender
    {
        private readonly EmailSettings _settings;
        private readonly ILogger<SmtpEmailSender> _logger;

        public SmtpEmailSender(
            IOptions<EmailSettings> options,
            ILogger<SmtpEmailSender> logger)
        {
            _settings = options.Value;
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

            var message = new MimeMessage();

            message.From.Add(
                new MailboxAddress(
                    _settings.SenderName,
                    _settings.SenderEmail));

            message.To.Add(
                MailboxAddress.Parse(
                    emailAddress.Trim()));

            message.Subject =
                purpose == "PasswordReset"
                    ? "Money Miners - Password Reset OTP"
                    : "Money Miners - Email Verification OTP";

            var validityMinutes =
                Math.Max(
                    1,
                    (int)Math.Ceiling(
                        validity.TotalMinutes));

            var bodyBuilder =
                new BodyBuilder
                {
                    TextBody =
                        $"Your Money Miners OTP is {otpCode}. " +
                        $"It is valid for {validityMinutes} minute(s). " +
                        "Do not share this OTP with anyone.",

                    HtmlBody =
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
                        """
                };

            message.Body =
                bodyBuilder.ToMessageBody();

            using var smtpClient =
                new SmtpClient();

            try
            {
                var securityOption =
                    _settings.EnableSsl
                        ? SecureSocketOptions.StartTls
                        : SecureSocketOptions.None;

                await smtpClient.ConnectAsync(
                    _settings.SmtpHost,
                    _settings.SmtpPort,
                    securityOption,
                    cancellationToken);

                await smtpClient.AuthenticateAsync(
                    _settings.Username,
                    _settings.Password,
                    cancellationToken);

                await smtpClient.SendAsync(
                    message,
                    cancellationToken);

                await smtpClient.DisconnectAsync(
                    true,
                    cancellationToken);
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
namespace MoneyMiners.Services
{
    public interface IEmailSender
    {
        Task SendOtpAsync(
            string emailAddress,
            string otpCode,
            string purpose,
            TimeSpan validity,
            CancellationToken cancellationToken = default);
    }
}
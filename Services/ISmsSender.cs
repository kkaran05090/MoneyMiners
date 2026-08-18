namespace MoneyMiners.Services
{
    public interface ISmsSender
    {
        Task SendOtpAsync(
            string phoneNumber,
            string otpCode,
            string purpose,
            TimeSpan validity,
            CancellationToken cancellationToken = default);
    }
}
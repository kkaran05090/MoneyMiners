using MoneyMiners.Models;

namespace MoneyMiners.Services
{
    public interface IInvestorSmsSender
    {
        Task SendOtpAsync(
            string phoneNumber,
            string otpCode,
            InvestorOtpPurpose purpose,
            TimeSpan validity,
            CancellationToken cancellationToken = default);
    }
}
namespace MoneyMiners.Services
{
    public interface ISensitiveDataProtector
    {
        byte[] Protect(string plainText);

        string Unprotect(byte[] protectedData);

        byte[] ComputeHash(string normalizedValue);
    }
}

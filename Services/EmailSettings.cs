namespace MoneyMiners.Services
{
    public sealed class EmailSettings
    {
        public const string SectionName = "Email";

        public string SmtpHost { get; set; } = string.Empty;

        public int SmtpPort { get; set; }

        public string SenderName { get; set; } = string.Empty;

        public string SenderEmail { get; set; } = string.Empty;

        public string Username { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public bool EnableSsl { get; set; } = true;
    }
}
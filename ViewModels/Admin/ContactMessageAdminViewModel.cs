namespace MoneyMiners.ViewModels.Admin
{
    public class ContactMessageAdminViewModel
    {
        public long ContactMessageID { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        public string Subject { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public byte[] RowVersion { get; set; } = Array.Empty<byte>();

        public string RowVersionBase64 =>
            Convert.ToBase64String(RowVersion);

        public int TotalRecords { get; set; }
    }
}
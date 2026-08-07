namespace MoneyMiners.ViewModels.Admin
{
    public sealed class AdminUserListItemViewModel
    {
        public long AdminUserID { get; set; }

        public string Username { get; set; }
            = string.Empty;

        public string Email { get; set; }
            = string.Empty;

        public string Role { get; set; }
            = string.Empty;

        public bool IsActive { get; set; }

        public int FailedLoginCount { get; set; }

        public DateTime? LockoutEndUtc { get; set; }

        public DateTime? LastLoginAtUtc { get; set; }

        public DateTime CreatedAtUtc { get; set; }

        public DateTime? UpdatedAtUtc { get; set; }

        public byte[] RowVersion { get; set; }
            = Array.Empty<byte>();

        public string RowVersionBase64 =>
            Convert.ToBase64String(RowVersion);
    }
}
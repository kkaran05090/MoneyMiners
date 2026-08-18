namespace MoneyMiners.Models
{
    public sealed class AdminUser
    {
        public long AdminUserID { get; set; }

        public string Username { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }

        public bool IsMobileVerified { get; set; }

        public bool IsActive { get; set; }

        public int FailedLoginCount { get; set; }

        public DateTime? LockoutEndUtc { get; set; }

        public DateTime? LastLoginAtUtc { get; set; }

        public DateTime? PasswordChangedAtUtc { get; set; }

        public Guid SecurityStamp { get; set; }

        public DateTime CreatedAtUtc { get; set; }

        public DateTime? UpdatedAtUtc { get; set; }

        public byte[] RowVersion { get; set; } =
            Array.Empty<byte>();
    }
}

namespace MoneyMiners.Models
{
    public sealed class Notification
    {
        public long NotificationID { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public bool IsPublished { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime CreatedAtUtc { get; set; }

        public DateTime? PublishedAtUtc { get; set; }
    }
}

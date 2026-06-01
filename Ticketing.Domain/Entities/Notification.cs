using System;
using System.ComponentModel.DataAnnotations;

namespace Ticketing.Domain.Entities
{
    public class Notification
    {
        [Key]
        public Guid NotificationId { get; set; }

        public string Operation { get; set; } = null!;
        public DateTime Timestamp { get; set; }
        public string Parameters { get; set; } = null!; // JSON
        public string Content { get; set; } = null!; // JSON or plain
        public string Status { get; set; } = "Queued"; // Queued, InProgress, Sent, Failed
        public string? Result { get; set; }
    }
}

using System;

namespace Ticketing.Services.DTOs
{
    public class NotificationDto
    {
        public Guid NotificationId { get; set; }
        public string Operation { get; set; } = null!;
        public DateTime Timestamp { get; set; }
        public string Parameters { get; set; } = null!;
        public string Content { get; set; } = null!;
        public string Status { get; set; } = null!;
    }
}

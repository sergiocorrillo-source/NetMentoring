using System;

namespace Ticketing.Domain.Entities
{
    public class Customer
    {
        public Guid CustomerId { get; set; }
        public string Email { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

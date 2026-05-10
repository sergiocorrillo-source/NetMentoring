using System;

namespace Ticketing.Services.DTOs
{
    public class PaymentDto
    {
        public Guid PaymentId { get; set; }
        public string Status { get; set; } = null!;
        public decimal Amount { get; set; }
    }
}

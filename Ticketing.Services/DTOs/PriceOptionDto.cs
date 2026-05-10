using System;

namespace Ticketing.Services.DTOs
{
    public class PriceOptionDto
    {
        public Guid PriceId { get; set; }
        public string Name { get; set; } = null!;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = null!;
    }
}

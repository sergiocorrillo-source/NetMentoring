using System;

namespace Ticketing.Services.DTOs
{
    public class CartItemDto
    {
        public Guid EventId { get; set; }
        public Guid SeatId { get; set; }
        public Guid PriceId { get; set; }
        public string SeatDescription { get; set; } = null!;
        public decimal Price { get; set; }
    }
}

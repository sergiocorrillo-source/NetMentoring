using System;

namespace Ticketing.Services.DTOs
{
    public class AddToCartRequestDto
    {
        public Guid EventId { get; set; }
        public Guid SeatId { get; set; }
        public Guid PriceId { get; set; }
    }
}

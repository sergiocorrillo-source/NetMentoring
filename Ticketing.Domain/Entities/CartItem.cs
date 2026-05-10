using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ticketing.Domain.Entities
{
    public class CartItem
    {
        [Key]
        public Guid CartItemId { get; set; }

        public Guid CartId { get; set; }
        public Guid EventId { get; set; }
        public Guid SeatId { get; set; }
        public Guid PriceId { get; set; }

        [ForeignKey(nameof(CartId))]
        public Cart? Cart { get; set; }

        [ForeignKey(nameof(EventId))]
        public Event? Event { get; set; }

        [ForeignKey(nameof(SeatId))]
        public Seat? Seat { get; set; }

        [ForeignKey(nameof(PriceId))]
        public Price? Price { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

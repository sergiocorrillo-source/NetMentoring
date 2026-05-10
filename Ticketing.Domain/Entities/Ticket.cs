using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ticketing.Domain.Entities
{
    public class Ticket
    {
        [Key]
        public Guid TicketId { get; set; }

        public Guid EventId { get; set; }
        public Guid SeatId { get; set; }
        public Guid? CustomerId { get; set; }
        public Guid OfferId { get; set; }

        [Required, MaxLength(50)]
        public string Status { get; set; } = "Created";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(EventId))]
        public Event? Event { get; set; }

        [ForeignKey(nameof(SeatId))]
        public Seat? Seat { get; set; }

        [ForeignKey(nameof(CustomerId))]
        public Customer? Customer { get; set; }

        [ForeignKey(nameof(OfferId))]
        public Offer? Offer { get; set; }
    }
}

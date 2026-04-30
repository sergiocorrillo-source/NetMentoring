using System;

namespace Ticketing.Domain.Entities
{
    public class Ticket
    {
        public Guid TicketId { get; set; }
        public Guid EventId { get; set; }
        public Guid SeatId { get; set; }
        public Guid? CustomerId { get; set; }
        public Guid OfferId { get; set; }
        public string Status { get; set; } = "Created";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Event? Event { get; set; }
        public Seat? Seat { get; set; }
        public Customer? Customer { get; set; }
        public Offer? Offer { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ticketing.Domain.Entities
{
    public class Offer
    {
        [Key]
        public Guid OfferId { get; set; }

        public Guid EventId { get; set; }
        public Guid PriceId { get; set; }

        [MaxLength(500)]
        public string Description { get; set; } = null!;

        public bool IsActive { get; set; } = true;

        [ForeignKey(nameof(EventId))]
        public Event? Event { get; set; }

        [ForeignKey(nameof(PriceId))]
        public Price? Price { get; set; }

        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    }
}

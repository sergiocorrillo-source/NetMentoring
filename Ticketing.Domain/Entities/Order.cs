using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ticketing.Domain.Entities
{
    public class Order
    {
        [Key]
        public Guid OrderId { get; set; }

        public Guid CustomerId { get; set; }
        public Guid EventId { get; set; }

        [Required, MaxLength(20)]
        public string OrderStatus { get; set; } = "Created"; // Created | PendingPayment | Paid | Cancelled

        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalAmount { get; set; }

        [Required, MaxLength(10)]
        public string Currency { get; set; } = "USD";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(CustomerId))]
        public Customer? Customer { get; set; }

        [ForeignKey(nameof(EventId))]
        public Event? Event { get; set; }

        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    }
}

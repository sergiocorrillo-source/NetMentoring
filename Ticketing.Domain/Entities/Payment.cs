using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Ticketing.Domain.Entities
{
    public class Payment
    {
        [Key]
        public Guid PaymentId { get; set; }

        [Required, MaxLength(50)]
        public string Status { get; set; } = "Pending";

        public decimal Amount { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    }
}

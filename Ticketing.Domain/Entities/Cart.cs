using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Ticketing.Domain.Entities
{
    public class Cart
    {
        [Key]
        public Guid CartId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
    }
}

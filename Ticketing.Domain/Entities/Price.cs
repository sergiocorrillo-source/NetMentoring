using System;
using System.ComponentModel.DataAnnotations;

namespace Ticketing.Domain.Entities
{
    public class Price
    {
        [Key]
        public Guid PriceId { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = null!;

        public decimal Amount { get; set; }

        [Required, MaxLength(8)]
        public string Currency { get; set; } = "EUR";
    }
}

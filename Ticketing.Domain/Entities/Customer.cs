using System;
using System.ComponentModel.DataAnnotations;

namespace Ticketing.Domain.Entities
{
    public class Customer
    {
        [Key]
        public Guid CustomerId { get; set; }

        [Required, EmailAddress, MaxLength(200)]
        public string Email { get; set; } = null!;

        [Required, MaxLength(200)]
        public string FullName { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

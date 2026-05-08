using System;
using System.ComponentModel.DataAnnotations;

namespace Ticketing.Domain.Entities
{
    public class EventManager
    {
        [Key]
        public Guid EventManagerId { get; set; }

        [Required, EmailAddress, MaxLength(200)]
        public string Email { get; set; } = null!;

        [Required, MaxLength(200)]
        public string FullName { get; set; } = null!;
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Ticketing.Domain.Entities
{
    public class Venue
    {
        [Key]
        public Guid VenueId { get; set; }

        [Required, MaxLength(200)]
        public string Name { get; set; } = null!;

        [Required, MaxLength(500)]
        public string Address { get; set; } = null!;

        public ICollection<SeatManifest> SeatManifests { get; set; } = new List<SeatManifest>();
    }
}

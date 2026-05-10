using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ticketing.Domain.Entities
{
    public class SeatManifest
    {
        [Key]
        public Guid SeatManifestId { get; set; }

        public Guid VenueId { get; set; }

        [MaxLength(500)]
        public string Description { get; set; } = null!;

        [ForeignKey(nameof(VenueId))]
        public Venue? Venue { get; set; }

        public ICollection<Seat> Seats { get; set; } = new List<Seat>();
    }
}

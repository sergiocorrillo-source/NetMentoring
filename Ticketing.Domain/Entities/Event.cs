using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ticketing.Domain.Entities
{
    public class Event
    {
        [Key]
        public Guid EventId { get; set; }

        public Guid VenueId { get; set; }
        public Guid SeatManifestId { get; set; }

        [Required, MaxLength(200)]
        public string Name { get; set; } = null!;

        public DateTime EventDate { get; set; }
        public TimeSpan EventTime { get; set; }

        [Required, MaxLength(100)]
        public string CreatedBy { get; set; } = null!;

        [ForeignKey(nameof(VenueId))]
        public Venue? Venue { get; set; }

        [ForeignKey(nameof(SeatManifestId))]
        public SeatManifest? SeatManifest { get; set; }

        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    }
}

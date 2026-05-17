using System;

namespace Ticketing.Services.DTOs
{
    public class EventDto
    {
        public Guid EventId { get; set; }
        public string Name { get; set; } = null!;
        public DateTime EventDate { get; set; }
        public TimeSpan EventTime { get; set; }
        public Guid VenueId { get; set; }
    }
}

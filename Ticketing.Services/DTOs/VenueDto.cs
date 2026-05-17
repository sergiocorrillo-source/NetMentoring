using System;

namespace Ticketing.Services.DTOs
{
    public class VenueDto
    {
        public Guid VenueId { get; set; }
        public string Name { get; set; } = null!;
        public string Address { get; set; } = null!;
    }
}

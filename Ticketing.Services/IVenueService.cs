using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ticketing.Services.DTOs;

namespace Ticketing.Services
{
    public interface IVenueService
    {
        Task<IEnumerable<VenueDto>> GetAllVenuesAsync();
        Task<IEnumerable<SectionDto>> GetVenueSectionsAsync(Guid venueId);
    }
}

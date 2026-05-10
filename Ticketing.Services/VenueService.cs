using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ticketing.DAL;
using Ticketing.Domain.Entities;
using Ticketing.Services.DTOs;

namespace Ticketing.Services
{
    public class VenueService
    {
        private readonly IUnitOfWork _uow;

        public VenueService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<IEnumerable<VenueDto>> GetAllVenuesAsync()
        {
            var venues = await _uow.Repository<Venue>().GetAllAsync();
            return venues.Select(v => new VenueDto
            {
                VenueId = v.VenueId,
                Name = v.Name,
                Address = v.Address
            });
        }

        public async Task<IEnumerable<SectionDto>> GetVenueSectionsAsync(Guid venueId)
        {
            var seats = await _uow.Repository<Seat>().FindAsync(s =>
                s.SeatManifest != null && s.SeatManifest.VenueId == venueId);

            var sections = seats
                .GroupBy(s => s.Section)
                .Select(g => new SectionDto
                {
                    Section = g.Key,
                    Rows = g.Select(s => s.RowNumber).Distinct().ToList()
                })
                .ToList();

            return sections;
        }
    }
}

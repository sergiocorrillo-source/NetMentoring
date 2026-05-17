using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ticketing.DAL;
using Ticketing.Domain.Entities;
using Ticketing.Services.DTOs;

namespace Ticketing.Services
{
    public class EventService
    {
        private readonly IUnitOfWork _uow;

        public EventService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<IEnumerable<EventDto>> GetAllEventsAsync()
        {
            var events = await _uow.Repository<Event>().GetAllAsync();
            return events.Select(e => new EventDto
            {
                EventId = e.EventId,
                Name = e.Name,
                EventDate = e.EventDate,
                EventTime = e.EventTime,
                VenueId = e.VenueId
            });
        }

        public async Task<IEnumerable<SeatDto>> GetEventSeatsAsync(Guid eventId, string section)
        {
            var event_ = await _uow.Repository<Event>().GetByIdAsync(eventId);
            if (event_ == null) throw new InvalidOperationException("Event not found.");

            var seats = await _uow.Repository<Seat>()
                .FindAsync(s => s.SeatManifest != null && s.SeatManifest.SeatManifestId == event_.SeatManifestId && s.Section == section);

            return seats.Select(s => new SeatDto
            {
                SeatId = s.SeatId,
                Section = s.Section,
                RowNumber = s.RowNumber,
                SeatNumber = s.SeatNumber,
                Status = s.Status.ToString(),
                SeatType = s.SeatType
            });
        }
    }
}

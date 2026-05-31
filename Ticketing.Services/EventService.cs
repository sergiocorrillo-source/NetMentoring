using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Ticketing.DAL;
using Ticketing.Domain.Entities;
using Ticketing.Services.DTOs;

namespace Ticketing.Services
{
    public class EventService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMemoryCache? _cache;
        private const string EventsCacheKey = "events_all";

        public EventService(IUnitOfWork uow, IMemoryCache? cache = null)
        {
            _uow = uow;
            _cache = cache;
        }

        public async Task<IEnumerable<EventDto>> GetAllEventsAsync()
        {
            if (_cache != null && _cache.TryGetValue<IEnumerable<EventDto>>(EventsCacheKey, out var cached))
            {
                return cached;
            }

            var events = await _uow.Repository<Event>().GetAllAsync();
            var dtos = events.Select(e => new EventDto
            {
                EventId = e.EventId,
                Name = e.Name,
                EventDate = e.EventDate,
                EventTime = e.EventTime,
                VenueId = e.VenueId
            }).ToList();

            if (_cache != null)
            {
                _cache.Set(EventsCacheKey, dtos, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                });
            }

            return dtos;
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

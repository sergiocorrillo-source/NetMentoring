using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ticketing.Services;
using Ticketing.Services.DTOs;

namespace Ticketing.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventsControllerV2 : ControllerBase
    {
        private readonly EventService _eventService;

        public EventsControllerV2(EventService eventService)
        {
            _eventService = eventService;
        }

        /// <summary>
        /// GET /api/events
        /// Obtiene la lista de todos los eventos
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EventDto>>> GetEvents()
        {
            try
            {
                var events = await _eventService.GetAllEventsAsync();
                return Ok(events);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// GET /api/events/{eventId}/sections/{section}/seats
        /// Obtiene la lista de asientos de un evento en una sección específica
        /// </summary>
        [HttpGet("{eventId}/sections/{section}/seats")]
        public async Task<ActionResult<IEnumerable<SeatDto>>> GetEventSeats(Guid eventId, string section)
        {
            try
            {
                var seats = await _eventService.GetEventSeatsAsync(eventId, section);
                return Ok(seats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Ticketing.Services;
using Ticketing.Services.DTOs;

namespace Ticketing.Api.Controllers
{
    [ApiController]
    [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Any, NoStore = false)]
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

                // Compute ETag from serialized payload
                var payload = JsonSerializer.Serialize(events);
                using var sha = SHA256.Create();
                var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(payload));
                var etag = '"' + Convert.ToBase64String(hash) + '"';

                // If client provided If-None-Match and it matches, return 304
                if (Request.Headers.TryGetValue("If-None-Match", out var incoming) && incoming.ToString() == etag)
                {
                    Response.Headers["Cache-Control"] = "public, max-age=300";
                    Response.Headers["ETag"] = etag;
                    return StatusCode(304);
                }

                Response.Headers["Cache-Control"] = "public, max-age=300";
                Response.Headers["ETag"] = etag;

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

                // Compute ETag from serialized payload
                var payload = JsonSerializer.Serialize(seats);
                using var sha = SHA256.Create();
                var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(payload));
                var etag = '"' + Convert.ToBase64String(hash) + '"';

                if (Request.Headers.TryGetValue("If-None-Match", out var incoming) && incoming.ToString() == etag)
                {
                    Response.Headers["Cache-Control"] = "public, max-age=300";
                    Response.Headers["ETag"] = etag;
                    return StatusCode(304);
                }

                Response.Headers["Cache-Control"] = "public, max-age=300";
                Response.Headers["ETag"] = etag;

                return Ok(seats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}

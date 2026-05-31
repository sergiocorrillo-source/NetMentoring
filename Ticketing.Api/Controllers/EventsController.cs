using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Ticketing.Services;

namespace Ticketing.Api.Controllers
{
    [ApiController]
    [Route("api/events")]
    public class EventsController : ControllerBase
    {
        private readonly IReservationService _reservationService;

        public EventsController(IReservationService reservationService)
        {
            _reservationService = reservationService;
        }

        [HttpPost("{eventId}/seats/{seatId}/reserve")]
        public async Task<IActionResult> Reserve(Guid eventId, Guid seatId, [FromQuery] Guid customerId, [FromQuery] Guid offerId)
        {
            try
            {
                var ticketId = await _reservationService.ReserveSeatAsync(eventId, seatId, customerId, offerId);
                return CreatedAtAction(nameof(Reserve), new { ticketId }, new { ticketId });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException)
            {
                return Conflict(new { error = "Concurrency conflict, please retry." });
            }
        }
    }
}

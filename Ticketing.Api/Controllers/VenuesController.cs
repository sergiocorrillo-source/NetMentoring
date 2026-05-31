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
    public class VenuesController : ControllerBase
    {
        private readonly IVenueService _venueService;

        public VenuesController(IVenueService venueService)
        {
            _venueService = venueService;
        }

        /// <summary>
        /// GET /api/venues
        /// Obtiene la lista de todos los recintos
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<VenueDto>>> GetVenues()
        {
            try
            {
                var venues = await _venueService.GetAllVenuesAsync();
                return Ok(venues);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// GET /api/venues/{venueId}/sections
        /// Obtiene todas las secciones del recinto
        /// </summary>
        [HttpGet("{venueId}/sections")]
        public async Task<ActionResult<IEnumerable<SectionDto>>> GetVenueSections(Guid venueId)
        {
            try
            {
                var sections = await _venueService.GetVenueSectionsAsync(venueId);
                return Ok(sections);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}

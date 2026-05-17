using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Ticketing.Services;
using Ticketing.Services.DTOs;

namespace Ticketing.Api.Controllers
{
    [ApiController]
    [Route("api/orders/carts")]
    public class CartsController : ControllerBase
    {
        private readonly CartService _cartService;

        public CartsController(CartService cartService)
        {
            _cartService = cartService;
        }

        /// <summary>
        /// GET /api/orders/carts/{cartId}
        /// Obtiene la lista de artículos en un carrito
        /// </summary>
        [HttpGet("{cartId}")]
        public async Task<ActionResult<CartDto>> GetCart(Guid cartId)
        {
            try
            {
                var cart = await _cartService.GetCartAsync(cartId);
                return Ok(cart);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// POST /api/orders/carts/{cartId}
        /// Añade un asiento al carrito
        /// </summary>
        [HttpPost("{cartId}")]
        public async Task<ActionResult<CartDto>> AddToCart(Guid cartId, [FromBody] AddToCartRequestDto request)
        {
            try
            {
                var cart = await _cartService.AddToCartAsync(cartId, request);
                return Ok(cart);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// DELETE /api/orders/carts/{cartId}/events/{eventId}/seats/{seatId}
        /// Elimina un asiento de un carrito
        /// </summary>
        [HttpDelete("{cartId}/events/{eventId}/seats/{seatId}")]
        public async Task<IActionResult> RemoveFromCart(Guid cartId, Guid eventId, Guid seatId)
        {
            try
            {
                await _cartService.RemoveFromCartAsync(cartId, eventId, seatId);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// PUT /api/orders/carts/{cartId}/book
        /// Cambia el estado de todos los asientos del carrito a reservado
        /// </summary>
        [HttpPut("{cartId}/book")]
        public async Task<ActionResult<object>> BookCart(Guid cartId)
        {
            try
            {
                var paymentId = await _cartService.BookCartAsync(cartId);
                return Ok(new { paymentId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}

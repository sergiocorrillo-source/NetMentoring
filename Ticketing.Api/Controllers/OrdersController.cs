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
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        /// <summary>
        /// GET /api/orders/{orderId}
        /// Obtiene una orden por su ID
        /// </summary>
        [HttpGet("{orderId}")]
        public async Task<ActionResult<OrderDto>> GetOrder(Guid orderId)
        {
            try
            {
                var order = await _orderService.GetOrderAsync(orderId);
                if (order == null)
                    return NotFound(new { error = "Order not found" });

                return Ok(order);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// GET /api/orders/customer/{customerId}
        /// Obtiene todas las órdenes de un cliente
        /// </summary>
        [HttpGet("customer/{customerId}")]
        public async Task<ActionResult<List<OrderDto>>> GetOrdersByCustomer(Guid customerId)
        {
            try
            {
                var orders = await _orderService.GetOrdersByCustomerAsync(customerId);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// GET /api/orders/event/{eventId}
        /// Obtiene todas las órdenes de un evento
        /// </summary>
        [HttpGet("event/{eventId}")]
        public async Task<ActionResult<List<OrderDto>>> GetOrdersByEvent(Guid eventId)
        {
            try
            {
                var orders = await _orderService.GetOrdersByEventAsync(eventId);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// POST /api/orders
        /// Crea una nueva orden
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<OrderDto>> CreateOrder([FromBody] CreateOrderDto createOrderDto)
        {
            try
            {
                if (createOrderDto == null)
                    return BadRequest(new { error = "Order data is required" });

                if (createOrderDto.CustomerId == Guid.Empty)
                    return BadRequest(new { error = "CustomerId is required" });

                if (createOrderDto.EventId == Guid.Empty)
                    return BadRequest(new { error = "EventId is required" });

                if (createOrderDto.TotalAmount <= 0)
                    return BadRequest(new { error = "TotalAmount must be greater than 0" });

                var order = await _orderService.CreateOrderAsync(createOrderDto);
                return CreatedAtAction(nameof(GetOrder), new { orderId = order.OrderId }, order);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// PUT /api/orders/{orderId}/status
        /// Actualiza el estado de una orden
        /// Estados válidos: Created, PendingPayment, Paid, Cancelled
        /// </summary>
        [HttpPut("{orderId}/status")]
        public async Task<ActionResult<OrderDto>> UpdateOrderStatus(Guid orderId, [FromBody] UpdateOrderStatusDto updateDto)
        {
            try
            {
                if (updateDto == null || string.IsNullOrEmpty(updateDto.OrderStatus))
                    return BadRequest(new { error = "OrderStatus is required" });

                var order = await _orderService.UpdateOrderStatusAsync(orderId, updateDto.OrderStatus);
                return Ok(order);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// DELETE /api/orders/{orderId}
        /// Cancela una orden (cambia estado a Cancelled y libera los asientos)
        /// </summary>
        [HttpDelete("{orderId}")]
        public async Task<IActionResult> CancelOrder(Guid orderId)
        {
            try
            {
                await _orderService.CancelOrderAsync(orderId);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}

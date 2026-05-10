using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Ticketing.Services;
using Ticketing.Services.DTOs;

namespace Ticketing.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly PaymentService _paymentService;

        public PaymentsController(PaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        /// <summary>
        /// GET /api/payments/{paymentId}
        /// Obtiene el estado de un pago
        /// </summary>
        [HttpGet("{paymentId}")]
        public async Task<ActionResult<PaymentDto>> GetPayment(Guid paymentId)
        {
            try
            {
                var payment = await _paymentService.GetPaymentAsync(paymentId);
                if (payment == null)
                    return NotFound(new { error = "Payment not found" });

                return Ok(payment);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// POST /api/payments/{paymentId}/complete
        /// Completa un pago y cambia todas las entradas a vendido
        /// </summary>
        [HttpPost("{paymentId}/complete")]
        public async Task<IActionResult> CompletePayment(Guid paymentId)
        {
            try
            {
                await _paymentService.CompletePaymentAsync(paymentId);
                return Ok(new { message = "Payment completed successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// POST /api/payments/{paymentId}/failed
        /// Marca un pago como fallido y libera los asientos
        /// </summary>
        [HttpPost("{paymentId}/failed")]
        public async Task<IActionResult> FailPayment(Guid paymentId)
        {
            try
            {
                await _paymentService.FailPaymentAsync(paymentId);
                return Ok(new { message = "Payment failed, seats released" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}

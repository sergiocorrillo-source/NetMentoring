using System;
using System.Linq;
using System.Threading.Tasks;
using Ticketing.DAL;
using Ticketing.Domain.Entities;
using Ticketing.Services.DTOs;

namespace Ticketing.Services
{
    public class PaymentService
    {
        private readonly IUnitOfWork _uow;

        public PaymentService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<PaymentDto?> GetPaymentAsync(Guid paymentId)
        {
            var payment = await _uow.Repository<Payment>().GetByIdAsync(paymentId);
            if (payment == null) return null;

            return new PaymentDto
            {
                PaymentId = payment.PaymentId,
                Status = payment.Status,
                Amount = payment.Amount
            };
        }

        public async Task CompletePaymentAsync(Guid paymentId)
        {
            await _uow.ExecuteInTransactionAsync(async () =>
            {
                var payment = await _uow.Repository<Payment>().GetByIdAsync(paymentId);
                if (payment == null)
                    throw new InvalidOperationException("Payment not found.");

                payment.Status = "Completed";
                _uow.Repository<Payment>().Update(payment);

                // Cambiar estado de tickets y asientos a Sold
                var tickets = await _uow.Repository<Ticket>()
                    .FindAsync(t => t.PaymentId == paymentId);

                var seatRepo = _uow.Repository<Seat>();
                var ticketRepo = _uow.Repository<Ticket>();

                foreach (var ticket in tickets)
                {
                    ticket.Status = "Sold";
                    ticketRepo.Update(ticket);

                    var seat = await seatRepo.GetByIdAsync(ticket.SeatId);
                    if (seat != null)
                    {
                        seat.Status = SeatStatus.Sold;
                        seatRepo.Update(seat);
                    }
                }

                await _uow.SaveChangesAsync();
            });
        }

        public async Task FailPaymentAsync(Guid paymentId)
        {
            await _uow.ExecuteInTransactionAsync(async () =>
            {
                var payment = await _uow.Repository<Payment>().GetByIdAsync(paymentId);
                if (payment == null)
                    throw new InvalidOperationException("Payment not found.");

                payment.Status = "Failed";
                _uow.Repository<Payment>().Update(payment);

                // Cambiar estado de tickets y asientos a Available
                var tickets = await _uow.Repository<Ticket>()
                    .FindAsync(t => t.PaymentId == paymentId);

                var seatRepo = _uow.Repository<Seat>();
                var ticketRepo = _uow.Repository<Ticket>();

                foreach (var ticket in tickets)
                {
                    ticket.Status = "Cancelled";
                    ticketRepo.Update(ticket);

                    var seat = await seatRepo.GetByIdAsync(ticket.SeatId);
                    if (seat != null)
                    {
                        seat.Status = SeatStatus.Available;
                        seatRepo.Update(seat);
                    }
                }

                await _uow.SaveChangesAsync();
            });
        }
    }
}

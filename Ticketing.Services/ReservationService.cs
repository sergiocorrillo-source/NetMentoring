using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Ticketing.DAL;
using Ticketing.Domain.Entities;

namespace Ticketing.Services
{
    public class ReservationService
    {
        private readonly IUnitOfWork _uow;

        public ReservationService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        // Reserva un asiento de forma transaccional y crea un ticket asociado.
        public async Task<Guid> ReserveSeatAsync(Guid eventId, Guid seatId, Guid customerId, Guid offerId)
        {
            // Ejecutar en transacción para asegurar consistencia
            Guid ticketId = Guid.Empty;
            await _uow.ExecuteInTransactionAsync(async () =>
            {
                var seatRepo = _uow.Repository<Seat>();
                var ticketRepo = _uow.Repository<Ticket>();

                // Cargar el asiento con RowVersion (optimistic concurrency) y relación necesaria
                var seats = await seatRepo.GetWithIncludesAsync(s => s.SeatId == seatId, s => s.SeatManifest);
                var seat = seats.FirstOrDefault();
                if (seat is null) throw new InvalidOperationException("Seat not found.");

                if (seat.Status != SeatStatus.Available)
                {
                    throw new InvalidOperationException("Seat is not available.");
                }

                // Cambiar estado a Reserved
                seat.Status = SeatStatus.Reserved;
                seatRepo.Update(seat);

                // Crear ticket
                var ticket = new Ticket
                {
                    TicketId = Guid.NewGuid(),
                    EventId = eventId,
                    SeatId = seatId,
                    CustomerId = customerId,
                    OfferId = offerId,
                    Status = "Reserved",
                    CreatedAt = DateTime.UtcNow
                };

                await ticketRepo.AddAsync(ticket);
                await _uow.SaveChangesAsync();

                ticketId = ticket.TicketId;
            });

            return ticketId;
        }

        // Confirmar compra: cambia asiento a Sold y ticket a Confirmed en una transacción
        public async Task ConfirmPurchaseAsync(Guid ticketId)
        {
            await _uow.ExecuteInTransactionAsync(async () =>
            {
                var ticketRepo = _uow.Repository<Ticket>();
                var seatRepo = _uow.Repository<Seat>();

                var tickets = await ticketRepo.GetWithIncludesAsync(t => t.TicketId == ticketId, t => t.Seat);
                var ticket = tickets.FirstOrDefault();
                if (ticket is null) throw new InvalidOperationException("Ticket not found.");

                var seat = ticket.Seat ?? throw new InvalidOperationException("Seat not loaded.");

                if (seat.Status != SeatStatus.Reserved)
                    throw new InvalidOperationException("Seat is not reserved.");

                seat.Status = SeatStatus.Sold;
                seatRepo.Update(seat);

                ticket.Status = "Confirmed";
                ticketRepo.Update(ticket);

                await _uow.SaveChangesAsync();
            });
        }
    }
}

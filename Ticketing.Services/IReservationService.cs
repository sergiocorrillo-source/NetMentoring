using System;
using System.Threading.Tasks;

namespace Ticketing.Services
{
    public interface IReservationService
    {
        Task<Guid> ReserveSeatAsync(Guid eventId, Guid seatId, Guid customerId, Guid offerId);
        Task ConfirmPurchaseAsync(Guid ticketId);
    }
}

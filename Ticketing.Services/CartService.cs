using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ticketing.DAL;
using Ticketing.Domain.Entities;
using Ticketing.Services.DTOs;

namespace Ticketing.Services
{
    public class CartService
    {
        private readonly IUnitOfWork _uow;

        public CartService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<CartDto> GetCartAsync(Guid cartId)
        {
            var cartItems = await _uow.Repository<CartItem>()
                .GetWithIncludesAsync(ci => ci.CartId == cartId, ci => ci.Price);

            var items = cartItems.Select(ci => new CartItemDto
            {
                EventId = ci.EventId,
                SeatId = ci.SeatId,
                PriceId = ci.PriceId,
                SeatDescription = $"{ci.Seat?.Section}-{ci.Seat?.RowNumber}-{ci.Seat?.SeatNumber}",
                Price = ci.Price?.Amount ?? 0
            }).ToList();

            var totalAmount = items.Sum(i => i.Price);

            return new CartDto
            {
                CartId = cartId,
                Items = items,
                TotalAmount = totalAmount
            };
        }

        public async Task<CartDto> AddToCartAsync(Guid cartId, AddToCartRequestDto request)
        {
            await _uow.ExecuteInTransactionAsync(async () =>
            {
                var cartRepo = _uow.Repository<CartItem>();
                var seatRepo = _uow.Repository<Seat>();

                // Verificar que el asiento existe y está disponible
                var seat = await seatRepo.GetByIdAsync(request.SeatId);
                if (seat == null || seat.Status != SeatStatus.Available)
                    throw new InvalidOperationException("Seat is not available.");

                // Crear item de carrito
                var cartItem = new CartItem
                {
                    CartItemId = Guid.NewGuid(),
                    CartId = cartId,
                    EventId = request.EventId,
                    SeatId = request.SeatId,
                    PriceId = request.PriceId,
                    CreatedAt = DateTime.UtcNow
                };

                await cartRepo.AddAsync(cartItem);
                await _uow.SaveChangesAsync();
            });

            return await GetCartAsync(cartId);
        }

        public async Task RemoveFromCartAsync(Guid cartId, Guid eventId, Guid seatId)
        {
            await _uow.ExecuteInTransactionAsync(async () =>
            {
                var cartItems = await _uow.Repository<CartItem>()
                    .FindAsync(ci => ci.CartId == cartId && ci.EventId == eventId && ci.SeatId == seatId);

                var itemToRemove = cartItems.FirstOrDefault();
                if (itemToRemove != null)
                {
                    _uow.Repository<CartItem>().Remove(itemToRemove);
                    await _uow.SaveChangesAsync();
                }
            });
        }

        public async Task<Guid> BookCartAsync(Guid cartId)
        {
            Guid paymentId = Guid.Empty;

            await _uow.ExecuteInTransactionAsync(async () =>
            {
                var cartItems = await _uow.Repository<CartItem>()
                    .GetWithIncludesAsync(ci => ci.CartId == cartId, ci => ci.Price);

                if (!cartItems.Any())
                    throw new InvalidOperationException("Cart is empty.");

                var seatRepo = _uow.Repository<Seat>();
                var ticketRepo = _uow.Repository<Ticket>();
                var offerRepo = _uow.Repository<Offer>();

                // Crear pago
                var totalAmount = cartItems.Sum(ci => ci.Price?.Amount ?? 0);
                var payment = new Payment
                {
                    PaymentId = Guid.NewGuid(),
                    Status = "Pending",
                    Amount = totalAmount,
                    CreatedAt = DateTime.UtcNow
                };

                await _uow.Repository<Payment>().AddAsync(payment);

                // Cambiar estado de asientos a Reserved y crear tickets
                foreach (var cartItem in cartItems)
                {
                    var seat = await seatRepo.GetByIdAsync(cartItem.SeatId);
                    if (seat == null || seat.Status != SeatStatus.Available)
                        throw new InvalidOperationException($"Seat {cartItem.SeatId} is not available.");

                    seat.Status = SeatStatus.Reserved;
                    seatRepo.Update(seat);

                    // Obtener la primera oferta del evento (o crear lógica apropiada)
                    var offers = await offerRepo.FindAsync(o => o.EventId == cartItem.EventId);
                    var offer = offers.FirstOrDefault() ?? throw new InvalidOperationException("No offer found for event.");

                    var ticket = new Ticket
                    {
                        TicketId = Guid.NewGuid(),
                        EventId = cartItem.EventId,
                        SeatId = cartItem.SeatId,
                        OfferId = offer.OfferId,
                        PaymentId = payment.PaymentId,
                        Status = "Reserved",
                        CreatedAt = DateTime.UtcNow
                    };

                    await ticketRepo.AddAsync(ticket);
                }

                // Limpiar carrito
                var cartItemsToDelete = await _uow.Repository<CartItem>()
                    .FindAsync(ci => ci.CartId == cartId);

                foreach (var item in cartItemsToDelete)
                {
                    _uow.Repository<CartItem>().Remove(item);
                }

                await _uow.SaveChangesAsync();
                paymentId = payment.PaymentId;
            });

            return paymentId;
        }
    }
}

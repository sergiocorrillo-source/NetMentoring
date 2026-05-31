using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ticketing.DAL;
using Ticketing.Domain.Entities;
using Ticketing.Services.DTOs;

namespace Ticketing.Services
{
    public class CartService
    {
        private readonly IUnitOfWork _uow;
        private readonly ILogger<CartService>? _logger;
        // Per-seat locks to enforce pessimistic concurrency in-process
        private static readonly ConcurrentDictionary<Guid, SemaphoreSlim> SeatLocks = new();

        public CartService(IUnitOfWork uow, ILogger<CartService>? logger = null)
        {
            _uow = uow;
            _logger = logger;
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
            // Optimistic concurrency: update the seat status to Reserved within a transaction
            // and rely on EF Core RowVersion to detect concurrent updates.
            await _uow.ExecuteInTransactionAsync(async () =>
            {
                var cartRepo = _uow.Repository<CartItem>();
                var seatRepo = _uow.Repository<Seat>();

                // Verificar que el asiento existe y está disponible
                var seat = await seatRepo.GetByIdAsync(request.SeatId);
                if (seat == null || seat.Status != SeatStatus.Available)
                {
                    _logger?.LogWarning("Attempt to add seat {SeatId} to cart {CartId} but it's not available", request.SeatId, cartId);
                    throw new InvalidOperationException("Seat is not available.");
                }

                // Marcar asiento como reservado (esto actualizará RowVersion)
                seat.Status = SeatStatus.Reserved;
                seatRepo.Update(seat);

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

                try
                {
                    await _uow.SaveChangesAsync();
                }
                catch (Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException)
                {
                    // Concurrency conflict: another request modified the seat concurrently
                    _logger?.LogWarning("Concurrency conflict while adding seat {SeatId} to cart {CartId}", request.SeatId, cartId);
                    throw new InvalidOperationException("Seat is not available.");
                }
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

            // Acquire per-seat locks for all seats in the cart in a deterministic order
            var cartItems = await _uow.Repository<CartItem>()
                .GetWithIncludesAsync(ci => ci.CartId == cartId, ci => ci.Price);

            if (!cartItems.Any())
                throw new InvalidOperationException("Cart is empty.");

            var seatIds = cartItems.Select(ci => ci.SeatId).Distinct().OrderBy(id => id).ToList();
            var acquiredLocks = new List<SemaphoreSlim>();

            try
            {
                foreach (var seatId in seatIds)
                {
                    var sem = SeatLocks.GetOrAdd(seatId, _ => new SemaphoreSlim(1, 1));
                    await sem.WaitAsync();
                    acquiredLocks.Add(sem);
                }

                // Inside the critical section proceed with transactional operations
                await _uow.ExecuteInTransactionAsync(async () =>
                {
                    // reload cart items inside transaction to ensure up-to-date data
                    var itemsInTx = await _uow.Repository<CartItem>()
                        .GetWithIncludesAsync(ci => ci.CartId == cartId, ci => ci.Price);

                    var seatRepo = _uow.Repository<Seat>();
                    var ticketRepo = _uow.Repository<Ticket>();
                    var offerRepo = _uow.Repository<Offer>();

                    // Crear pago
                    var totalAmount = itemsInTx.Sum(ci => ci.Price?.Amount ?? 0);
                    var payment = new Payment
                    {
                        PaymentId = Guid.NewGuid(),
                        Status = "Pending",
                        Amount = totalAmount,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _uow.Repository<Payment>().AddAsync(payment);

                    // Cambiar estado de asientos a Reserved y crear tickets
                    foreach (var cartItem in itemsInTx)
                    {
                        var seat = await seatRepo.GetByIdAsync(cartItem.SeatId);
                        if (seat == null || seat.Status != SeatStatus.Available)
                        {
                            _logger?.LogWarning("Seat {SeatId} is not available during BookCart for cart {CartId}", cartItem.SeatId, cartId);
                            throw new InvalidOperationException($"Seat {cartItem.SeatId} is not available.");
                        }

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
            }
            finally
            {
                // Release acquired locks
                foreach (var sem in acquiredLocks)
                {
                    try { sem.Release(); } catch { }
                }

                // Optionally cleanup unused semaphores to avoid memory leak
                foreach (var id in seatIds)
                {
                    if (SeatLocks.TryGetValue(id, out var sem) && sem.CurrentCount == 1)
                    {
                        // no one is waiting; attempt to remove
                        SeatLocks.TryRemove(id, out _);
                    }
                }
            }

            return paymentId;
        }
    }
}

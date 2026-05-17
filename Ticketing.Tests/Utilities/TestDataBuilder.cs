using Ticketing.Domain.Entities;
using Ticketing.Services.DTOs;

namespace Ticketing.Tests.Utilities
{
    /// <summary>
    /// Builders para crear objetos de prueba
    /// </summary>
    public static class TestDataBuilder
    {
        public static Customer BuildCustomer(
            Guid? id = null,
            string? email = null,
            string? fullName = null)
        {
            return new Customer
            {
                CustomerId = id ?? Guid.NewGuid(),
                Email = email ?? "test@example.com",
                FullName = fullName ?? "Test User",
                CreatedAt = DateTime.UtcNow
            };
        }

        public static Event BuildEvent(
            Guid? id = null,
            string? name = null,
            Guid? venueId = null,
            Guid? seatManifestId = null)
        {
            return new Event
            {
                EventId = id ?? Guid.NewGuid(),
                Name = name ?? "Test Event",
                VenueId = venueId ?? Guid.NewGuid(),
                SeatManifestId = seatManifestId ?? Guid.NewGuid(),
                EventDate = DateTime.UtcNow.AddDays(30),
                EventTime = new TimeSpan(19, 0, 0),
                CreatedBy = "TestCreator"
            };
        }

        public static Order BuildOrder(
            Guid? id = null,
            Guid? customerId = null,
            Guid? eventId = null,
            string? status = null,
            decimal? totalAmount = null)
        {
            return new Order
            {
                OrderId = id ?? Guid.NewGuid(),
                CustomerId = customerId ?? Guid.NewGuid(),
                EventId = eventId ?? Guid.NewGuid(),
                OrderStatus = status ?? "Created",
                TotalAmount = totalAmount ?? 100.00m,
                Currency = "USD",
                CreatedAt = DateTime.UtcNow
            };
        }

        public static OrderDto BuildOrderDto(
            Guid? id = null,
            Guid? customerId = null,
            Guid? eventId = null,
            string? status = null,
            decimal? totalAmount = null)
        {
            return new OrderDto
            {
                OrderId = id ?? Guid.NewGuid(),
                CustomerId = customerId ?? Guid.NewGuid(),
                EventId = eventId ?? Guid.NewGuid(),
                OrderStatus = status ?? "Created",
                TotalAmount = totalAmount ?? 100.00m,
                Currency = "USD",
                CreatedAt = DateTime.UtcNow,
                Tickets = new List<TicketDto>()
            };
        }

        public static CreateOrderDto BuildCreateOrderDto(
            Guid? customerId = null,
            Guid? eventId = null,
            decimal? totalAmount = null,
            string? currency = null)
        {
            return new CreateOrderDto
            {
                CustomerId = customerId ?? Guid.NewGuid(),
                EventId = eventId ?? Guid.NewGuid(),
                TotalAmount = totalAmount ?? 100.00m,
                Currency = currency ?? "USD"
            };
        }

        public static Ticket BuildTicket(
            Guid? id = null,
            Guid? eventId = null,
            Guid? seatId = null,
            Guid? customerId = null,
            Guid? orderId = null)
        {
            return new Ticket
            {
                TicketId = id ?? Guid.NewGuid(),
                EventId = eventId ?? Guid.NewGuid(),
                SeatId = seatId ?? Guid.NewGuid(),
                CustomerId = customerId,
                OrderId = orderId,
                OfferId = Guid.NewGuid(),
                Status = "Created",
                CreatedAt = DateTime.UtcNow
            };
        }

        public static PaymentDto BuildPaymentDto(
            Guid? id = null,
            string? status = null,
            decimal? amount = null)
        {
            return new PaymentDto
            {
                PaymentId = id ?? Guid.NewGuid(),
                Status = status ?? "Pending",
                Amount = amount ?? 100.00m
            };
        }
    }
}

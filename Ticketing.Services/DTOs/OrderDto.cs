using System;
using System.Collections.Generic;

namespace Ticketing.Services.DTOs
{
    public class OrderDto
    {
        public Guid OrderId { get; set; }
        public Guid CustomerId { get; set; }
        public Guid EventId { get; set; }
        public string OrderStatus { get; set; } = null!;
        public decimal TotalAmount { get; set; }
        public string Currency { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public List<TicketDto> Tickets { get; set; } = new();
    }

    public class CreateOrderDto
    {
        public Guid CustomerId { get; set; }
        public Guid EventId { get; set; }
        public decimal TotalAmount { get; set; }
        public string Currency { get; set; } = "USD";
    }

    public class UpdateOrderStatusDto
    {
        public string OrderStatus { get; set; } = null!;
    }

    public class TicketDto
    {
        public Guid TicketId { get; set; }
        public Guid SeatId { get; set; }
        public string Status { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }
}

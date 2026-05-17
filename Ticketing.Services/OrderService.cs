using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ticketing.DAL;
using Ticketing.Domain.Entities;
using Ticketing.Services.DTOs;

namespace Ticketing.Services
{
    public class OrderService
    {
        private readonly IUnitOfWork _uow;

        public OrderService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        /// <summary>
        /// Obtiene una orden por su ID
        /// </summary>
        public async Task<OrderDto?> GetOrderAsync(Guid orderId)
        {
            var order = await _uow.Repository<Order>().GetByIdAsync(orderId);
            if (order == null) return null;

            var tickets = await _uow.Repository<Ticket>()
                .FindAsync(t => t.OrderId == orderId);

            return MapToDto(order, tickets.ToList());
        }

        /// <summary>
        /// Obtiene todas las órdenes de un cliente
        /// </summary>
        public async Task<List<OrderDto>> GetOrdersByCustomerAsync(Guid customerId)
        {
            var orders = await _uow.Repository<Order>()
                .FindAsync(o => o.CustomerId == customerId);

            var orderDtos = new List<OrderDto>();

            foreach (var order in orders)
            {
                var tickets = await _uow.Repository<Ticket>()
                    .FindAsync(t => t.OrderId == order.OrderId);

                orderDtos.Add(MapToDto(order, tickets.ToList()));
            }

            return orderDtos;
        }

        /// <summary>
        /// Obtiene todas las órdenes de un evento
        /// </summary>
        public async Task<List<OrderDto>> GetOrdersByEventAsync(Guid eventId)
        {
            var orders = await _uow.Repository<Order>()
                .FindAsync(o => o.EventId == eventId);

            var orderDtos = new List<OrderDto>();

            foreach (var order in orders)
            {
                var tickets = await _uow.Repository<Ticket>()
                    .FindAsync(t => t.OrderId == order.OrderId);

                orderDtos.Add(MapToDto(order, tickets.ToList()));
            }

            return orderDtos;
        }

        /// <summary>
        /// Crea una nueva orden
        /// </summary>
        public async Task<OrderDto> CreateOrderAsync(CreateOrderDto createOrderDto)
        {
            // Validar que el cliente existe
            var customer = await _uow.Repository<Customer>()
                .GetByIdAsync(createOrderDto.CustomerId);
            if (customer == null)
                throw new InvalidOperationException("Customer not found.");

            // Validar que el evento existe
            var @event = await _uow.Repository<Event>()
                .GetByIdAsync(createOrderDto.EventId);
            if (@event == null)
                throw new InvalidOperationException("Event not found.");

            var order = new Order
            {
                OrderId = Guid.NewGuid(),
                CustomerId = createOrderDto.CustomerId,
                EventId = createOrderDto.EventId,
                OrderStatus = "Created",
                TotalAmount = createOrderDto.TotalAmount,
                Currency = createOrderDto.Currency,
                CreatedAt = DateTime.UtcNow
            };

            await _uow.Repository<Order>().AddAsync(order);
            await _uow.SaveChangesAsync();

            return MapToDto(order, new List<Ticket>());
        }

        /// <summary>
        /// Actualiza el estado de una orden
        /// </summary>
        public async Task<OrderDto> UpdateOrderStatusAsync(Guid orderId, string newStatus)
        {
            var validStatuses = new[] { "Created", "PendingPayment", "Paid", "Cancelled" };
            if (!validStatuses.Contains(newStatus))
                throw new InvalidOperationException($"Invalid order status: {newStatus}");

            var order = await _uow.Repository<Order>().GetByIdAsync(orderId);
            if (order == null)
                throw new InvalidOperationException("Order not found.");

            await _uow.ExecuteInTransactionAsync(async () =>
            {
                order.OrderStatus = newStatus;
                _uow.Repository<Order>().Update(order);

                // Si la orden se marca como "Paid", actualizar los tickets y asientos
                if (newStatus == "Paid")
                {
                    var tickets = await _uow.Repository<Ticket>()
                        .FindAsync(t => t.OrderId == orderId);

                    foreach (var ticket in tickets)
                    {
                        ticket.Status = "Paid";
                        _uow.Repository<Ticket>().Update(ticket);

                        var seat = await _uow.Repository<Seat>().GetByIdAsync(ticket.SeatId);
                        if (seat != null)
                        {
                            seat.Status = SeatStatus.Sold;
                            _uow.Repository<Seat>().Update(seat);
                        }
                    }
                }

                // Si la orden se cancela, liberar los asientos
                if (newStatus == "Cancelled")
                {
                    var tickets = await _uow.Repository<Ticket>()
                        .FindAsync(t => t.OrderId == orderId);

                    foreach (var ticket in tickets)
                    {
                        var seat = await _uow.Repository<Seat>().GetByIdAsync(ticket.SeatId);
                        if (seat != null)
                        {
                            seat.Status = SeatStatus.Available;
                            _uow.Repository<Seat>().Update(seat);
                        }

                        _uow.Repository<Ticket>().Remove(ticket);
                    }
                }

                await _uow.SaveChangesAsync();
            });

            var updatedOrder = await _uow.Repository<Order>().GetByIdAsync(orderId);
            var updatedTickets = await _uow.Repository<Ticket>()
                .FindAsync(t => t.OrderId == orderId);

            return MapToDto(updatedOrder!, updatedTickets.ToList());
        }

        /// <summary>
        /// Cancela una orden
        /// </summary>
        public async Task<OrderDto> CancelOrderAsync(Guid orderId)
        {
            return await UpdateOrderStatusAsync(orderId, "Cancelled");
        }

        /// <summary>
        /// Mapea una entidad Order a OrderDto
        /// </summary>
        private OrderDto MapToDto(Order order, List<Ticket> tickets)
        {
            return new OrderDto
            {
                OrderId = order.OrderId,
                CustomerId = order.CustomerId,
                EventId = order.EventId,
                OrderStatus = order.OrderStatus,
                TotalAmount = order.TotalAmount,
                Currency = order.Currency,
                CreatedAt = order.CreatedAt,
                Tickets = tickets.Select(t => new TicketDto
                {
                    TicketId = t.TicketId,
                    SeatId = t.SeatId,
                    Status = t.Status,
                    CreatedAt = t.CreatedAt
                }).ToList()
            };
        }
    }
}

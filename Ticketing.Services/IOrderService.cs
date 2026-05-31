using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ticketing.Services.DTOs;

namespace Ticketing.Services
{
    public interface IOrderService
    {
        Task<OrderDto?> GetOrderAsync(Guid orderId);
        Task<List<OrderDto>> GetOrdersByCustomerAsync(Guid customerId);
        Task<List<OrderDto>> GetOrdersByEventAsync(Guid eventId);
        Task<OrderDto> CreateOrderAsync(CreateOrderDto createOrderDto);
        Task<OrderDto> UpdateOrderStatusAsync(Guid orderId, string newStatus);
        Task<OrderDto> CancelOrderAsync(Guid orderId);
    }
}

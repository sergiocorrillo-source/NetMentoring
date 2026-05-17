using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using Ticketing.Api.Controllers;
using Ticketing.Services;
using Ticketing.Services.DTOs;
using Ticketing.DAL;

namespace Ticketing.Tests.Controllers
{
    public class OrdersControllerTests
    {
        private readonly Mock<OrderService> _mockOrderService;
        private readonly OrdersController _controller;

        public OrdersControllerTests()
        {
            var mockUow = new Mock<IUnitOfWork>();
            _mockOrderService = new Mock<OrderService>(mockUow.Object);
            _controller = new OrdersController(_mockOrderService.Object);
        }

        [Fact]
        public async Task GetOrder_WithValidId_ReturnsOkResult()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var orderDto = new OrderDto
            {
                OrderId = orderId,
                CustomerId = Guid.NewGuid(),
                EventId = Guid.NewGuid(),
                OrderStatus = "Created",
                TotalAmount = 100.00m,
                Currency = "USD",
                CreatedAt = DateTime.UtcNow,
                Tickets = new List<TicketDto>()
            };

            _mockOrderService
                .Setup(service => service.GetOrderAsync(orderId))
                .ReturnsAsync(orderDto);

            // Act
            var result = await _controller.GetOrder(orderId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(200, okResult.StatusCode);
            var returnedOrder = Assert.IsType<OrderDto>(okResult.Value);
            Assert.Equal(orderId, returnedOrder.OrderId);
        }

        [Fact]
        public async Task GetOrder_WithInvalidId_ReturnsNotFoundResult()
        {
            // Arrange
            var invalidId = Guid.NewGuid();

            _mockOrderService
                .Setup(service => service.GetOrderAsync(invalidId))
                .ReturnsAsync((OrderDto?)null);

            // Act
            var result = await _controller.GetOrder(invalidId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal(404, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task GetOrdersByCustomer_WithValidCustomerId_ReturnsOkResult()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var orders = new List<OrderDto>
            {
                new OrderDto
                {
                    OrderId = Guid.NewGuid(),
                    CustomerId = customerId,
                    EventId = Guid.NewGuid(),
                    OrderStatus = "Created",
                    TotalAmount = 100.00m,
                    Currency = "USD",
                    CreatedAt = DateTime.UtcNow,
                    Tickets = new List<TicketDto>()
                }
            };

            _mockOrderService
                .Setup(service => service.GetOrdersByCustomerAsync(customerId))
                .ReturnsAsync(orders);

            // Act
            var result = await _controller.GetOrdersByCustomer(customerId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(200, okResult.StatusCode);
            var returnedOrders = Assert.IsType<List<OrderDto>>(okResult.Value);
            Assert.Single(returnedOrders);
        }

        [Fact]
        public async Task GetOrdersByEvent_WithValidEventId_ReturnsOkResult()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var orders = new List<OrderDto>
            {
                new OrderDto
                {
                    OrderId = Guid.NewGuid(),
                    CustomerId = Guid.NewGuid(),
                    EventId = eventId,
                    OrderStatus = "Paid",
                    TotalAmount = 150.00m,
                    Currency = "USD",
                    CreatedAt = DateTime.UtcNow,
                    Tickets = new List<TicketDto>()
                },
                new OrderDto
                {
                    OrderId = Guid.NewGuid(),
                    CustomerId = Guid.NewGuid(),
                    EventId = eventId,
                    OrderStatus = "Created",
                    TotalAmount = 200.00m,
                    Currency = "USD",
                    CreatedAt = DateTime.UtcNow,
                    Tickets = new List<TicketDto>()
                }
            };

            _mockOrderService
                .Setup(service => service.GetOrdersByEventAsync(eventId))
                .ReturnsAsync(orders);

            // Act
            var result = await _controller.GetOrdersByEvent(eventId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(200, okResult.StatusCode);
            var returnedOrders = Assert.IsType<List<OrderDto>>(okResult.Value);
            Assert.Equal(2, returnedOrders.Count);
        }

        [Fact]
        public async Task CreateOrder_WithValidData_ReturnsCreatedAtActionResult()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var eventId = Guid.NewGuid();
            var createOrderDto = new CreateOrderDto
            {
                CustomerId = customerId,
                EventId = eventId,
                TotalAmount = 100.00m,
                Currency = "USD"
            };

            var createdOrder = new OrderDto
            {
                OrderId = Guid.NewGuid(),
                CustomerId = customerId,
                EventId = eventId,
                OrderStatus = "Created",
                TotalAmount = 100.00m,
                Currency = "USD",
                CreatedAt = DateTime.UtcNow,
                Tickets = new List<TicketDto>()
            };

            _mockOrderService
                .Setup(service => service.CreateOrderAsync(createOrderDto))
                .ReturnsAsync(createdOrder);

            // Act
            var result = await _controller.CreateOrder(createOrderDto);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(nameof(OrdersController.GetOrder), createdResult.ActionName);
            Assert.Equal(createdOrder.OrderId, ((OrderDto)createdResult.Value!).OrderId);
        }

        [Fact]
        public async Task CreateOrder_WithNullData_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.CreateOrder(null!);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal(400, badRequestResult.StatusCode);
        }

        [Fact]
        public async Task CreateOrder_WithEmptyCustomerId_ReturnsBadRequest()
        {
            // Arrange
            var createOrderDto = new CreateOrderDto
            {
                CustomerId = Guid.Empty,
                EventId = Guid.NewGuid(),
                TotalAmount = 100.00m,
                Currency = "USD"
            };

            // Act
            var result = await _controller.CreateOrder(createOrderDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal(400, badRequestResult.StatusCode);
        }

        [Fact]
        public async Task CreateOrder_WithZeroAmount_ReturnsBadRequest()
        {
            // Arrange
            var createOrderDto = new CreateOrderDto
            {
                CustomerId = Guid.NewGuid(),
                EventId = Guid.NewGuid(),
                TotalAmount = 0m,
                Currency = "USD"
            };

            // Act
            var result = await _controller.CreateOrder(createOrderDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal(400, badRequestResult.StatusCode);
        }

        [Fact]
        public async Task UpdateOrderStatus_WithValidStatus_ReturnsOkResult()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var updateDto = new UpdateOrderStatusDto { OrderStatus = "PendingPayment" };
            var updatedOrder = new OrderDto
            {
                OrderId = orderId,
                CustomerId = Guid.NewGuid(),
                EventId = Guid.NewGuid(),
                OrderStatus = "PendingPayment",
                TotalAmount = 100.00m,
                Currency = "USD",
                CreatedAt = DateTime.UtcNow,
                Tickets = new List<TicketDto>()
            };

            _mockOrderService
                .Setup(service => service.UpdateOrderStatusAsync(orderId, updateDto.OrderStatus))
                .ReturnsAsync(updatedOrder);

            // Act
            var result = await _controller.UpdateOrderStatus(orderId, updateDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(200, okResult.StatusCode);
            var returnedOrder = Assert.IsType<OrderDto>(okResult.Value);
            Assert.Equal("PendingPayment", returnedOrder.OrderStatus);
        }

        [Fact]
        public async Task UpdateOrderStatus_WithNullDto_ReturnsBadRequest()
        {
            // Arrange
            var orderId = Guid.NewGuid();

            // Act
            var result = await _controller.UpdateOrderStatus(orderId, null!);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal(400, badRequestResult.StatusCode);
        }

        [Fact]
        public async Task UpdateOrderStatus_WithInvalidStatus_ReturnsBadRequest()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var updateDto = new UpdateOrderStatusDto { OrderStatus = "InvalidStatus" };

            _mockOrderService
                .Setup(service => service.UpdateOrderStatusAsync(orderId, updateDto.OrderStatus))
                .ThrowsAsync(new InvalidOperationException("Invalid order status"));

            // Act
            var result = await _controller.UpdateOrderStatus(orderId, updateDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal(400, badRequestResult.StatusCode);
        }

        [Fact]
        public async Task CancelOrder_WithValidId_ReturnsNoContent()
        {
            // Arrange
            var orderId = Guid.NewGuid();

            _mockOrderService
                .Setup(service => service.CancelOrderAsync(orderId))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.CancelOrder(orderId);

            // Assert
            var noContentResult = Assert.IsType<NoContentResult>(result);
            Assert.Equal(204, noContentResult.StatusCode);
        }

        [Fact]
        public async Task CancelOrder_WithInvalidId_ReturnsBadRequest()
        {
            // Arrange
            var invalidId = Guid.NewGuid();

            _mockOrderService
                .Setup(service => service.CancelOrderAsync(invalidId))
                .ThrowsAsync(new InvalidOperationException("Order not found"));

            // Act
            var result = await _controller.CancelOrder(invalidId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
        }
    }
}

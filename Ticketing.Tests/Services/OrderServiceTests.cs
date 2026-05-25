using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Moq;
using Xunit;
using Ticketing.DAL;
using Ticketing.Domain.Entities;
using Ticketing.Services;
using Ticketing.Services.DTOs;

namespace Ticketing.Tests.Services
{
    public class OrderServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly OrderService _orderService;

        public OrderServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _orderService = new OrderService(_mockUnitOfWork.Object);
        }

        [Fact]
        public async Task GetOrderAsync_WithValidId_ReturnsOrder()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var eventId = Guid.NewGuid();

            var order = new Order
            {
                OrderId = orderId,
                CustomerId = customerId,
                EventId = eventId,
                OrderStatus = "Created",
                TotalAmount = 100.00m,
                Currency = "USD",
                CreatedAt = DateTime.UtcNow
            };

            var tickets = new List<Ticket>();

            var mockOrderRepository = new Mock<IRepository<Order>>();
            var mockTicketRepository = new Mock<IRepository<Ticket>>();

            mockOrderRepository
                .Setup(repo => repo.GetByIdAsync(orderId))
                .ReturnsAsync(order);

            mockTicketRepository
                .Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<Ticket, bool>>>()))
                .ReturnsAsync(tickets);

            _mockUnitOfWork
                .Setup(uow => uow.Repository<Order>())
                .Returns(mockOrderRepository.Object);

            _mockUnitOfWork
                .Setup(uow => uow.Repository<Ticket>())
                .Returns(mockTicketRepository.Object);

            // Act
            var result = await _orderService.GetOrderAsync(orderId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(orderId, result.OrderId);
            Assert.Equal("Created", result.OrderStatus);
            Assert.Equal(100.00m, result.TotalAmount);
        }

        [Fact]
        public async Task GetOrderAsync_WithInvalidId_ReturnsNull()
        {
            // Arrange
            var invalidId = Guid.NewGuid();

            var mockOrderRepository = new Mock<IRepository<Order>>();
            mockOrderRepository
                .Setup(repo => repo.GetByIdAsync(invalidId))
                .ReturnsAsync((Order?)null);

            _mockUnitOfWork
                .Setup(uow => uow.Repository<Order>())
                .Returns(mockOrderRepository.Object);

            // Act
            var result = await _orderService.GetOrderAsync(invalidId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task CreateOrderAsync_WithValidData_CreatesOrder()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var eventId = Guid.NewGuid();

            var createOrderDto = new CreateOrderDto
            {
                CustomerId = customerId,
                EventId = eventId,
                TotalAmount = 150.00m,
                Currency = "USD"
            };

            var customer = new Customer
            {
                CustomerId = customerId,
                Email = "test@example.com",
                FullName = "Test User"
            };

            var @event = new Event
            {
                EventId = eventId,
                Name = "Test Event"
            };

            var mockCustomerRepository = new Mock<IRepository<Customer>>();
            var mockEventRepository = new Mock<IRepository<Event>>();
            var mockOrderRepository = new Mock<IRepository<Order>>();

            mockCustomerRepository
                .Setup(repo => repo.GetByIdAsync(customerId))
                .ReturnsAsync(customer);

            mockEventRepository
                .Setup(repo => repo.GetByIdAsync(eventId))
                .ReturnsAsync(@event);

            mockOrderRepository
                .Setup(repo => repo.AddAsync(It.IsAny<Order>()))
                .Returns(Task.CompletedTask);

            _mockUnitOfWork
                .Setup(uow => uow.Repository<Customer>())
                .Returns(mockCustomerRepository.Object);

            _mockUnitOfWork
                .Setup(uow => uow.Repository<Event>())
                .Returns(mockEventRepository.Object);

            _mockUnitOfWork
                .Setup(uow => uow.Repository<Order>())
                .Returns(mockOrderRepository.Object);

            _mockUnitOfWork
                .Setup(uow => uow.SaveChangesAsync())
                .Returns(Task.FromResult(1));

            // Act
            var result = await _orderService.CreateOrderAsync(createOrderDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(customerId, result.CustomerId);
            Assert.Equal(eventId, result.EventId);
            Assert.Equal("Created", result.OrderStatus);
            Assert.Equal(150.00m, result.TotalAmount);
            mockOrderRepository.Verify(repo => repo.AddAsync(It.IsAny<Order>()), Times.Once);
        }

        [Fact]
        public async Task CreateOrderAsync_WithInvalidCustomer_ThrowsException()
        {
            // Arrange
            var invalidCustomerId = Guid.NewGuid();
            var eventId = Guid.NewGuid();

            var createOrderDto = new CreateOrderDto
            {
                CustomerId = invalidCustomerId,
                EventId = eventId,
                TotalAmount = 150.00m,
                Currency = "USD"
            };

            var mockCustomerRepository = new Mock<IRepository<Customer>>();
            mockCustomerRepository
                .Setup(repo => repo.GetByIdAsync(invalidCustomerId))
                .ReturnsAsync((Customer?)null);

            _mockUnitOfWork
                .Setup(uow => uow.Repository<Customer>())
                .Returns(mockCustomerRepository.Object);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _orderService.CreateOrderAsync(createOrderDto));
        }

        [Fact]
        public async Task UpdateOrderStatusAsync_WithValidStatus_UpdatesOrder()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var newStatus = "PendingPayment";

            var order = new Order
            {
                OrderId = orderId,
                CustomerId = Guid.NewGuid(),
                EventId = Guid.NewGuid(),
                OrderStatus = "Created",
                TotalAmount = 100.00m,
                Currency = "USD"
            };

            var mockOrderRepository = new Mock<IRepository<Order>>();
            var mockTicketRepository = new Mock<IRepository<Ticket>>();

            mockOrderRepository
                .Setup(repo => repo.GetByIdAsync(orderId))
                .ReturnsAsync(order);

            mockTicketRepository
                .Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<Ticket, bool>>>()))
                .ReturnsAsync(new List<Ticket>());

            _mockUnitOfWork
                .Setup(uow => uow.Repository<Order>())
                .Returns(mockOrderRepository.Object);

            _mockUnitOfWork
                .Setup(uow => uow.Repository<Ticket>())
                .Returns(mockTicketRepository.Object);

            _mockUnitOfWork
                .Setup(uow => uow.ExecuteInTransactionAsync(It.IsAny<Func<Task>>()))
                .Returns<Func<Task>>(async action => await action());

            // Act
            var result = await _orderService.UpdateOrderStatusAsync(orderId, newStatus);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(newStatus, result.OrderStatus);
            mockOrderRepository.Verify(repo => repo.Update(It.IsAny<Order>()), Times.Once);
        }

        [Fact]
        public async Task UpdateOrderStatusAsync_WithInvalidStatus_ThrowsException()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var invalidStatus = "InvalidStatus";

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _orderService.UpdateOrderStatusAsync(orderId, invalidStatus));
        }

        [Fact]
        public async Task CancelOrderAsync_CancelsOrder()
        {
            // Arrange
            var orderId = Guid.NewGuid();

            var order = new Order
            {
                OrderId = orderId,
                CustomerId = Guid.NewGuid(),
                EventId = Guid.NewGuid(),
                OrderStatus = "Created",
                TotalAmount = 100.00m,
                Currency = "USD"
            };

            var tickets = new List<Ticket>();
            var mockOrderRepository = new Mock<IRepository<Order>>();
            var mockTicketRepository = new Mock<IRepository<Ticket>>();
            var mockSeatRepository = new Mock<IRepository<Seat>>();

            mockOrderRepository
                .Setup(repo => repo.GetByIdAsync(orderId))
                .ReturnsAsync(order);

            mockTicketRepository
                .Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<Ticket, bool>>>()))
                .ReturnsAsync(tickets);

            _mockUnitOfWork
                .Setup(uow => uow.Repository<Order>())
                .Returns(mockOrderRepository.Object);

            _mockUnitOfWork
                .Setup(uow => uow.Repository<Ticket>())
                .Returns(mockTicketRepository.Object);

            _mockUnitOfWork
                .Setup(uow => uow.Repository<Seat>())
                .Returns(mockSeatRepository.Object);

            _mockUnitOfWork
                .Setup(uow => uow.ExecuteInTransactionAsync(It.IsAny<Func<Task>>()))
                .Returns<Func<Task>>(async action => await action());

            // Act
            var result = await _orderService.CancelOrderAsync(orderId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Cancelled", result.OrderStatus);
        }
    }
}

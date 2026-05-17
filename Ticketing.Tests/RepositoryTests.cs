using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ticketing.DAL;
using Ticketing.Domain.Entities;
using Xunit;
using FluentAssertions;

namespace Ticketing.Tests
{
    public class RepositoryTests
    {
        private readonly Mock<IRepository<Customer>> _mockCustomerRepo;
        private readonly Mock<IRepository<Seat>> _mockSeatRepo;
        private readonly Mock<IRepository<Ticket>> _mockTicketRepo;

        public RepositoryTests()
        {
            _mockCustomerRepo = new Mock<IRepository<Customer>>();
            _mockSeatRepo = new Mock<IRepository<Seat>>();
            _mockTicketRepo = new Mock<IRepository<Ticket>>();
        }

        [Fact]
        public async Task GetByIdAsync_WithValidId_ReturnsCustomer()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var customer = new Customer
            {
                CustomerId = customerId,
                Email = "test@example.com",
                FullName = "Test User",
                CreatedAt = DateTime.UtcNow
            };

            _mockCustomerRepo
                .Setup(r => r.GetByIdAsync(customerId, default))
                .ReturnsAsync(customer);

            // Act
            var result = await _mockCustomerRepo.Object.GetByIdAsync(customerId);

            // Assert
            result.Should().NotBeNull();
            result?.CustomerId.Should().Be(customerId);
            result?.Email.Should().Be("test@example.com");
            result?.FullName.Should().Be("Test User");
        }

        [Fact]
        public async Task GetByIdAsync_WithInvalidId_ReturnsNull()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            _mockCustomerRepo
                .Setup(r => r.GetByIdAsync(customerId, default))
                .ReturnsAsync((Customer?)null);

            // Act
            var result = await _mockCustomerRepo.Object.GetByIdAsync(customerId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllCustomers()
        {
            // Arrange
            var customers = new List<Customer>
            {
                new Customer { CustomerId = Guid.NewGuid(), Email = "user1@example.com", FullName = "User 1" },
                new Customer { CustomerId = Guid.NewGuid(), Email = "user2@example.com", FullName = "User 2" },
                new Customer { CustomerId = Guid.NewGuid(), Email = "user3@example.com", FullName = "User 3" }
            };

            _mockCustomerRepo
                .Setup(r => r.GetAllAsync(default))
                .ReturnsAsync(customers);

            // Act
            var result = await _mockCustomerRepo.Object.GetAllAsync();

            // Assert
            result.Should().HaveCount(3);
            result.Should().ContainEquivalentOf(customers[0]);
        }

        [Fact]
        public async Task FindAsync_WithPredicate_ReturnsFilteredCustomers()
        {
            // Arrange
            var customers = new List<Customer>
            {
                new Customer { CustomerId = Guid.NewGuid(), Email = "admin@example.com", FullName = "Admin User" },
                new Customer { CustomerId = Guid.NewGuid(), Email = "user@example.com", FullName = "Regular User" }
            };

            _mockCustomerRepo
                .Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Customer, bool>>>(), default))
                .ReturnsAsync(customers.Where(c => c.Email.Contains("admin")));

            // Act
            var result = await _mockCustomerRepo.Object.FindAsync(c => c.Email.Contains("admin"));

            // Assert
            result.Should().HaveCount(1);
            result.First().Email.Should().Be("admin@example.com");
        }

        [Fact]
        public async Task AddAsync_WithValidCustomer_CallsAdd()
        {
            // Arrange
            var customer = new Customer
            {
                CustomerId = Guid.NewGuid(),
                Email = "newuser@example.com",
                FullName = "New User"
            };

            _mockCustomerRepo
                .Setup(r => r.AddAsync(customer, default))
                .Returns(Task.CompletedTask);

            // Act
            await _mockCustomerRepo.Object.AddAsync(customer);

            // Assert
            _mockCustomerRepo.Verify(r => r.AddAsync(customer, default), Times.Once);
        }

        [Fact]
        public void Update_WithValidCustomer_CallsUpdate()
        {
            // Arrange
            var customer = new Customer
            {
                CustomerId = Guid.NewGuid(),
                Email = "updated@example.com",
                FullName = "Updated User"
            };

            _mockCustomerRepo
                .Setup(r => r.Update(It.IsAny<Customer>()));

            // Act
            _mockCustomerRepo.Object.Update(customer);

            // Assert
            _mockCustomerRepo.Verify(r => r.Update(It.IsAny<Customer>()), Times.Once);
        }

        [Fact]
        public void Remove_WithValidCustomer_CallsRemove()
        {
            // Arrange
            var customer = new Customer
            {
                CustomerId = Guid.NewGuid(),
                Email = "todelete@example.com",
                FullName = "To Delete"
            };

            _mockCustomerRepo
                .Setup(r => r.Remove(It.IsAny<Customer>()));

            // Act
            _mockCustomerRepo.Object.Remove(customer);

            // Assert
            _mockCustomerRepo.Verify(r => r.Remove(It.IsAny<Customer>()), Times.Once);
        }

        [Fact]
        public async Task GetWithIncludesAsync_LoadsRelatedEntities()
        {
            // Arrange
            var seatId = Guid.NewGuid();
            var seat = new Seat
            {
                SeatId = seatId,
                SeatManifestId = Guid.NewGuid(),
                SeatType = "VIP",
                Section = "A",
                RowNumber = "1",
                SeatNumber = "10",
                Status = SeatStatus.Available
            };

            _mockSeatRepo
                .Setup(r => r.GetWithIncludesAsync(
                    It.IsAny<System.Linq.Expressions.Expression<System.Func<Seat, bool>>>(),
                    It.IsAny<System.Linq.Expressions.Expression<System.Func<Seat, object>>[]>()))
                .ReturnsAsync(new List<Seat> { seat });

            // Act
            var result = await _mockSeatRepo.Object.GetWithIncludesAsync(s => s.SeatId == seatId);

            // Assert
            result.Should().HaveCount(1);
            result.First().SeatType.Should().Be("VIP");
        }
    }
}

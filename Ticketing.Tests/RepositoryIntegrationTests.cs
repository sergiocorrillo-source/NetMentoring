using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Ticketing.Data;
using Ticketing.DAL;
using Ticketing.Domain.Entities;
using Xunit;
using FluentAssertions;

namespace Ticketing.Tests
{
    public class RepositoryIntegrationTests : IDisposable
    {
        private readonly TicketingDbContext _dbContext;
        private readonly IRepository<Customer> _customerRepository;
        private readonly IRepository<Venue> _venueRepository;
        private readonly IRepository<Seat> _seatRepository;

        public RepositoryIntegrationTests()
        {
            var options = new DbContextOptionsBuilder<TicketingDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _dbContext = new TicketingDbContext(options);
            _customerRepository = new Repository<Customer>(_dbContext);
            _venueRepository = new Repository<Venue>(_dbContext);
            _seatRepository = new Repository<Seat>(_dbContext);
        }

        [Fact]
        public async Task AddAsync_WithValidCustomer_SavesToDatabase()
        {
            // Arrange
            var customer = new Customer
            {
                CustomerId = Guid.NewGuid(),
                Email = "integration@test.com",
                FullName = "Integration Test User",
                CreatedAt = DateTime.UtcNow
            };

            // Act
            await _customerRepository.AddAsync(customer);
            await _dbContext.SaveChangesAsync();

            // Assert
            var savedCustomer = await _dbContext.Customers
                .FirstOrDefaultAsync(c => c.CustomerId == customer.CustomerId);

            savedCustomer.Should().NotBeNull();
            savedCustomer?.Email.Should().Be("integration@test.com");
            savedCustomer?.FullName.Should().Be("Integration Test User");
        }

        [Fact]
        public async Task GetByIdAsync_WithValidId_ReturnsCustomerFromDatabase()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var customer = new Customer
            {
                CustomerId = customerId,
                Email = "getbyid@test.com",
                FullName = "Get By ID User",
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.Customers.Add(customer);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _customerRepository.GetByIdAsync(customerId);

            // Assert
            result.Should().NotBeNull();
            result?.CustomerId.Should().Be(customerId);
            result?.Email.Should().Be("getbyid@test.com");
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllCustomersFromDatabase()
        {
            // Arrange
            var customers = new List<Customer>
            {
                new Customer { CustomerId = Guid.NewGuid(), Email = "user1@test.com", FullName = "User 1" },
                new Customer { CustomerId = Guid.NewGuid(), Email = "user2@test.com", FullName = "User 2" },
                new Customer { CustomerId = Guid.NewGuid(), Email = "user3@test.com", FullName = "User 3" }
            };

            _dbContext.Customers.AddRange(customers);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _customerRepository.GetAllAsync();

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
                new Customer { CustomerId = Guid.NewGuid(), Email = "admin@test.com", FullName = "Admin" },
                new Customer { CustomerId = Guid.NewGuid(), Email = "user@test.com", FullName = "User" }
            };

            _dbContext.Customers.AddRange(customers);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _customerRepository.FindAsync(c => c.Email.Contains("admin"));

            // Assert
            result.Should().HaveCount(1);
            result.First().Email.Should().Be("admin@test.com");
        }

        [Fact]
        public async Task Update_WithModifiedCustomer_PersistsChangesToDatabase()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var customer = new Customer
            {
                CustomerId = customerId,
                Email = "original@test.com",
                FullName = "Original Name",
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.Customers.Add(customer);
            await _dbContext.SaveChangesAsync();

            // Act
            customer.Email = "updated@test.com";
            customer.FullName = "Updated Name";
            _customerRepository.Update(customer);
            await _dbContext.SaveChangesAsync();

            // Assert
            var updatedCustomer = await _dbContext.Customers.FindAsync(customerId);
            updatedCustomer?.Email.Should().Be("updated@test.com");
            updatedCustomer?.FullName.Should().Be("Updated Name");
        }

        [Fact]
        public async Task Remove_WithValidCustomer_DeletesFromDatabase()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var customer = new Customer
            {
                CustomerId = customerId,
                Email = "todelete@test.com",
                FullName = "To Delete",
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.Customers.Add(customer);
            await _dbContext.SaveChangesAsync();

            // Act
            _customerRepository.Remove(customer);
            await _dbContext.SaveChangesAsync();

            // Assert
            var deletedCustomer = await _dbContext.Customers.FindAsync(customerId);
            deletedCustomer.Should().BeNull();
        }

        [Fact]
        public async Task GetWithIncludesAsync_LoadsRelatedEntities()
        {
            // Arrange
            var venueId = Guid.NewGuid();
            var seatManifestId = Guid.NewGuid();

            var venue = new Venue
            {
                VenueId = venueId,
                Name = "Test Venue",
                Address = "Test Address"
            };

            var seatManifest = new SeatManifest
            {
                SeatManifestId = seatManifestId,
                VenueId = venueId,
                Description = "Test Manifest",
                Venue = venue
            };

            var seats = new List<Seat>
            {
                new Seat
                {
                    SeatId = Guid.NewGuid(),
                    SeatManifestId = seatManifestId,
                    SeatType = "VIP",
                    Section = "A",
                    RowNumber = "1",
                    SeatNumber = "1",
                    Status = SeatStatus.Available
                }
            };

            seatManifest.Seats = seats;
            _dbContext.Venues.Add(venue);
            _dbContext.SeatManifests.Add(seatManifest);
            _dbContext.Seats.AddRange(seats);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _seatRepository.GetWithIncludesAsync(
                s => s.SeatId == seats[0].SeatId,
                s => s.SeatManifest);

            // Assert
            result.Should().HaveCount(1);
            result.First().SeatManifest.Should().NotBeNull();
            result.First().SeatManifest?.SeatManifestId.Should().Be(seatManifestId);
        }

        public void Dispose()
        {
            _dbContext?.Dispose();
        }
    }
}

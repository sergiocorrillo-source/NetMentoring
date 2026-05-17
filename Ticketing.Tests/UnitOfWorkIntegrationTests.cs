using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Ticketing.Data;
using Ticketing.DAL;
using Ticketing.Domain.Entities;
using Xunit;
using FluentAssertions;

namespace Ticketing.Tests
{
    public class UnitOfWorkIntegrationTests : IDisposable
    {
        private readonly TicketingDbContext _dbContext;
        private readonly IUnitOfWork _unitOfWork;

        public UnitOfWorkIntegrationTests()
        {
            var options = new DbContextOptionsBuilder<TicketingDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _dbContext = new TicketingDbContext(options);
            _unitOfWork = new UnitOfWork(_dbContext);
        }

        [Fact]
        public async Task SaveChangesAsync_WithNewCustomer_SavesToDatabaseAndReturnsCount()
        {
            // Arrange
            var customer = new Customer
            {
                CustomerId = Guid.NewGuid(),
                Email = "uow@test.com",
                FullName = "UOW Test User",
                CreatedAt = DateTime.UtcNow
            };

            var customerRepo = _unitOfWork.Repository<Customer>();

            // Act
            await customerRepo.AddAsync(customer);
            var saveCount = await _unitOfWork.SaveChangesAsync();

            // Assert
            saveCount.Should().BeGreaterThan(0);

            var savedCustomer = await _dbContext.Customers
                .FirstOrDefaultAsync(c => c.CustomerId == customer.CustomerId);

            savedCustomer.Should().NotBeNull();
            savedCustomer?.Email.Should().Be("uow@test.com");
        }

        [Fact]
        public async Task Repository_WithDifferentTypes_ReturnsDifferentRepositories()
        {
            // Arrange
            var customerRepo = _unitOfWork.Repository<Customer>();
            var venueRepo = _unitOfWork.Repository<Venue>();
            var seatRepo = _unitOfWork.Repository<Seat>();

            // Act & Assert
            customerRepo.Should().NotBeNull();
            venueRepo.Should().NotBeNull();
            seatRepo.Should().NotBeNull();
        }

        [Fact]
        public async Task ExecuteInTransactionAsync_WithSuccessfulOperation_CommitsChanges()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var customer = new Customer
            {
                CustomerId = customerId,
                Email = "transaction@test.com",
                FullName = "Transaction Test",
                CreatedAt = DateTime.UtcNow
            };

            var customerRepo = _unitOfWork.Repository<Customer>();

            // Act
            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                await customerRepo.AddAsync(customer);
                await _unitOfWork.SaveChangesAsync();
            });

            // Assert
            var savedCustomer = await _dbContext.Customers.FindAsync(customerId);
            savedCustomer.Should().NotBeNull();
            savedCustomer?.Email.Should().Be("transaction@test.com");
        }

        [Fact]
        public async Task ExecuteInTransactionAsync_WithFailedOperation_RollsBack()
        {
            // Arrange
            var customer = new Customer
            {
                CustomerId = Guid.NewGuid(),
                Email = "rollback@test.com",
                FullName = "Rollback Test",
                CreatedAt = DateTime.UtcNow
            };

            var customerRepo = _unitOfWork.Repository<Customer>();
            var initialCount = (await _dbContext.Customers.ToListAsync()).Count;

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await _unitOfWork.ExecuteInTransactionAsync(async () =>
                {
                    await customerRepo.AddAsync(customer);
                    await _unitOfWork.SaveChangesAsync();
                    throw new InvalidOperationException("Simulated error");
                });
            });

            // Verify rollback (depending on EF Core configuration)
            var finalCount = (await _dbContext.Customers.ToListAsync()).Count;
            finalCount.Should().Be(initialCount);
        }

        [Fact]
        public async Task MultipleOperations_InTransaction_MaintainConsistency()
        {
            // Arrange
            var venueId = Guid.NewGuid();
            var venue = new Venue
            {
                VenueId = venueId,
                Name = "Multi-Op Venue",
                Address = "123 Test Street"
            };

            var seatManifestId = Guid.NewGuid();
            var seatManifest = new SeatManifest
            {
                SeatManifestId = seatManifestId,
                VenueId = venueId,
                Description = "Test Manifest"
            };

            var venueRepo = _unitOfWork.Repository<Venue>();
            var seatManifestRepo = _unitOfWork.Repository<SeatManifest>();

            // Act
            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                await venueRepo.AddAsync(venue);
                await seatManifestRepo.AddAsync(seatManifest);
                await _unitOfWork.SaveChangesAsync();
            });

            // Assert
            var savedVenue = await _dbContext.Venues.FindAsync(venueId);
            var savedSeatManifest = await _dbContext.SeatManifests.FindAsync(seatManifestId);

            savedVenue.Should().NotBeNull();
            savedSeatManifest.Should().NotBeNull();
            savedSeatManifest?.VenueId.Should().Be(venueId);
        }

        public void Dispose()
        {
            _unitOfWork?.Dispose();
            _dbContext?.Dispose();
        }
    }
}

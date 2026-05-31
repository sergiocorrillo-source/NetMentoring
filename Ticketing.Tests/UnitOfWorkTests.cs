using Moq;
using System;
using System.Threading.Tasks;
using Ticketing.DAL;
using Ticketing.Domain.Entities;
using Xunit;
using FluentAssertions;

namespace Ticketing.Tests
{
    public class UnitOfWorkTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IRepository<Customer>> _mockCustomerRepo;

        public UnitOfWorkTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockCustomerRepo = new Mock<IRepository<Customer>>();
        }

        [Fact]
        public void Repository_WithValidType_ReturnsRepository()
        {
            // Arrange
            _mockUnitOfWork
                .Setup(u => u.Repository<Customer>())
                .Returns(_mockCustomerRepo.Object);

            // Act
            var result = _mockUnitOfWork.Object.Repository<Customer>();

            // Assert
            result.Should().NotBeNull();
            result.Should().Be(_mockCustomerRepo.Object);
        }

        [Fact]
        public async Task SaveChangesAsync_CallsSaveChanges()
        {
            // Arrange
            _mockUnitOfWork
                .Setup(u => u.SaveChangesAsync(default))
                .ReturnsAsync(5);

            // Act
            var result = await _mockUnitOfWork.Object.SaveChangesAsync();

            // Assert
            result.Should().Be(5);
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(default), Times.Once);
        }

        [Fact]
        public async Task ExecuteInTransactionAsync_ExecutesOperation()
        {
            // Arrange
            var operationExecuted = false;
            Func<Task> operation = async () =>
            {
                operationExecuted = true;
                await Task.CompletedTask;
            };

            _mockUnitOfWork
                .Setup(u => u.ExecuteInTransactionAsync(It.IsAny<Func<Task>>() ))
                .Returns<Func<Task>>(op => op());

            // Act
            await _mockUnitOfWork.Object.ExecuteInTransactionAsync(operation);

            // Assert
            operationExecuted.Should().BeTrue();
            _mockUnitOfWork.Verify(u => u.ExecuteInTransactionAsync(It.IsAny<Func<Task>>()), Times.Once);
        }

        [Fact]
        public void Dispose_CallsDispose()
        {
            // Arrange
            _mockUnitOfWork.Setup(u => u.Dispose());

            // Act
            _mockUnitOfWork.Object.Dispose();

            // Assert
            _mockUnitOfWork.Verify(u => u.Dispose(), Times.Once);
        }

        [Fact]
        public async Task ExecuteInTransactionAsync_WithException_PropagatesException()
        {
            // Arrange
            var operation = new Func<Task>(async () =>
            {
                await Task.CompletedTask;
                throw new InvalidOperationException("Operation failed");
            });

            _mockUnitOfWork
                .Setup(u => u.ExecuteInTransactionAsync(It.IsAny<Func<Task>>() ))
                .Returns<Func<Task>>(op => op());

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _mockUnitOfWork.Object.ExecuteInTransactionAsync(operation));

            ex.Message.Should().Be("Operation failed");
        }
    }
}

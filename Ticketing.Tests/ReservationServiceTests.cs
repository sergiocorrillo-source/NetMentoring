using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ticketing.DAL;
using Ticketing.Domain.Entities;
using Ticketing.Services;
using Xunit;
using FluentAssertions;

namespace Ticketing.Tests
{
    public class ReservationServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IRepository<Seat>> _mockSeatRepo;
        private readonly Mock<IRepository<Ticket>> _mockTicketRepo;
        private readonly ReservationService _reservationService;

        public ReservationServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockSeatRepo = new Mock<IRepository<Seat>>();
            _mockTicketRepo = new Mock<IRepository<Ticket>>();

            _mockUnitOfWork
                .Setup(u => u.Repository<Seat>())
                .Returns(_mockSeatRepo.Object);

            _mockUnitOfWork
                .Setup(u => u.Repository<Ticket>())
                .Returns(_mockTicketRepo.Object);

            _reservationService = new ReservationService(_mockUnitOfWork.Object);
        }

        [Fact]
        public async Task ReserveSeatAsync_WithAvailableSeat_CreatesTicketAndReservesSeat()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var seatId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var offerId = Guid.NewGuid();

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

            _mockTicketRepo
                .Setup(r => r.AddAsync(It.IsAny<Ticket>(), default))
                .Returns(Task.CompletedTask);

            _mockSeatRepo
                .Setup(r => r.Update(It.IsAny<Seat>()))
                .Callback<Seat>(s => s.Status = SeatStatus.Reserved);

            _mockUnitOfWork
                .Setup(u => u.SaveChangesAsync(default))
                .ReturnsAsync(2);

            _mockUnitOfWork
                .Setup(u => u.ExecuteInTransactionAsync(It.IsAny<Func<Task>>()))
                .Callback<Func<Task>>(async op => await op())
                .Returns(Task.CompletedTask);

            // Act
            var ticketId = await _reservationService.ReserveSeatAsync(eventId, seatId, customerId, offerId);

            // Assert
            ticketId.Should().NotBe(Guid.Empty);
            _mockSeatRepo.Verify(r => r.Update(It.IsAny<Seat>()), Times.Once);
            _mockTicketRepo.Verify(r => r.AddAsync(It.IsAny<Ticket>(), default), Times.Once);
        }

        [Fact]
        public async Task ReserveSeatAsync_WithUnavailableSeat_ThrowsException()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var seatId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var offerId = Guid.NewGuid();

            var seat = new Seat
            {
                SeatId = seatId,
                SeatManifestId = Guid.NewGuid(),
                SeatType = "VIP",
                Section = "A",
                RowNumber = "1",
                SeatNumber = "10",
                Status = SeatStatus.Sold
            };

            _mockSeatRepo
                .Setup(r => r.GetWithIncludesAsync(
                    It.IsAny<System.Linq.Expressions.Expression<System.Func<Seat, bool>>>(),
                    It.IsAny<System.Linq.Expressions.Expression<System.Func<Seat, object>>[]>()))
                .ReturnsAsync(new List<Seat> { seat });

            _mockUnitOfWork
                .Setup(u => u.ExecuteInTransactionAsync(It.IsAny<Func<Task>>()))
                .Callback<Func<Task>>(async op => await op())
                .Returns(Task.CompletedTask);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _reservationService.ReserveSeatAsync(eventId, seatId, customerId, offerId));

            ex.Message.Should().Contain("not available");
        }

        [Fact]
        public async Task ReserveSeatAsync_WithNonExistentSeat_ThrowsException()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var seatId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var offerId = Guid.NewGuid();

            _mockSeatRepo
                .Setup(r => r.GetWithIncludesAsync(
                    It.IsAny<System.Linq.Expressions.Expression<System.Func<Seat, bool>>>(),
                    It.IsAny<System.Linq.Expressions.Expression<System.Func<Seat, object>>[]>()))
                .ReturnsAsync(new List<Seat>());

            _mockUnitOfWork
                .Setup(u => u.ExecuteInTransactionAsync(It.IsAny<Func<Task>>()))
                .Callback<Func<Task>>(async op => await op())
                .Returns(Task.CompletedTask);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _reservationService.ReserveSeatAsync(eventId, seatId, customerId, offerId));

            ex.Message.Should().Contain("not found");
        }

        [Fact]
        public async Task ConfirmPurchaseAsync_WithValidTicket_ConfirmsTicketAndMarksSeatSold()
        {
            // Arrange
            var ticketId = Guid.NewGuid();
            var seatId = Guid.NewGuid();

            var seat = new Seat
            {
                SeatId = seatId,
                SeatManifestId = Guid.NewGuid(),
                SeatType = "VIP",
                Section = "A",
                RowNumber = "1",
                SeatNumber = "10",
                Status = SeatStatus.Reserved
            };

            var ticket = new Ticket
            {
                TicketId = ticketId,
                EventId = Guid.NewGuid(),
                SeatId = seatId,
                CustomerId = Guid.NewGuid(),
                OfferId = Guid.NewGuid(),
                Status = "Reserved",
                Seat = seat
            };

            _mockTicketRepo
                .Setup(r => r.GetWithIncludesAsync(
                    It.IsAny<System.Linq.Expressions.Expression<System.Func<Ticket, bool>>>(),
                    It.IsAny<System.Linq.Expressions.Expression<System.Func<Ticket, object>>[]>()))
                .ReturnsAsync(new List<Ticket> { ticket });

            _mockSeatRepo
                .Setup(r => r.Update(It.IsAny<Seat>()))
                .Callback<Seat>(s => s.Status = SeatStatus.Sold);

            _mockTicketRepo
                .Setup(r => r.Update(It.IsAny<Ticket>()))
                .Callback<Ticket>(t => t.Status = "Confirmed");

            _mockUnitOfWork
                .Setup(u => u.SaveChangesAsync(default))
                .ReturnsAsync(2);

            _mockUnitOfWork
                .Setup(u => u.ExecuteInTransactionAsync(It.IsAny<Func<Task>>()))
                .Callback<Func<Task>>(async op => await op())
                .Returns(Task.CompletedTask);

            // Act
            await _reservationService.ConfirmPurchaseAsync(ticketId);

            // Assert
            _mockSeatRepo.Verify(r => r.Update(It.IsAny<Seat>()), Times.Once);
            _mockTicketRepo.Verify(r => r.Update(It.IsAny<Ticket>()), Times.Once);
        }

        [Fact]
        public async Task ConfirmPurchaseAsync_WithNonExistentTicket_ThrowsException()
        {
            // Arrange
            var ticketId = Guid.NewGuid();

            _mockTicketRepo
                .Setup(r => r.GetWithIncludesAsync(
                    It.IsAny<System.Linq.Expressions.Expression<System.Func<Ticket, bool>>>(),
                    It.IsAny<System.Linq.Expressions.Expression<System.Func<Ticket, object>>[]>()))
                .ReturnsAsync(new List<Ticket>());

            _mockUnitOfWork
                .Setup(u => u.ExecuteInTransactionAsync(It.IsAny<Func<Task>>()))
                .Callback<Func<Task>>(async op => await op())
                .Returns(Task.CompletedTask);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _reservationService.ConfirmPurchaseAsync(ticketId));

            ex.Message.Should().Contain("not found");
        }
    }
}

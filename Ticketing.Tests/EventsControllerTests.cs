using Moq;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Ticketing.Api.Controllers;
using Ticketing.Services;
using Xunit;
using FluentAssertions;

namespace Ticketing.Tests
{
    public class EventsControllerTests
    {
        private readonly Mock<ReservationService> _mockReservationService;
        private readonly EventsController _controller;

        public EventsControllerTests()
        {
            _mockReservationService = new Mock<ReservationService>(null!);
            _controller = new EventsController(_mockReservationService.Object);
        }

        [Fact]
        public async Task Reserve_WithValidData_ReturnsCreatedAtAction()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var seatId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var offerId = Guid.NewGuid();
            var ticketId = Guid.NewGuid();

            _mockReservationService
                .Setup(s => s.ReserveSeatAsync(eventId, seatId, customerId, offerId))
                .ReturnsAsync(ticketId);

            // Act
            var result = await _controller.Reserve(eventId, seatId, customerId, offerId);

            // Assert
            var createdResult = result as CreatedAtActionResult;
            createdResult.Should().NotBeNull();
            createdResult?.ActionName.Should().Be(nameof(EventsController.Reserve));
            createdResult?.StatusCode.Should().Be(201);

            _mockReservationService.Verify(
                s => s.ReserveSeatAsync(eventId, seatId, customerId, offerId),
                Times.Once);
        }

        [Fact]
        public async Task Reserve_WithInvalidSeat_ReturnsBadRequest()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var seatId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var offerId = Guid.NewGuid();

            _mockReservationService
                .Setup(s => s.ReserveSeatAsync(eventId, seatId, customerId, offerId))
                .ThrowsAsync(new InvalidOperationException("Seat not found."));

            // Act
            var result = await _controller.Reserve(eventId, seatId, customerId, offerId);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult?.StatusCode.Should().Be(400);
        }

        [Fact]
        public async Task Reserve_WithUnavailableSeat_ReturnsBadRequest()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var seatId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var offerId = Guid.NewGuid();

            _mockReservationService
                .Setup(s => s.ReserveSeatAsync(eventId, seatId, customerId, offerId))
                .ThrowsAsync(new InvalidOperationException("Seat is not available."));

            // Act
            var result = await _controller.Reserve(eventId, seatId, customerId, offerId);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult?.StatusCode.Should().Be(400);
        }

        [Fact]
        public async Task Reserve_WithConcurrencyConflict_ReturnsConflict()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var seatId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var offerId = Guid.NewGuid();

            _mockReservationService
                .Setup(s => s.ReserveSeatAsync(eventId, seatId, customerId, offerId))
                .ThrowsAsync(new Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException("Concurrency conflict"));

            // Act
            var result = await _controller.Reserve(eventId, seatId, customerId, offerId);

            // Assert
            var conflictResult = result as ConflictObjectResult;
            conflictResult.Should().NotBeNull();
            conflictResult?.StatusCode.Should().Be(409);
        }

        [Fact]
        public async Task Reserve_ReturnsTicketIdInResponse()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var seatId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var offerId = Guid.NewGuid();
            var ticketId = Guid.NewGuid();

            _mockReservationService
                .Setup(s => s.ReserveSeatAsync(eventId, seatId, customerId, offerId))
                .ReturnsAsync(ticketId);

            // Act
            var result = await _controller.Reserve(eventId, seatId, customerId, offerId);

            // Assert
            var createdResult = result as CreatedAtActionResult;
            createdResult?.Value.Should().NotBeNull();
        }
    }
}

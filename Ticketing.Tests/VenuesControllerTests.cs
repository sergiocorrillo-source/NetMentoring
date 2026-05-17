using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Ticketing.Api.Controllers;
using Ticketing.Services;
using Ticketing.Services.DTOs;
using Xunit;
using FluentAssertions;

namespace Ticketing.Tests
{
    public class VenuesControllerTests
    {
        private readonly Mock<VenueService> _mockVenueService;
        private readonly VenuesController _controller;

        public VenuesControllerTests()
        {
            _mockVenueService = new Mock<VenueService>(null!);
            _controller = new VenuesController(_mockVenueService.Object);
        }

        [Fact]
        public async Task GetVenues_ReturnsOkWithVenues()
        {
            // Arrange
            var venues = new List<VenueDto>
            {
                new VenueDto { VenueId = Guid.NewGuid(), Name = "Stadium A", Address = "Address 1" },
                new VenueDto { VenueId = Guid.NewGuid(), Name = "Stadium B", Address = "Address 2" }
            };

            _mockVenueService
                .Setup(s => s.GetAllVenuesAsync())
                .ReturnsAsync(venues);

            // Act
            var result = await _controller.GetVenues();

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult?.StatusCode.Should().Be(200);

            var returnedVenues = okResult?.Value as IEnumerable<VenueDto>;
            returnedVenues.Should().HaveCount(2);

            _mockVenueService.Verify(s => s.GetAllVenuesAsync(), Times.Once);
        }

        [Fact]
        public async Task GetVenues_WithException_ReturnsInternalServerError()
        {
            // Arrange
            _mockVenueService
                .Setup(s => s.GetAllVenuesAsync())
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.GetVenues();

            // Assert
            var statusCodeResult = result.Result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult?.StatusCode.Should().Be(500);
        }

        [Fact]
        public async Task GetVenueSections_WithValidVenueId_ReturnsOkWithSections()
        {
            // Arrange
            var venueId = Guid.NewGuid();
            var sections = new List<SectionDto>
            {
                new SectionDto { SectionName = "A", TotalSeats = 100 },
                new SectionDto { SectionName = "B", TotalSeats = 150 }
            };

            _mockVenueService
                .Setup(s => s.GetVenueSectionsAsync(venueId))
                .ReturnsAsync(sections);

            // Act
            var result = await _controller.GetVenueSections(venueId);

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult?.StatusCode.Should().Be(200);

            var returnedSections = okResult?.Value as IEnumerable<SectionDto>;
            returnedSections.Should().HaveCount(2);

            _mockVenueService.Verify(s => s.GetVenueSectionsAsync(venueId), Times.Once);
        }

        [Fact]
        public async Task GetVenueSections_WithInvalidVenueId_ReturnsBadRequest()
        {
            // Arrange
            var venueId = Guid.NewGuid();

            _mockVenueService
                .Setup(s => s.GetVenueSectionsAsync(venueId))
                .ThrowsAsync(new InvalidOperationException("Venue not found."));

            // Act
            var result = await _controller.GetVenueSections(venueId);

            // Assert
            var statusCodeResult = result.Result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult?.StatusCode.Should().Be(500);
        }
    }
}

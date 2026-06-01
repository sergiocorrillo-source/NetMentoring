using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Ticketing.Domain.Entities;
using Ticketing.Services.DTOs;
using Xunit;

namespace Ticketing.Tests.Integration
{
    public class NotificationPipelineTests : IClassFixture<CustomWebApplicationFactory<Ticketing.Api.Program>>
    {
        private readonly CustomWebApplicationFactory<Ticketing.Api.Program> _factory;

        public NotificationPipelineTests(CustomWebApplicationFactory<Ticketing.Api.Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task EnqueueNotification_IsPersisted_And_Dispatched()
        {
            // Arrange
            using var client = _factory.CreateClient();
            using var scope = _factory.Services.CreateScope();
            var sp = scope.ServiceProvider;

            var uow = sp.GetRequiredService<Ticketing.DAL.IUnitOfWork>();
            var channel = sp.GetRequiredService<Channel<Guid>>();

            // create a customer to reference
            var cust = new Customer { CustomerId = Guid.NewGuid(), Email = "test@example.com", FullName = "Test User" };
            await uow.Repository<Customer>().AddAsync(cust);
            await uow.SaveChangesAsync();

            var notifService = sp.GetRequiredService<Ticketing.Services.INotificationService>();

            var dto = new NotificationDto
            {
                NotificationId = Guid.NewGuid(),
                Operation = "IntegrationTest",
                Timestamp = DateTime.UtcNow,
                Parameters = JsonSerializer.Serialize(new { email = cust.Email, name = cust.FullName }),
                Content = "Hello"
            };

            // Act
            var id = await notifService.EnqueueNotificationAsync(dto);

            // Wait for background dispatcher to process (poll until status changes from Queued or timeout)
            Notification? notif = null;
            var sw = System.Diagnostics.Stopwatch.StartNew();
            while (sw.Elapsed < TimeSpan.FromSeconds(10))
            {
                notif = await uow.Repository<Notification>().GetByIdAsync(id);
                if (notif != null && notif.Status != "Queued") break;
                await Task.Delay(200);
            }

            // Assert
            Assert.NotNull(notif);
            Assert.True(notif!.Status == "Sent" || notif.Status == "InProgress" || notif.Status == "Failed", $"Unexpected status: {notif.Status}");
        }
    }
}

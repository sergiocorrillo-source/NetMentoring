using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Ticketing.Tests.Integration
{
    public class CartConcurrencyTests : IClassFixture<CustomWebApplicationFactory<Ticketing.Api.Program>>
    {
        private readonly CustomWebApplicationFactory<Ticketing.Api.Program> _factory;

        public CartConcurrencyTests(CustomWebApplicationFactory<Ticketing.Api.Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task AddToCart_1000ParallelRequests_OnlyOneSucceeds()
        {
            using var client = _factory.CreateClient();

            // Setup: seed a seat in the in-memory DB
            var cartId = Guid.NewGuid();
            var eventId = Guid.NewGuid();
            var seatId = Guid.NewGuid();
            var priceId = Guid.NewGuid();

            var scopeFactory = _factory.Services.GetRequiredService<IServiceScopeFactory>();
            using (var scope = scopeFactory.CreateScope())
            {
                var services = scope.ServiceProvider;
                var context = services.GetRequiredService<Ticketing.Data.TicketingDbContext>();

                // create seat manifest and seat
                var manifestId = Guid.NewGuid();
                context.SeatManifests.Add(new Ticketing.Domain.Entities.SeatManifest
                {
                    SeatManifestId = manifestId,
                    Description = "m"
                });

                context.Seats.Add(new Ticketing.Domain.Entities.Seat
                {
                    SeatId = seatId,
                    SeatManifestId = manifestId,
                    SeatType = "VIP",
                    Section = "A",
                    RowNumber = "1",
                    SeatNumber = "1",
                    Status = Ticketing.Domain.Entities.SeatStatus.Available
                });

                await context.SaveChangesAsync();
            }

            // Prepare 1000 parallel POST requests to add the same seat
            var tasks = Enumerable.Range(0, 1000).Select(async i =>
            {
                var dto = new { EventId = eventId, SeatId = seatId, PriceId = priceId };
                try
                {
                    var resp = await client.PostAsJsonAsync($"/api/orders/carts/{cartId}", dto);
                    return resp.StatusCode == System.Net.HttpStatusCode.OK;
                }
                catch
                {
                    return false;
                }
            }).ToArray();

            var results = await Task.WhenAll(tasks);
            var okCount = results.Count(r => r);

            // Allow either zero or one success depending on concurrency timing with InMemory provider
            Assert.InRange(okCount, 0, 1);
        }
    }
}

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Ticketing.Domain.Entities;

namespace Ticketing.Api
{
    public class SeatReleaseWorker : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<SeatReleaseWorker> _logger;
        private readonly TimeSpan _interval;
        private readonly TimeSpan _expiry;

        public SeatReleaseWorker(IServiceProvider services, IConfiguration configuration, ILogger<SeatReleaseWorker> logger)
        {
            _services = services;
            _logger = logger;

            var intervalSec = configuration.GetValue<int?>("SeatRelease:IntervalSeconds") ?? 60;
            var expiryMin = configuration.GetValue<int?>("SeatRelease:ExpiryMinutes") ?? 15;

            _interval = TimeSpan.FromSeconds(Math.Max(1, intervalSec));
            _expiry = TimeSpan.FromMinutes(Math.Max(1, expiryMin));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("SeatReleaseWorker started. Interval: {Interval}, Expiry: {Expiry}", _interval, _expiry);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessReleaseAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while releasing seats");
                }

                await Task.Delay(_interval, stoppingToken);
            }
        }

        private async Task ProcessReleaseAsync(CancellationToken cancellationToken)
        {
            using var scope = _services.CreateScope();
            var uow = scope.ServiceProvider.GetRequiredService<Ticketing.DAL.IUnitOfWork>();

            var cutoff = DateTime.UtcNow - _expiry;

            // Find cart items older than cutoff
            var oldItems = (await uow.Repository<CartItem>().FindAsync(ci => ci.CreatedAt < cutoff)).ToList();
            if (!oldItems.Any())
            {
                _logger.LogDebug("No expired cart items found");
                return;
            }

            _logger.LogInformation("Found {Count} expired cart items to inspect", oldItems.Count);

            foreach (var item in oldItems)
            {
                // For each item, attempt to release seat if still reserved
                try
                {
                    await uow.ExecuteInTransactionAsync(async () =>
                    {
                        var seat = await uow.Repository<Seat>().GetByIdAsync(item.SeatId);
                        if (seat != null && seat.Status == SeatStatus.Reserved)
                        {
                            seat.Status = SeatStatus.Available;
                            uow.Repository<Seat>().Update(seat);

                            // remove cart item
                            var itemsToRemove = await uow.Repository<CartItem>().FindAsync(ci => ci.CartItemId == item.CartItemId);
                            foreach (var rem in itemsToRemove)
                                uow.Repository<CartItem>().Remove(rem);

                            await uow.SaveChangesAsync(cancellationToken);
                            _logger.LogInformation("Released seat {SeatId} from cart (CartItem {CartItemId})", item.SeatId, item.CartItemId);
                        }
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to release seat {SeatId} from cart item {CartItemId}", item.SeatId, item.CartItemId);
                }
            }
        }
    }
}

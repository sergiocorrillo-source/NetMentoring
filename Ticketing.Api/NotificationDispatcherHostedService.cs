using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Ticketing.DAL;
using Ticketing.Domain.Entities;
using Ticketing.Services;

namespace Ticketing.Api
{
    public class NotificationDispatcherHostedService : BackgroundService
    {
        private readonly Channel<Guid> _channel;
        private readonly IServiceProvider _serviceProvider;
        private readonly IEmailProvider _emailProvider;
        private readonly ILogger<NotificationDispatcherHostedService> _logger;

        public NotificationDispatcherHostedService(Channel<Guid> channel, IServiceProvider serviceProvider, IEmailProvider emailProvider, ILogger<NotificationDispatcherHostedService> logger)
        {
            _channel = channel;
            _serviceProvider = serviceProvider;
            _emailProvider = emailProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await foreach (var id in _channel.Reader.ReadAllAsync(stoppingToken))
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var uow = scope.ServiceProvider.GetRequiredService<Ticketing.DAL.IUnitOfWork>();

                    var notif = await uow.Repository<Notification>().GetByIdAsync(id);
                    if (notif == null)
                    {
                        _logger.LogWarning("Notification {Id} not found in DB", id);
                        continue;
                    }

                    // Skip if another processor already handled it (status changed)
                    if (notif.Status != "Queued")
                    {
                        _logger.LogInformation("Notification {Id} status is {Status}, skipping dispatcher.", id, notif.Status);
                        continue;
                    }

                    notif.Status = "InProgress";
                    uow.Repository<Notification>().Update(notif);
                    await uow.SaveChangesAsync();

                    var param = JsonSerializer.Deserialize<NotificationParams>(notif.Parameters);
                    var subject = notif.Operation;
                    var body = notif.Content;

                    var ok = await _emailProvider.SendEmailAsync(param?.Email ?? "noreply@example.com", subject, body);

                    notif.Status = ok ? "Sent" : "Failed";
                    notif.Result = ok ? "OK" : "Provider failed";
                    uow.Repository<Notification>().Update(notif);
                    await uow.SaveChangesAsync();

                    _logger.LogInformation("Notification {Id} processed. Result: {Result}", id, notif.Result);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing notification {Id}", id);
                }
            }
        }

        private class NotificationParams
        {
            public string? Email { get; set; }
            public string? Name { get; set; }
        }
    }
}

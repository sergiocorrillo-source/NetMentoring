using Microsoft.EntityFrameworkCore;
using Ticketing.Data;
using Ticketing.Api;
using Microsoft.EntityFrameworkCore;
using Ticketing.DAL;
using Ticketing.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
// In-memory caching
builder.Services.AddMemoryCache();
            // Notification pipeline: in-memory channel + dispatcher
            var notificationChannel = System.Threading.Channels.Channel.CreateUnbounded<Guid>();
            builder.Services.AddSingleton<System.Threading.Channels.Channel<Guid>>(notificationChannel);
            builder.Services.AddScoped<INotificationService, NotificationService>();
            // Email provider: prefer SendGrid if configured, otherwise fallback to simple provider
            builder.Services.AddSingleton<IEmailProvider>(sp =>
            {
                var cfg = sp.GetRequiredService<Microsoft.Extensions.Configuration.IConfiguration>();
                var apiKey = cfg["SendGrid:ApiKey"];
                if (!string.IsNullOrEmpty(apiKey))
                    return sp.GetRequiredService<Ticketing.Services.SendGridEmailProvider>();

                return new Ticketing.Services.EmailProvider(sp.GetService<Microsoft.Extensions.Logging.ILogger<Ticketing.Services.EmailProvider>>());
            });
            builder.Services.AddSingleton<Ticketing.Services.SendGridEmailProvider>();
            builder.Services.AddHostedService<NotificationDispatcherHostedService>();
            // Background worker to release reserved seats from carts after expiry
            builder.Services.AddHostedService<SeatReleaseWorker>();

// DbContext
var connectionString = builder.Configuration.GetConnectionString("Default") ?? "Server=NBCORAR2433;Database=TicketingDb;Trusted_Connection=True;";
if (builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddDbContext<TicketingDbContext>(options => options.UseInMemoryDatabase("TestDb"));
}
else
{
    builder.Services.AddDbContext<TicketingDbContext>(options => options.UseSqlServer(connectionString));
}

// DI registrations
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddScoped<IVenueService, VenueService>();
builder.Services.AddScoped<EventService>();
builder.Services.AddScoped<CartService>();
builder.Services.AddScoped<CartService>();
builder.Services.AddScoped<PaymentService>();
builder.Services.AddScoped<IOrderService, OrderService>();

var app = builder.Build();

app.MapControllers();

app.Run();

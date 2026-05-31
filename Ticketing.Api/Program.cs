using Microsoft.EntityFrameworkCore;
using Ticketing.Data;
using Ticketing.DAL;
using Ticketing.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// DbContext
var connectionString = builder.Configuration.GetConnectionString("Default") ?? "Server=NBCORAR2433;Database=TicketingDb;Trusted_Connection=True;";
builder.Services.AddDbContext<TicketingDbContext>(options => options.UseSqlServer(connectionString));

// DI registrations
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddScoped<IVenueService, VenueService>();
builder.Services.AddScoped<EventService>();
builder.Services.AddScoped<CartService>();
builder.Services.AddScoped<PaymentService>();
builder.Services.AddScoped<IOrderService, OrderService>();

var app = builder.Build();

app.MapControllers();

app.Run();

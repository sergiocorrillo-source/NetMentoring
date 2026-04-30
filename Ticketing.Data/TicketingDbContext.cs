using Microsoft.EntityFrameworkCore;
using Ticketing.Domain.Entities;

namespace Ticketing.Data
{
    public class TicketingDbContext : DbContext
    {
        public TicketingDbContext(DbContextOptions<TicketingDbContext> options) : base(options) { }

        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<Venue> Venues => Set<Venue>();
        public DbSet<SeatManifest> SeatManifests => Set<SeatManifest>();
        public DbSet<Seat> Seats => Set<Seat>();
        public DbSet<EventManager> EventManagers => Set<EventManager>();
        public DbSet<Event> Events => Set<Event>();
        public DbSet<Price> Prices => Set<Price>();
        public DbSet<Offer> Offers => Set<Offer>();
        public DbSet<Ticket> Tickets => Set<Ticket>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Seat concurrency token
            modelBuilder.Entity<Seat>()
                .Property(s => s.RowVersion)
                .IsRowVersion();

            // Relations
            modelBuilder.Entity<SeatManifest>()
                .HasOne(sm => sm.Venue)
                .WithMany(v => v.SeatManifests)
                .HasForeignKey(sm => sm.VenueId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Seat>()
                .HasOne(s => s.SeatManifest)
                .WithMany(sm => sm.Seats)
                .HasForeignKey(s => s.SeatManifestId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Event>()
                .HasOne(e => e.Venue)
                .WithMany()
                .HasForeignKey(e => e.VenueId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Event>()
                .HasOne(e => e.SeatManifest)
                .WithMany()
                .HasForeignKey(e => e.SeatManifestId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Offer>()
                .HasOne(o => o.Price)
                .WithMany()
                .HasForeignKey(o => o.PriceId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.Seat)
                .WithMany()
                .HasForeignKey(t => t.SeatId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.Event)
                .WithMany(e => e.Tickets)
                .HasForeignKey(t => t.EventId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.Customer)
                .WithMany()
                .HasForeignKey(t => t.CustomerId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.Offer)
                .WithMany(o => o.Tickets)
                .HasForeignKey(t => t.OfferId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

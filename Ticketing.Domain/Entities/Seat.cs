using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ticketing.Domain.Entities
{
    public class Seat
    {
        [Key]
        public Guid SeatId { get; set; }

        public Guid SeatManifestId { get; set; }

        [Required, MaxLength(100)]
        public string SeatType { get; set; } = null!;

        [Required, MaxLength(50)]
        public string Section { get; set; } = null!;

        [Required, MaxLength(20)]
        public string RowNumber { get; set; } = null!;

        [Required, MaxLength(20)]
        public string SeatNumber { get; set; } = null!;

        public SeatStatus Status { get; set; } = SeatStatus.Available;

        [ForeignKey(nameof(SeatManifestId))]
        public SeatManifest? SeatManifest { get; set; }

        // Concurrency token for optimistic concurrency control
        [Timestamp]
        public byte[]? RowVersion { get; set; }
    }
}

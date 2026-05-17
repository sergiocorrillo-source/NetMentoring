using System;

namespace Ticketing.Services.DTOs
{
    public class SeatDto
    {
        public Guid SeatId { get; set; }
        public string Section { get; set; } = null!;
        public string RowNumber { get; set; } = null!;
        public string SeatNumber { get; set; } = null!;
        public string Status { get; set; } = null!;
        public string SeatType { get; set; } = null!;
    }
}

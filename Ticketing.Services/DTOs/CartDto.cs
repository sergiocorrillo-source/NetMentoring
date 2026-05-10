using System;
using System.Collections.Generic;

namespace Ticketing.Services.DTOs
{
    public class CartDto
    {
        public Guid CartId { get; set; }
        public List<CartItemDto> Items { get; set; } = new();
        public decimal TotalAmount { get; set; }
    }
}

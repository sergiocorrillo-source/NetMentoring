using System;
using System.Collections.Generic;

namespace Ticketing.Services.DTOs
{
    public class SectionDto
    {
        public string Section { get; set; } = null!;
        public List<string> Rows { get; set; } = new();
    }
}

using System;

namespace POS.Application.DTOs
{
    public class RecentActivityDto
    {
        public DateTime Timestamp { get; set; }
        public string UserName { get; set; }
        public string Action { get; set; }
        public string Description { get; set; }
    }
}

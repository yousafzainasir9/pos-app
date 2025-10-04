using System;

namespace POS.Application.DTOs
{
    public class UserActivityDto
    {
        public DateTime Timestamp { get; set; }
        public string Action { get; set; }
        public string Description { get; set; }
        public string IpAddress { get; set; }
    }
}

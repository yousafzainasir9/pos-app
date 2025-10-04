using System;

namespace POS.Application.DTOs
{
    public class AuditLogDto
    {
        public long Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Action { get; set; }
        public string Entity { get; set; }
        public string EntityId { get; set; }
        public string Details { get; set; }
        public string IpAddress { get; set; }
    }
}

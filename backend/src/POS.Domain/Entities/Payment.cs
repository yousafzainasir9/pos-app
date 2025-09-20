using POS.Domain.Common;
using POS.Domain.Enums;

namespace POS.Domain.Entities;

public class Payment : BaseEntity
{
    public decimal Amount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public string? ReferenceNumber { get; set; }
    public string? CardLastFourDigits { get; set; }
    public string? CardType { get; set; }
    public DateTime PaymentDate { get; set; }
    public string? Notes { get; set; }
    
    // Foreign keys
    public long OrderId { get; set; }
    public long ProcessedByUserId { get; set; }
    
    // Navigation properties
    public virtual Order Order { get; set; } = null!;
    public virtual User ProcessedByUser { get; set; } = null!;
}

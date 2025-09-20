using POS.Domain.Common;

namespace POS.Domain.Entities;

public class OrderItem : BaseEntity
{
    public int Quantity { get; set; }
    public decimal UnitPriceExGst { get; set; }
    public decimal UnitGstAmount { get; set; }
    public decimal UnitPriceIncGst { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Notes { get; set; }
    public bool IsVoided { get; set; }
    public DateTime? VoidedAt { get; set; }
    public string? VoidReason { get; set; }
    
    // Foreign keys
    public long OrderId { get; set; }
    public long ProductId { get; set; }
    public long? VoidedByUserId { get; set; }
    
    // Navigation properties
    public virtual Order Order { get; set; } = null!;
    public virtual Product Product { get; set; } = null!;
    public virtual User? VoidedByUser { get; set; }
}

using POS.Domain.Common;
using POS.Domain.Enums;

namespace POS.Domain.Entities;

public class Order : BaseEntity
{
    public required string OrderNumber { get; set; }
    public DateTime OrderDate { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public OrderType OrderType { get; set; } = OrderType.DineIn;
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal ChangeAmount { get; set; }
    public string? Notes { get; set; }
    public string? TableNumber { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? CancellationReason { get; set; }
    
    // Foreign keys
    public long? CustomerId { get; set; }
    public long StoreId { get; set; }
    public long UserId { get; set; } // Cashier/Employee
    public long? ShiftId { get; set; }
    
    // Navigation properties
    public virtual Customer? Customer { get; set; }
    public virtual Store Store { get; set; } = null!;
    public virtual User User { get; set; } = null!;
    public virtual Shift? Shift { get; set; }
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}

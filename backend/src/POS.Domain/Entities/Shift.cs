using POS.Domain.Common;
using POS.Domain.Enums;

namespace POS.Domain.Entities;

public class Shift : BaseEntity
{
    public required string ShiftNumber { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public decimal StartingCash { get; set; }
    public decimal? EndingCash { get; set; }
    public decimal? CashSales { get; set; }
    public decimal? CardSales { get; set; }
    public decimal? OtherSales { get; set; }
    public decimal? TotalSales { get; set; }
    public int? TotalOrders { get; set; }
    public ShiftStatus Status { get; set; } = ShiftStatus.Open;
    public string? Notes { get; set; }
    
    // Foreign keys
    public long UserId { get; set; }
    public long StoreId { get; set; }
    public long? ClosedByUserId { get; set; }
    
    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual Store Store { get; set; } = null!;
    public virtual User? ClosedByUser { get; set; }
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}

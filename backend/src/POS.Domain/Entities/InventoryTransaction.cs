using POS.Domain.Common;
using POS.Domain.Enums;

namespace POS.Domain.Entities;

public class InventoryTransaction : BaseEntity
{
    public InventoryTransactionType TransactionType { get; set; }
    public int Quantity { get; set; }
    public int StockBefore { get; set; }
    public int StockAfter { get; set; }
    public decimal? UnitCost { get; set; }
    public decimal? TotalCost { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? Notes { get; set; }
    public DateTime TransactionDate { get; set; }
    
    // Foreign keys
    public long ProductId { get; set; }
    public long StoreId { get; set; }
    public long UserId { get; set; }
    public long? OrderId { get; set; }
    public long? SupplierId { get; set; }
    
    // Navigation properties
    public virtual Product Product { get; set; } = null!;
    public virtual Store Store { get; set; } = null!;
    public virtual User User { get; set; } = null!;
    public virtual Order? Order { get; set; }
    public virtual Supplier? Supplier { get; set; }
}

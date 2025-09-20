using POS.Domain.Common;

namespace POS.Domain.Entities;

public class Product : BaseEntity
{
    public required string Name { get; set; }
    public required string Slug { get; set; }
    public string? SKU { get; set; }
    public string? Barcode { get; set; }
    public string? Description { get; set; }
    public decimal PriceExGst { get; set; }
    public decimal GstAmount { get; set; }
    public decimal PriceIncGst { get; set; }
    public decimal? Cost { get; set; }
    public string? PackNotes { get; set; }
    public int? PackSize { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public bool TrackInventory { get; set; } = true;
    public int StockQuantity { get; set; }
    public int LowStockThreshold { get; set; }
    public int DisplayOrder { get; set; }
    
    // Foreign keys
    public long SubcategoryId { get; set; }
    public long? SupplierId { get; set; }
    
    // Navigation properties
    public virtual Subcategory Subcategory { get; set; } = null!;
    public virtual Supplier? Supplier { get; set; }
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public virtual ICollection<InventoryTransaction> InventoryTransactions { get; set; } = new List<InventoryTransaction>();
}

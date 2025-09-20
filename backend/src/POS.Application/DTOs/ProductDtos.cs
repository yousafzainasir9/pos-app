namespace POS.Application.DTOs;

public class CategoryDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public List<SubcategoryDto> Subcategories { get; set; } = new();
}

public class SubcategoryDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public long CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
}

public class ProductDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
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
    public bool IsActive { get; set; }
    public bool TrackInventory { get; set; }
    public int StockQuantity { get; set; }
    public int LowStockThreshold { get; set; }
    public long SubcategoryId { get; set; }
    public string SubcategoryName { get; set; } = string.Empty;
    public long CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
}

public class ProductListDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? SKU { get; set; }
    public decimal PriceIncGst { get; set; }
    public int StockQuantity { get; set; }
    public bool IsActive { get; set; }
    public string? ImageUrl { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string SubcategoryName { get; set; } = string.Empty;
}

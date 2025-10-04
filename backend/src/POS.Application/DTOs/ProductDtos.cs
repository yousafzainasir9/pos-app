using POS.Application.DTOs.Categories;
using POS.Application.DTOs.Suppliers;

namespace POS.Application.DTOs;

public class ProductDto
{
    public long Id { get; set; }
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
    public bool IsActive { get; set; }
    public bool TrackInventory { get; set; }
    public int StockQuantity { get; set; }
    public int LowStockThreshold { get; set; }
    public int DisplayOrder { get; set; }
    public long SubcategoryId { get; set; }
    public string? SubcategoryName { get; set; }
    public long? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public long? SupplierId { get; set; }
    public string? SupplierName { get; set; }
}

public class ProductListDto
{
    public long Id { get; set; }
    public required string Name { get; set; }
    public required string Slug { get; set; }
    public string? SKU { get; set; }
    public string? Barcode { get; set; }
    public string? Description { get; set; }
    public decimal PriceExGst { get; set; }
    public decimal GstAmount { get; set; }
    public decimal PriceIncGst { get; set; }
    public decimal? Cost { get; set; }
    public int? PackSize { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; }
    public bool TrackInventory { get; set; }
    public int StockQuantity { get; set; }
    public int LowStockThreshold { get; set; }
    public long SubcategoryId { get; set; }
    public Categories.SubcategoryDto? Subcategory { get; set; }
    public Categories.CategoryDto? Category { get; set; }
    public long? SupplierId { get; set; }
    public Suppliers.SupplierDto? Supplier { get; set; }
}

public class CreateProductDto
{
    public required string Name { get; set; }
    public string? Slug { get; set; }
    public string? SKU { get; set; }
    public string? Barcode { get; set; }
    public string? Description { get; set; }
    public decimal PriceExGst { get; set; }
    public decimal GstAmount { get; set; }
    public decimal PriceIncGst { get; set; }
    public decimal? Cost { get; set; }
    public string? PackNotes { get; set; }
    public int? PackSize { get; set; } = 1;
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public bool TrackInventory { get; set; } = true;
    public int StockQuantity { get; set; }
    public int LowStockThreshold { get; set; } = 10;
    public int DisplayOrder { get; set; }
    public long SubcategoryId { get; set; }
    public long? SupplierId { get; set; }
}

public class UpdateProductDto
{
    public required string Name { get; set; }
    public string? Slug { get; set; }
    public string? SKU { get; set; }
    public string? Barcode { get; set; }
    public string? Description { get; set; }
    public decimal PriceExGst { get; set; }
    public decimal GstAmount { get; set; }
    public decimal PriceIncGst { get; set; }
    public decimal? Cost { get; set; }
    public string? PackNotes { get; set; }
    public int? PackSize { get; set; } = 1;
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public bool TrackInventory { get; set; } = true;
    public int StockQuantity { get; set; }
    public int LowStockThreshold { get; set; } = 10;
    public int DisplayOrder { get; set; }
    public long SubcategoryId { get; set; }
    public long? SupplierId { get; set; }
}

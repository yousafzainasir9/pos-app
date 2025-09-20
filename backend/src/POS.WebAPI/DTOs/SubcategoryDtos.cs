namespace POS.WebAPI.DTOs;

public class SubcategoryDto
{
    public long Id { get; set; }
    public required string Name { get; set; }
    public required string Slug { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public long CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public int ProductCount { get; set; }
}

public class SubcategoryDetailDto
{
    public long Id { get; set; }
    public required string Name { get; set; }
    public required string Slug { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public long CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public List<ProductSummaryDto> Products { get; set; } = new();
}

public class ProductSummaryDto
{
    public long Id { get; set; }
    public required string Name { get; set; }
    public string? SKU { get; set; }
    public decimal PriceIncGst { get; set; }
    public int StockQuantity { get; set; }
    public bool IsActive { get; set; }
}

public class CreateSubcategoryDto
{
    public required string Name { get; set; }
    public string? Slug { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public long CategoryId { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateSubcategoryDto : CreateSubcategoryDto
{
}

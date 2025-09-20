namespace POS.WebAPI.DTOs;

public class CategoryDto
{
    public long Id { get; set; }
    public required string Name { get; set; }
    public required string Slug { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public int SubcategoryCount { get; set; }
    public int ProductCount { get; set; }
}

public class CategoryDetailDto
{
    public long Id { get; set; }
    public required string Name { get; set; }
    public required string Slug { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public List<SubcategoryDto> Subcategories { get; set; } = new();
}

public class CreateCategoryDto
{
    public required string Name { get; set; }
    public string? Slug { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateCategoryDto : CreateCategoryDto
{
}

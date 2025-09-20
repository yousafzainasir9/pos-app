using POS.Domain.Common;

namespace POS.Domain.Entities;

public class Subcategory : BaseEntity
{
    public required string Name { get; set; }
    public required string Slug { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
    
    // Foreign key
    public long CategoryId { get; set; }
    
    // Navigation properties
    public virtual Category Category { get; set; } = null!;
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}

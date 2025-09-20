using POS.Domain.Common;

namespace POS.Domain.Entities;

public class Category : BaseEntity
{
    public required string Name { get; set; }
    public required string Slug { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public virtual ICollection<Subcategory> Subcategories { get; set; } = new List<Subcategory>();
}

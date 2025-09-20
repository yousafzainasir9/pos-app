using POS.Domain.Common;
using POS.Domain.Enums;

namespace POS.Domain.Entities;

public class User : BaseEntity
{
    public required string Username { get; set; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public string FullName => $"{FirstName} {LastName}";
    public string? Phone { get; set; }
    public string? Pin { get; set; } // For quick POS access
    public UserRole Role { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? LastLoginAt { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }
    
    // Foreign keys
    public long? StoreId { get; set; }
    
    // Navigation properties
    public virtual Store? Store { get; set; }
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    public virtual ICollection<Shift> Shifts { get; set; } = new List<Shift>();
}

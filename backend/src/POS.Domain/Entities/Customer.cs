using POS.Domain.Common;

namespace POS.Domain.Entities;

public class Customer : BaseEntity
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public string FullName => $"{FirstName} {LastName}";
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
    public decimal TotalPurchases { get; set; }
    public int TotalOrders { get; set; }
    public DateTime? LastOrderDate { get; set; }
    public string? LoyaltyCardNumber { get; set; }
    public int LoyaltyPoints { get; set; }
    
    // Navigation properties
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}

namespace POS.Domain.Entities.Audit;

/// <summary>
/// Tracks all entity changes in the system for audit purposes
/// </summary>
public class AuditLog
{
    public long Id { get; set; }
    
    /// <summary>
    /// When the action occurred
    /// </summary>
    public DateTime Timestamp { get; set; }
    
    /// <summary>
    /// User who performed the action
    /// </summary>
    public long? UserId { get; set; }
    public string? UserName { get; set; }
    
    /// <summary>
    /// Type of action performed (Create, Update, Delete, etc.)
    /// </summary>
    public string Action { get; set; } = string.Empty;
    
    /// <summary>
    /// Entity type that was affected (Product, Order, User, etc.)
    /// </summary>
    public string EntityName { get; set; } = string.Empty;
    
    /// <summary>
    /// ID of the entity that was affected
    /// </summary>
    public string? EntityId { get; set; }
    
    /// <summary>
    /// Old values (JSON) before the change
    /// </summary>
    public string? OldValues { get; set; }
    
    /// <summary>
    /// New values (JSON) after the change
    /// </summary>
    public string? NewValues { get; set; }
    
    /// <summary>
    /// Additional details about the action
    /// </summary>
    public string? Details { get; set; }
    
    /// <summary>
    /// IP address of the user
    /// </summary>
    public string? IpAddress { get; set; }
    
    /// <summary>
    /// User agent (browser info)
    /// </summary>
    public string? UserAgent { get; set; }
    
    /// <summary>
    /// Store context if applicable
    /// </summary>
    public long? StoreId { get; set; }
    
    // Navigation property
    public virtual User? User { get; set; }
    public virtual Store? Store { get; set; }
}

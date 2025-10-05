namespace POS.Domain.Entities.Audit;

/// <summary>
/// Tracks security-related events (login attempts, permission changes, etc.)
/// </summary>
public class SecurityLog
{
    public long Id { get; set; }
    
    /// <summary>
    /// When the security event occurred
    /// </summary>
    public DateTime Timestamp { get; set; }
    
    /// <summary>
    /// Type of security event
    /// </summary>
    public SecurityEventType EventType { get; set; }
    
    /// <summary>
    /// Severity level of the event
    /// </summary>
    public SecuritySeverity Severity { get; set; }
    
    /// <summary>
    /// User involved (may be null for failed login attempts)
    /// </summary>
    public long? UserId { get; set; }
    public string? UserName { get; set; }
    
    /// <summary>
    /// Description of the security event
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// IP address from which the event originated
    /// </summary>
    public string? IpAddress { get; set; }
    
    /// <summary>
    /// User agent (browser/device info)
    /// </summary>
    public string? UserAgent { get; set; }
    
    /// <summary>
    /// Whether the action was successful
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Additional metadata (JSON)
    /// </summary>
    public string? Metadata { get; set; }
    
    /// <summary>
    /// Store context if applicable
    /// </summary>
    public long? StoreId { get; set; }
    
    // Navigation properties
    public virtual User? User { get; set; }
    public virtual Store? Store { get; set; }
}

/// <summary>
/// Types of security events tracked
/// </summary>
public enum SecurityEventType
{
    Login = 1,
    Logout = 2,
    LoginFailed = 3,
    PasswordChanged = 4,
    PasswordReset = 5,
    UserCreated = 6,
    UserModified = 7,
    UserDeleted = 8,
    RoleChanged = 9,
    PermissionChanged = 10,
    TokenRefreshed = 11,
    UnauthorizedAccess = 12,
    SessionExpired = 13,
    AccountLocked = 14,
    AccountUnlocked = 15,
    TwoFactorEnabled = 16,
    TwoFactorDisabled = 17,
    SuspiciousActivity = 18
}

/// <summary>
/// Severity levels for security events
/// </summary>
public enum SecuritySeverity
{
    Info = 1,       // Normal operations
    Warning = 2,    // Potential issues
    Critical = 3,   // Security violations
    Emergency = 4   // Immediate attention required
}

using POS.Domain.Common;

namespace POS.Domain.Entities.Settings;

/// <summary>
/// System-wide settings stored in key-value pairs
/// </summary>
public class SystemSetting : BaseEntity
{
    public required string Key { get; set; }
    public required string Value { get; set; }
    public string? Description { get; set; }
    public string Category { get; set; } = "General"; // General, Receipt, Email, Defaults, etc.
    public string DataType { get; set; } = "string"; // string, int, bool, decimal, json
    public bool IsEncrypted { get; set; } = false;
    public bool IsPublic { get; set; } = false; // Can be accessed by non-admin users
}

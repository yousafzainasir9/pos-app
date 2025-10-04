namespace POS.Application.DTOs;

public class UserDto
{
    public long Id { get; set; }
    public required string Username { get; set; }
    public required string Email { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string FullName { get; set; }
    public string? Phone { get; set; }
    public required string Role { get; set; }
    public bool IsActive { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public long? StoreId { get; set; }
    public string? StoreName { get; set; }
}

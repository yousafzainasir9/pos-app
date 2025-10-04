namespace POS.Application.DTOs;

public class UserDto
{
    public long Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public long? StoreId { get; set; }
    public string? StoreName { get; set; }
}

public class UserDetailDto
{
    public long Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public long? StoreId { get; set; }
    public string? StoreName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool HasPin { get; set; }
}

public class CreateUserDto
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Pin { get; set; }
    public string Role { get; set; } = string.Empty;
    public long? StoreId { get; set; }
}

public class UpdateUserDto
{
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool? IsActive { get; set; }
    public long? StoreId { get; set; }
}

public class ResetPasswordDto
{
    public string NewPassword { get; set; } = string.Empty;
}

public class ResetPinDto
{
    public string NewPin { get; set; } = string.Empty;
}

public class UserActivityDto
{
    public DateTime Timestamp { get; set; }
    public string Action { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
}

namespace POS.Application.DTOs.Auth;

public class LoginRequestDto
{
    public required string Username { get; set; }
    public required string Password { get; set; }
}

public class PinLoginRequestDto
{
    public required string Pin { get; set; }
    public long StoreId { get; set; }
}

public class RefreshTokenRequestDto
{
    public required string RefreshToken { get; set; }
}

public class LoginResponseDto
{
    public required string Token { get; set; }
    public required string RefreshToken { get; set; }
    public int ExpiresIn { get; set; }
    public required AuthUserDto User { get; set; }
}

public class AuthUserDto
{
    public long Id { get; set; }
    public required string Username { get; set; }
    public required string Email { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Role { get; set; }
    public long? StoreId { get; set; }
    public string? StoreName { get; set; }
    public bool HasActiveShift { get; set; }
    public long? ActiveShiftId { get; set; }
}

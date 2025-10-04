namespace POS.Application.DTOs;

public class StoreDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public decimal TaxRate { get; set; }
    public string Currency { get; set; } = "AUD";
    public bool IsActive { get; set; }
}

public class StoreDetailDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? TaxNumber { get; set; }
    public decimal TaxRate { get; set; }
    public string Currency { get; set; } = "AUD";
    public TimeOnly? OpeningTime { get; set; }
    public TimeOnly? ClosingTime { get; set; }
    public bool IsActive { get; set; }
    public int ActiveUserCount { get; set; }
}

public class UpdateStoreDto
{
    public string? Name { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? TaxNumber { get; set; }
    public decimal? TaxRate { get; set; }
    public string? Currency { get; set; }
    public TimeOnly? OpeningTime { get; set; }
    public TimeOnly? ClosingTime { get; set; }
}

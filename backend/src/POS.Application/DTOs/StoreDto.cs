namespace POS.Application.DTOs;

public class StoreDto
{
    public long Id { get; set; }
    public required string Name { get; set; }
    public required string Code { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public decimal TaxRate { get; set; }
    public required string Currency { get; set; }
    public bool IsActive { get; set; }
}

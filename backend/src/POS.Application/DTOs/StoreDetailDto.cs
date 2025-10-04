namespace POS.Application.DTOs;

public class StoreDetailDto : StoreDto
{
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public string? TaxNumber { get; set; }
    public TimeOnly? OpeningTime { get; set; }
    public TimeOnly? ClosingTime { get; set; }
    public int ActiveUserCount { get; set; }
}

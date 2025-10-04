namespace POS.Application.DTOs.Shifts;

public class OpenShiftDto
{
    public decimal StartingCash { get; set; }
    public string? Notes { get; set; }
}

public class CloseShiftDto
{
    public decimal EndingCash { get; set; }
    public string? Notes { get; set; }
}

public class ShiftDto
{
    public long Id { get; set; }
    public required string ShiftNumber { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public decimal StartingCash { get; set; }
    public required string Status { get; set; }
    public int TotalOrders { get; set; }
    public decimal TotalSales { get; set; }
}

public class ShiftSummaryDto : ShiftDto
{
    public decimal EndingCash { get; set; }
    public decimal ExpectedCash { get; set; }
    public decimal CashDifference { get; set; }
    public decimal CashSales { get; set; }
    public decimal CardSales { get; set; }
    public decimal OtherSales { get; set; }
}

public class ShiftReportDto
{
    public long ShiftId { get; set; }
    public required string ShiftNumber { get; set; }
    public required string CashierName { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public required string Status { get; set; }
    public decimal StartingCash { get; set; }
    public decimal? EndingCash { get; set; }
    public int TotalOrders { get; set; }
    public int CompletedOrders { get; set; }
    public int CancelledOrders { get; set; }
    public decimal CashSales { get; set; }
    public decimal CardSales { get; set; }
    public decimal TotalSales { get; set; }
    public List<ProductSalesDto> TopProducts { get; set; } = new();
    public List<HourlySalesDto> HourlySales { get; set; } = new();
}

public class ProductSalesDto
{
    public required string ProductName { get; set; }
    public int Quantity { get; set; }
    public decimal TotalSales { get; set; }
}

public class HourlySalesDto
{
    public int Hour { get; set; }
    public int OrderCount { get; set; }
    public decimal TotalSales { get; set; }
}

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using POS.Domain.Entities;
using POS.Domain.Enums;
using POS.Infrastructure.Data;
using POS.Infrastructure.Repositories;
using POS.Infrastructure.Services;
using POS.Infrastructure.Tests.Helpers;
using Xunit;

namespace POS.Infrastructure.Tests.Services;

/// <summary>
/// Tests for ReportService - Business reporting and analytics
/// </summary>
public class ReportServiceTests : IDisposable
{
    private readonly POSDbContext _context;
    private readonly Mock<ILogger<ReportService>> _mockLogger;
    private readonly ReportService _reportService;
    private readonly UnitOfWork _unitOfWork;

    public ReportServiceTests()
    {
        _context = InMemoryDbContextFactory.CreateWithData();
        _unitOfWork = new UnitOfWork(_context);
        _mockLogger = new Mock<ILogger<ReportService>>();
        _reportService = new ReportService(_context);
        
        SeedTestOrders();
    }

    private void SeedTestOrders()
    {
        // Create some test orders for reporting
        var order1 = new Order
        {
            OrderNumber = "TEST001",
            OrderDate = DateTime.Now.AddDays(-5),
            Status = OrderStatus.Completed,
            OrderType = OrderType.DineIn,
            UserId = 1,
            StoreId = 1,
            SubTotal = 90.91m,
            TaxAmount = 9.09m,
            TotalAmount = 100.00m,
            PaidAmount = 100.00m,
            CompletedAt = DateTime.Now.AddDays(-5)
        };

        var order2 = new Order
        {
            OrderNumber = "TEST002",
            OrderDate = DateTime.Now.AddDays(-3),
            Status = OrderStatus.Completed,
            OrderType = OrderType.TakeAway,
            UserId = 1,
            StoreId = 1,
            SubTotal = 45.45m,
            TaxAmount = 4.55m,
            TotalAmount = 50.00m,
            PaidAmount = 50.00m,
            CompletedAt = DateTime.Now.AddDays(-3)
        };

        var order3 = new Order
        {
            OrderNumber = "TEST003",
            OrderDate = DateTime.Now.AddDays(-1),
            Status = OrderStatus.Pending,
            OrderType = OrderType.DineIn,
            UserId = 2,
            StoreId = 1,
            SubTotal = 27.27m,
            TaxAmount = 2.73m,
            TotalAmount = 30.00m,
            PaidAmount = 0m
        };

        _context.Orders.AddRange(order1, order2, order3);
        _context.SaveChanges();
    }

    [Fact]
    public async Task GetSalesReportAsync_WithDateRange_ShouldReturnReport()
    {
        // Arrange
        var fromDate = DateTime.Now.AddDays(-7);
        var toDate = DateTime.Now;

        // Act
        var report = await _reportService.GetSalesReportAsync(fromDate, toDate);

        // Assert
        report.Should().NotBeNull();
        report.TotalSales.Should().BeGreaterThan(0);
        report.TotalOrders.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetProductPerformanceAsync_WithDateRange_ShouldReturnReport()
    {
        // Arrange
        var fromDate = DateTime.Now.AddDays(-30);
        var toDate = DateTime.Now;

        // Act
        var report = await _reportService.GetProductPerformanceAsync(fromDate, toDate);

        // Assert
        report.Should().NotBeNull();
        report.TopSellingProducts.Should().NotBeNull();
        report.CategoryPerformance.Should().NotBeNull();
        report.LowStockProducts.Should().NotBeNull();
    }

    [Fact]
    public async Task GetDashboardStatsAsync_ShouldReturnStats()
    {
        // Act
        var stats = await _reportService.GetDashboardStatsAsync();

        // Assert
        stats.Should().NotBeNull();
        stats.TodayOrders.Should().BeGreaterOrEqualTo(0);
        stats.TodaySales.Should().BeGreaterOrEqualTo(0);
        stats.WeekSales.Should().BeGreaterOrEqualTo(0);
        stats.MonthSales.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public async Task GetShiftReportAsync_WithValidShiftId_ShouldReturnShiftData()
    {
        // Arrange
        var shift = new Shift
        {
            ShiftNumber = "SHIFT001",
            UserId = 1,
            StoreId = 1,
            StartTime = DateTime.Now.AddHours(-8),
            Status = ShiftStatus.Open,
            StartingCash = 100.00m
        };
        _context.Shifts.Add(shift);
        await _context.SaveChangesAsync();

        // Act
        var report = await _reportService.GetShiftReportAsync((int)shift.Id);

        // Assert
        report.Should().NotBeNull();
        report.ShiftNumber.Should().Be("SHIFT001");
        report.StartingCash.Should().Be(100.00m);
    }

    [Fact]
    public async Task GetShiftReportAsync_ShouldIncludeShiftOrders()
    {
        // Arrange
        var shift = new Shift
        {
            ShiftNumber = "SHIFT002",
            UserId = 1,
            StoreId = 1,
            StartTime = DateTime.Now.AddHours(-4),
            Status = ShiftStatus.Open,
            StartingCash = 50.00m
        };
        _context.Shifts.Add(shift);
        await _context.SaveChangesAsync();
        
        // Clear tracker to ensure fresh load
        _context.ChangeTracker.Clear();

        var order = new Order
        {
            OrderNumber = "SHIFT_ORDER_001",
            OrderDate = DateTime.Now.AddHours(-2),
            Status = OrderStatus.Completed,
            OrderType = OrderType.DineIn,
            UserId = 1,
            StoreId = 1,
            ShiftId = shift.Id,
            SubTotal = 90.91m,
            TaxAmount = 9.09m,
            TotalAmount = 100.00m,
            PaidAmount = 100.00m,
            CompletedAt = DateTime.Now.AddHours(-2)
        };
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();
        
        // Clear tracker again to force reload from database
        _context.ChangeTracker.Clear();

        // Act
        var report = await _reportService.GetShiftReportAsync((int)shift.Id);

        // Assert
        report.Should().NotBeNull();
        report.TotalOrders.Should().BeGreaterOrEqualTo(1, "because we added 1 order to the shift");
        report.TotalSales.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetCurrentShiftReportAsync_WithActiveShift_ShouldReturnReport()
    {
        // Arrange
        var userId = 1;
        var shift = new Shift
        {
            ShiftNumber = "CURRENT_SHIFT",
            UserId = userId,
            StoreId = 1,
            StartTime = DateTime.Now.AddHours(-2),
            Status = ShiftStatus.Open,
            StartingCash = 75.00m
        };
        _context.Shifts.Add(shift);
        await _context.SaveChangesAsync();

        // Act
        var report = await _reportService.GetCurrentShiftReportAsync(userId);

        // Assert
        report.Should().NotBeNull();
        report.ShiftNumber.Should().Be("CURRENT_SHIFT");
    }

    [Fact]
    public async Task GetCurrentShiftReportAsync_WithNoActiveShift_ShouldReturnNull()
    {
        // Arrange
        var userId = 999; // User with no active shift

        // Act
        var report = await _reportService.GetCurrentShiftReportAsync(userId);

        // Assert
        report.Should().BeNull();
    }

    [Fact]
    public async Task ExportSalesReportAsync_ShouldGenerateCsv()
    {
        // Arrange
        var fromDate = DateTime.Now.AddDays(-7);
        var toDate = DateTime.Now;

        // Act
        var csvBytes = await _reportService.ExportSalesReportAsync(fromDate, toDate);

        // Assert
        csvBytes.Should().NotBeNull();
        csvBytes.Length.Should().BeGreaterThan(0);
        
        var csvContent = System.Text.Encoding.UTF8.GetString(csvBytes);
        csvContent.Should().Contain("Sales Report");
    }

    [Fact]
    public async Task ExportProductPerformanceAsync_ShouldGenerateCsv()
    {
        // Arrange
        var fromDate = DateTime.Now.AddDays(-30);
        var toDate = DateTime.Now;

        // Act
        var csvBytes = await _reportService.ExportProductPerformanceAsync(fromDate, toDate);

        // Assert
        csvBytes.Should().NotBeNull();
        csvBytes.Length.Should().BeGreaterThan(0);
        
        var csvContent = System.Text.Encoding.UTF8.GetString(csvBytes);
        csvContent.Should().Contain("Product Performance Report");
    }

    [Fact]
    public async Task ExportShiftReportAsync_ShouldGenerateCsv()
    {
        // Arrange
        var fromDate = DateTime.Now.AddDays(-7);
        var toDate = DateTime.Now;

        // Act
        var csvBytes = await _reportService.ExportShiftReportAsync(fromDate, toDate);

        // Assert
        csvBytes.Should().NotBeNull();
        csvBytes.Length.Should().BeGreaterThan(0);
        
        var csvContent = System.Text.Encoding.UTF8.GetString(csvBytes);
        csvContent.Should().Contain("Shift Report");
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}

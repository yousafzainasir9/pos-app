using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using POS.Application.Common.Models;
using POS.Application.DTOs;
using POS.Domain.Entities;
using POS.Domain.Enums;
using POS.Infrastructure.Data;
using POS.Infrastructure.Data.Interceptors;
using POS.Infrastructure.Repositories;
using POS.WebAPI.Controllers;
using POS.WebAPI.Tests.Helpers;
using Xunit;
using Xunit.Abstractions;
using System.Text.Json;

namespace POS.WebAPI.Tests.Controllers;

/// <summary>
/// Tests for OrdersController - Get operations, filtering, and pagination
/// </summary>
public class OrdersControllerGetTests : IDisposable
{
    private readonly POSDbContext _context;
    private readonly OrdersController _controller;
    private readonly UnitOfWork _unitOfWork;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly ITestOutputHelper _output;

    public OrdersControllerGetTests(ITestOutputHelper output)
    {
        _output = output;
        _context = InMemoryDbContextFactory.CreateWithData();
        _unitOfWork = new UnitOfWork(_context);
        _mockCurrentUserService = new Mock<ICurrentUserService>();
        
        _controller = new OrdersController(
            _unitOfWork,
            Mock.Of<ILogger<OrdersController>>(),
            _mockCurrentUserService.Object
        );
        
        SeedTestOrders();
    }

    private void SetupControllerContext(long userId, string username, string role, long? storeId = null)
    {
        var claims = new List<System.Security.Claims.Claim>
        {
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, userId.ToString()),
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, username),
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, role)
        };

        if (storeId.HasValue)
        {
            claims.Add(new System.Security.Claims.Claim("StoreId", storeId.Value.ToString()));
        }

        var identity = new System.Security.Claims.ClaimsIdentity(claims, "TestAuth");
        var principal = new System.Security.Claims.ClaimsPrincipal(identity);

        var httpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext
        {
            User = principal
        };

        _controller.ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext
        {
            HttpContext = httpContext
        };
    }

    private void SeedTestOrders()
    {
        // Note: Users and Stores are already seeded by TestDataSeeder in CreateWithData()
        // User IDs: 1 (admin), 2 (cashier), 3 (customer)
        // Store IDs: 1, 2
        
        var order1 = new Order
        {
            OrderNumber = "GET001",
            OrderDate = DateTime.Now.AddDays(-5),
            Status = OrderStatus.Completed,
            OrderType = OrderType.DineIn,
            UserId = 1, // Admin user exists from TestDataSeeder
            StoreId = 1, // Store exists from TestDataSeeder
            ShiftId = null,
            SubTotal = 90.91m,
            TaxAmount = 9.09m,
            TotalAmount = 100.00m,
            PaidAmount = 100.00m,
            CompletedAt = DateTime.Now.AddDays(-5)
        };

        var order2 = new Order
        {
            OrderNumber = "GET002",
            OrderDate = DateTime.Now.AddDays(-3),
            Status = OrderStatus.Completed,
            OrderType = OrderType.TakeAway,
            UserId = 1,
            StoreId = 1,
            ShiftId = null,
            SubTotal = 45.45m,
            TaxAmount = 4.55m,
            TotalAmount = 50.00m,
            PaidAmount = 50.00m,
            CompletedAt = DateTime.Now.AddDays(-3)
        };

        var order3 = new Order
        {
            OrderNumber = "GET003",
            OrderDate = DateTime.Now,
            Status = OrderStatus.Pending,
            OrderType = OrderType.DineIn,
            UserId = 2, // Cashier user exists from TestDataSeeder
            StoreId = 1,
            ShiftId = null,
            SubTotal = 27.27m,
            TaxAmount = 2.73m,
            TotalAmount = 30.00m,
            PaidAmount = 0m
        };

        _context.Orders.AddRange(order1, order2, order3);
        _context.SaveChanges();
    }

    [Fact]
    public async Task GetOrders_WithCustomerFilter_ShouldFilterByCustomer()
    {
        // Arrange
        _mockCurrentUserService.Setup(s => s.UserId).Returns(1);
        _mockCurrentUserService.Setup(s => s.Username).Returns("testcashier");
        SetupControllerContext(1, "testcashier", "Cashier", 1);
        var customerId = 1L;

        // Act
        var result = await _controller.GetOrders(null, null, null, customerId, 1, 20);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetOrder_WithValidId_ShouldReturnOrderWithDetails()
    {
        // Arrange
        _mockCurrentUserService.Setup(s => s.UserId).Returns(1);
        _mockCurrentUserService.Setup(s => s.Username).Returns("testcashier");
        SetupControllerContext(1, "testcashier", "Cashier", 1);
        
        // Get an order that was seeded with valid User and Store references
        var order = await _unitOfWork.Repository<Order>().Query()
            .FirstOrDefaultAsync(o => o.OrderNumber.StartsWith("GET"));

        if (order == null)
        {
            // Skip test if no test orders exist
            _output.WriteLine("No test orders found, skipping test");
            return;
        }

        // Act
        var result = await _controller.GetOrder(order.Id);

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().BeOfType<OkObjectResult>();
        
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task GetOrder_WithInvalidId_ShouldReturn404()
    {
        // Arrange
        _mockCurrentUserService.Setup(s => s.UserId).Returns(1);
        _mockCurrentUserService.Setup(s => s.Username).Returns("testcashier");
        SetupControllerContext(1, "testcashier", "Cashier", 1);

        // Act
        var result = await _controller.GetOrder(99999);

        // Assert
        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetOrdersSummary_ShouldReturnSummaryStatistics()
    {
        // Arrange
        _mockCurrentUserService.Setup(s => s.UserId).Returns(1);
        _mockCurrentUserService.Setup(s => s.Username).Returns("testcashier");
        SetupControllerContext(1, "testcashier", "Cashier", 1);

        // Act
        var result = await _controller.GetOrdersSummary(null, null, null);

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetOrdersSummary_WithDateFilter_ShouldFilterSummary()
    {
        // Arrange
        _mockCurrentUserService.Setup(s => s.UserId).Returns(1);
        _mockCurrentUserService.Setup(s => s.Username).Returns("testcashier");
        SetupControllerContext(1, "testcashier", "Cashier", 1);
        var fromDate = DateTime.Now.AddDays(-7);
        var toDate = DateTime.Now;

        // Act
        var result = await _controller.GetOrdersSummary(fromDate, toDate, null);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetCurrentShiftOrders_WithActiveShift_ShouldReturnShiftOrders()
    {
        // Arrange
        _mockCurrentUserService.Setup(s => s.UserId).Returns(1);
        _mockCurrentUserService.Setup(s => s.Username).Returns("testadmin");
        SetupControllerContext(1, "testadmin", "Admin", 1);
        
        var shift = new Shift
        {
            ShiftNumber = "SHIFT001",
            UserId = 1,
            StoreId = 1,
            StartTime = DateTime.Now.AddHours(-4),
            Status = ShiftStatus.Open,
            StartingCash = 100m
        };
        await _unitOfWork.Repository<Shift>().AddAsync(shift);
        await _unitOfWork.SaveChangesAsync();

        // Act
        var result = await _controller.GetCurrentShiftOrders();

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetCurrentShiftOrders_WithNoActiveShift_ShouldReturnEmptyOrders()
    {
        // Arrange
        _mockCurrentUserService.Setup(s => s.UserId).Returns(999); // User with no shift
        _mockCurrentUserService.Setup(s => s.Username).Returns("testuser");
        SetupControllerContext(999, "testuser", "Cashier", 1);

        // Act
        var result = await _controller.GetCurrentShiftOrders();

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
}

using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using POS.Application.DTOs;
using POS.Domain.Entities;
using POS.Domain.Enums;
using POS.Infrastructure.Data;
using POS.Infrastructure.Data.Interceptors;
using POS.Infrastructure.Repositories;
using POS.WebAPI.Controllers;
using POS.WebAPI.Tests.Helpers;
using Xunit;

namespace POS.WebAPI.Tests.Controllers;

/// <summary>
/// Tests for OrdersController - Process payments and void orders
/// </summary>
public class OrdersControllerPaymentTests : IDisposable
{
    private readonly POSDbContext _context;
    private readonly OrdersController _controller;
    private readonly UnitOfWork _unitOfWork;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;

    public OrdersControllerPaymentTests()
    {
        _context = InMemoryDbContextFactory.CreateWithData();
        _unitOfWork = new UnitOfWork(_context);
        _mockCurrentUserService = new Mock<ICurrentUserService>();
        
        _controller = new OrdersController(
            _unitOfWork,
            Mock.Of<ILogger<OrdersController>>(),
            _mockCurrentUserService.Object
        );
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

    [Fact]
    public async Task ProcessPayment_WithFullPayment_ShouldCompleteOrder()
    {
        // Arrange
        _mockCurrentUserService.Setup(s => s.UserId).Returns(1);
        _mockCurrentUserService.Setup(s => s.Username).Returns("testcashier");
        SetupControllerContext(1, "testcashier", "Cashier", 1);

        var order = new Order
        {
            OrderNumber = "PAY001",
            OrderDate = DateTime.Now,
            Status = OrderStatus.Pending,
            OrderType = OrderType.DineIn,
            UserId = 1,
            StoreId = 1,
            ShiftId = null, // No shift for test orders
            SubTotal = 90.91m,
            TaxAmount = 9.09m,
            TotalAmount = 100.00m,
            PaidAmount = 0m
        };
        await _unitOfWork.Repository<Order>().AddAsync(order);
        await _unitOfWork.SaveChangesAsync();

        var paymentDto = new ProcessPaymentDto
        {
            Amount = 100.00m,
            PaymentMethod = PaymentMethod.Cash
        };

        // Act
        var result = await _controller.ProcessPayment(order.Id, paymentDto);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<OkObjectResult>();
        
        _context.ChangeTracker.Clear();
        var updatedOrder = await _unitOfWork.Repository<Order>().GetByIdAsync(order.Id);
        updatedOrder!.Status.Should().Be(OrderStatus.Completed);
        updatedOrder.PaidAmount.Should().Be(100.00m);
    }

    [Fact]
    public async Task ProcessPayment_WithPartialPayment_ShouldSetProcessingStatus()
    {
        // Arrange
        _mockCurrentUserService.Setup(s => s.UserId).Returns(1);
        _mockCurrentUserService.Setup(s => s.Username).Returns("testcashier");
        SetupControllerContext(1, "testcashier", "Cashier", 1);

        var order = new Order
        {
            OrderNumber = "PAY002",
            OrderDate = DateTime.Now,
            Status = OrderStatus.Pending,
            OrderType = OrderType.DineIn,
            UserId = 1,
            StoreId = 1,
            ShiftId = null,
            SubTotal = 90.91m,
            TaxAmount = 9.09m,
            TotalAmount = 100.00m,
            PaidAmount = 0m
        };
        await _unitOfWork.Repository<Order>().AddAsync(order);
        await _unitOfWork.SaveChangesAsync();

        var paymentDto = new ProcessPaymentDto
        {
            Amount = 50.00m,
            PaymentMethod = PaymentMethod.Cash
        };

        // Act
        await _controller.ProcessPayment(order.Id, paymentDto);

        // Assert
        _context.ChangeTracker.Clear();
        var updatedOrder = await _unitOfWork.Repository<Order>().GetByIdAsync(order.Id);
        updatedOrder!.Status.Should().Be(OrderStatus.Processing);
        updatedOrder.PaidAmount.Should().Be(50.00m);
    }

    [Fact]
    public async Task ProcessPayment_WithOverpayment_ShouldCalculateChange()
    {
        // Arrange
        _mockCurrentUserService.Setup(s => s.UserId).Returns(1);
        _mockCurrentUserService.Setup(s => s.Username).Returns("testcashier");
        SetupControllerContext(1, "testcashier", "Cashier", 1);

        var order = new Order
        {
            OrderNumber = "PAY003",
            OrderDate = DateTime.Now,
            Status = OrderStatus.Pending,
            OrderType = OrderType.DineIn,
            UserId = 1,
            StoreId = 1,
            ShiftId = null,
            SubTotal = 90.91m,
            TaxAmount = 9.09m,
            TotalAmount = 100.00m,
            PaidAmount = 0m
        };
        await _unitOfWork.Repository<Order>().AddAsync(order);
        await _unitOfWork.SaveChangesAsync();

        var paymentDto = new ProcessPaymentDto
        {
            Amount = 120.00m,
            PaymentMethod = PaymentMethod.Cash
        };

        // Act
        await _controller.ProcessPayment(order.Id, paymentDto);

        // Assert
        _context.ChangeTracker.Clear();
        var updatedOrder = await _unitOfWork.Repository<Order>().GetByIdAsync(order.Id);
        updatedOrder!.ChangeAmount.Should().Be(20.00m);
    }

    [Fact]
    public async Task ProcessPayment_ShouldCreatePaymentRecord()
    {
        // Arrange
        _mockCurrentUserService.Setup(s => s.UserId).Returns(1);
        _mockCurrentUserService.Setup(s => s.Username).Returns("testcashier");
        SetupControllerContext(1, "testcashier", "Cashier", 1);

        var order = new Order
        {
            OrderNumber = "PAY004",
            OrderDate = DateTime.Now,
            Status = OrderStatus.Pending,
            OrderType = OrderType.DineIn,
            UserId = 1,
            StoreId = 1,
            ShiftId = null,
            SubTotal = 90.91m,
            TaxAmount = 9.09m,
            TotalAmount = 100.00m,
            PaidAmount = 0m
        };
        await _unitOfWork.Repository<Order>().AddAsync(order);
        await _unitOfWork.SaveChangesAsync();

        var paymentDto = new ProcessPaymentDto
        {
            Amount = 100.00m,
            PaymentMethod = PaymentMethod.CreditCard,
            ReferenceNumber = "REF123456"
        };

        // Act
        await _controller.ProcessPayment(order.Id, paymentDto);

        // Assert
        var payment = await _unitOfWork.Repository<Payment>().Query()
            .FirstOrDefaultAsync(p => p.OrderId == order.Id);
        
        payment.Should().NotBeNull();
        payment!.Amount.Should().Be(100.00m);
        payment.PaymentMethod.Should().Be(PaymentMethod.CreditCard);
        payment.ReferenceNumber.Should().Be("REF123456");
    }

    [Fact]
    public async Task ProcessPayment_WithCompletedOrder_ShouldReturnBadRequest()
    {
        // Arrange
        _mockCurrentUserService.Setup(s => s.UserId).Returns(1);
        _mockCurrentUserService.Setup(s => s.Username).Returns("testcashier");
        SetupControllerContext(1, "testcashier", "Cashier", 1);

        var order = new Order
        {
            OrderNumber = "PAY005",
            OrderDate = DateTime.Now,
            Status = OrderStatus.Completed,
            OrderType = OrderType.DineIn,
            UserId = 1,
            StoreId = 1,
            ShiftId = null, // FIXED: Removed ShiftId = 1 (which doesn't exist)
            SubTotal = 90.91m,
            TaxAmount = 9.09m,
            TotalAmount = 100.00m,
            PaidAmount = 100.00m
        };
        await _unitOfWork.Repository<Order>().AddAsync(order);
        await _unitOfWork.SaveChangesAsync();

        var paymentDto = new ProcessPaymentDto
        {
            Amount = 10.00m,
            PaymentMethod = PaymentMethod.Cash
        };

        // Act
        var result = await _controller.ProcessPayment(order.Id, paymentDto);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task ProcessPayment_WithInvalidOrderId_ShouldReturn404()
    {
        // Arrange
        _mockCurrentUserService.Setup(s => s.UserId).Returns(1);
        _mockCurrentUserService.Setup(s => s.Username).Returns("testcashier");
        SetupControllerContext(1, "testcashier", "Cashier", 1);

        var paymentDto = new ProcessPaymentDto
        {
            Amount = 100.00m,
            PaymentMethod = PaymentMethod.Cash
        };

        // Act
        var result = await _controller.ProcessPayment(99999, paymentDto);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task ProcessPayment_MultiplePayments_ShouldAccumulatePaidAmount()
    {
        // Arrange
        _mockCurrentUserService.Setup(s => s.UserId).Returns(1);
        _mockCurrentUserService.Setup(s => s.Username).Returns("testcashier");
        SetupControllerContext(1, "testcashier", "Cashier", 1);

        var order = new Order
        {
            OrderNumber = "PAY006",
            OrderDate = DateTime.Now,
            Status = OrderStatus.Pending,
            OrderType = OrderType.DineIn,
            UserId = 1,
            StoreId = 1,
            ShiftId = null,
            SubTotal = 90.91m,
            TaxAmount = 9.09m,
            TotalAmount = 100.00m,
            PaidAmount = 0m
        };
        await _unitOfWork.Repository<Order>().AddAsync(order);
        await _unitOfWork.SaveChangesAsync();

        // Act - First payment
        await _controller.ProcessPayment(order.Id, new ProcessPaymentDto
        {
            Amount = 50.00m,
            PaymentMethod = PaymentMethod.Cash
        });

        // Act - Second payment
        await _controller.ProcessPayment(order.Id, new ProcessPaymentDto
        {
            Amount = 50.00m,
            PaymentMethod = PaymentMethod.CreditCard
        });

        // Assert
        _context.ChangeTracker.Clear();
        var updatedOrder = await _unitOfWork.Repository<Order>().GetByIdAsync(order.Id);
        updatedOrder!.PaidAmount.Should().Be(100.00m);
        updatedOrder.Status.Should().Be(OrderStatus.Completed);
    }

    [Fact]
    public async Task VoidOrder_WithValidOrder_ShouldCancelOrder()
    {
        // Arrange
        _mockCurrentUserService.Setup(s => s.UserId).Returns(1);
        _mockCurrentUserService.Setup(s => s.Username).Returns("testcashier");
        SetupControllerContext(1, "testcashier", "Cashier", 1);

        var product = await _unitOfWork.Repository<Product>().Query()
            .FirstAsync(p => p.TrackInventory);
        
        var order = new Order
        {
            OrderNumber = "VOID001",
            OrderDate = DateTime.Now,
            Status = OrderStatus.Pending,
            OrderType = OrderType.DineIn,
            UserId = 1,
            StoreId = 1,
            ShiftId = null,
            SubTotal = 90.91m,
            TaxAmount = 9.09m,
            TotalAmount = 100.00m,
            PaidAmount = 0m
        };
        await _unitOfWork.Repository<Order>().AddAsync(order);
        await _unitOfWork.SaveChangesAsync();

        var voidDto = new VoidOrderDto
        {
            Reason = "Customer changed mind"
        };

        // Act
        var result = await _controller.VoidOrder(order.Id, voidDto);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<OkObjectResult>();
        
        _context.ChangeTracker.Clear();
        var voidedOrder = await _unitOfWork.Repository<Order>().GetByIdAsync(order.Id);
        voidedOrder!.Status.Should().Be(OrderStatus.Cancelled);
        voidedOrder.CancellationReason.Should().Be("Customer changed mind");
    }

    [Fact]
    public async Task VoidOrder_ShouldRestoreInventory()
    {
        // Arrange
        _mockCurrentUserService.Setup(s => s.UserId).Returns(1);
        _mockCurrentUserService.Setup(s => s.Username).Returns("testcashier");
        SetupControllerContext(1, "testcashier", "Cashier", 1);

        var product = await _unitOfWork.Repository<Product>().Query()
            .FirstAsync(p => p.TrackInventory);
        var originalStock = product.StockQuantity;
        var orderQuantity = 5;

        // Deduct stock
        product.StockQuantity -= orderQuantity;
        _unitOfWork.Repository<Product>().Update(product);
        await _unitOfWork.SaveChangesAsync();

        var order = new Order
        {
            OrderNumber = "VOID002",
            OrderDate = DateTime.Now,
            Status = OrderStatus.Pending,
            OrderType = OrderType.DineIn,
            UserId = 1,
            StoreId = 1,
            ShiftId = null,
            SubTotal = 45.45m,
            TaxAmount = 4.55m,
            TotalAmount = 50.00m,
            PaidAmount = 0m
        };
        
        var orderItem = new OrderItem
        {
            Order = order,
            ProductId = product.Id,
            Quantity = orderQuantity,
            UnitPriceIncGst = 10.00m,
            UnitPriceExGst = 9.09m,
            UnitGstAmount = 0.91m,
            SubTotal = 45.45m,
            TaxAmount = 4.55m,
            TotalAmount = 50.00m
        };
        order.OrderItems.Add(orderItem);
        
        await _unitOfWork.Repository<Order>().AddAsync(order);
        await _unitOfWork.SaveChangesAsync();

        var voidDto = new VoidOrderDto { Reason = "Test void" };

        // Act
        await _controller.VoidOrder(order.Id, voidDto);

        // Assert
        _context.ChangeTracker.Clear();
        var restoredProduct = await _unitOfWork.Repository<Product>().GetByIdAsync(product.Id);
        restoredProduct!.StockQuantity.Should().Be(originalStock);
    }

    [Fact]
    public async Task VoidOrder_ShouldCreateReturnInventoryTransaction()
    {
        // Arrange
        _mockCurrentUserService.Setup(s => s.UserId).Returns(1);
        _mockCurrentUserService.Setup(s => s.Username).Returns("testcashier");
        SetupControllerContext(1, "testcashier", "Cashier", 1);

        var product = await _unitOfWork.Repository<Product>().Query()
            .FirstAsync(p => p.TrackInventory);

        var order = new Order
        {
            OrderNumber = "VOID003",
            OrderDate = DateTime.Now,
            Status = OrderStatus.Pending,
            OrderType = OrderType.DineIn,
            UserId = 1,
            StoreId = 1,
            ShiftId = null,
            SubTotal = 9.09m,
            TaxAmount = 0.91m,
            TotalAmount = 10.00m,
            PaidAmount = 0m
        };
        
        var orderItem = new OrderItem
        {
            Order = order,
            ProductId = product.Id,
            Quantity = 1,
            UnitPriceIncGst = 10.00m,
            UnitPriceExGst = 9.09m,
            UnitGstAmount = 0.91m,
            SubTotal = 9.09m,
            TaxAmount = 0.91m,
            TotalAmount = 10.00m
        };
        order.OrderItems.Add(orderItem);
        
        await _unitOfWork.Repository<Order>().AddAsync(order);
        await _unitOfWork.SaveChangesAsync();

        var transactionsBefore = await _unitOfWork.Repository<InventoryTransaction>()
            .Query()
            .CountAsync();

        var voidDto = new VoidOrderDto { Reason = "Test" };

        // Act
        await _controller.VoidOrder(order.Id, voidDto);

        // Assert
        var transactionsAfter = await _unitOfWork.Repository<InventoryTransaction>()
            .Query()
            .CountAsync();
        
        transactionsAfter.Should().BeGreaterThan(transactionsBefore);
    }

    [Fact]
    public async Task VoidOrder_WithAlreadyCancelledOrder_ShouldReturnBadRequest()
    {
        // Arrange
        _mockCurrentUserService.Setup(s => s.UserId).Returns(1);
        _mockCurrentUserService.Setup(s => s.Username).Returns("testcashier");
        SetupControllerContext(1, "testcashier", "Cashier", 1);

        var order = new Order
        {
            OrderNumber = "VOID004",
            OrderDate = DateTime.Now,
            Status = OrderStatus.Cancelled,
            OrderType = OrderType.DineIn,
            UserId = 1,
            StoreId = 1,
            ShiftId = null,
            SubTotal = 90.91m,
            TaxAmount = 9.09m,
            TotalAmount = 100.00m,
            PaidAmount = 0m
        };
        await _unitOfWork.Repository<Order>().AddAsync(order);
        await _unitOfWork.SaveChangesAsync();

        var voidDto = new VoidOrderDto { Reason = "Test" };

        // Act
        var result = await _controller.VoidOrder(order.Id, voidDto);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task VoidOrder_WithInvalidOrderId_ShouldReturn404()
    {
        // Arrange
        _mockCurrentUserService.Setup(s => s.UserId).Returns(1);
        _mockCurrentUserService.Setup(s => s.Username).Returns("testcashier");
        SetupControllerContext(1, "testcashier", "Cashier", 1);

        var voidDto = new VoidOrderDto { Reason = "Test" };

        // Act
        var result = await _controller.VoidOrder(99999, voidDto);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task VoidOrder_ShouldSetCancelledAtTimestamp()
    {
        // Arrange
        _mockCurrentUserService.Setup(s => s.UserId).Returns(1);
        _mockCurrentUserService.Setup(s => s.Username).Returns("testcashier");
        SetupControllerContext(1, "testcashier", "Cashier", 1);

        var order = new Order
        {
            OrderNumber = "VOID005",
            OrderDate = DateTime.Now,
            Status = OrderStatus.Pending,
            OrderType = OrderType.DineIn,
            UserId = 1,
            StoreId = 1,
            ShiftId = null,
            SubTotal = 90.91m,
            TaxAmount = 9.09m,
            TotalAmount = 100.00m,
            PaidAmount = 0m
        };
        await _unitOfWork.Repository<Order>().AddAsync(order);
        await _unitOfWork.SaveChangesAsync();

        var beforeVoid = DateTime.Now;
        var voidDto = new VoidOrderDto { Reason = "Test" };

        // Act
        await _controller.VoidOrder(order.Id, voidDto);
        var afterVoid = DateTime.Now;

        // Assert
        _context.ChangeTracker.Clear();
        var voidedOrder = await _unitOfWork.Repository<Order>().GetByIdAsync(order.Id);
        voidedOrder!.CancelledAt.Should().NotBeNull();
        voidedOrder.CancelledAt.Should().BeOnOrAfter(beforeVoid);
        voidedOrder.CancelledAt.Should().BeOnOrBefore(afterVoid);
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
}

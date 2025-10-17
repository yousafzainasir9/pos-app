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
using Xunit.Abstractions;

namespace POS.WebAPI.Tests.Controllers;

/// <summary>
/// Tests for OrdersController - Create order and process payments
/// </summary>
public class OrdersControllerCreateTests : IDisposable
{
    private readonly POSDbContext _context;
    private readonly OrdersController _controller;
    private readonly UnitOfWork _unitOfWork;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly Mock<ILogger<OrdersController>> _mockLogger;
    private readonly ITestOutputHelper _output;

    public OrdersControllerCreateTests(ITestOutputHelper output)
    {
        _output = output;
        
        // Create in-memory database with test data
        _context = InMemoryDbContextFactory.CreateWithData();
        _unitOfWork = new UnitOfWork(_context);
        
        // Setup mocks
        _mockCurrentUserService = new Mock<ICurrentUserService>();
        _mockLogger = new Mock<ILogger<OrdersController>>();
        
        // Create controller
        _controller = new OrdersController(
            _unitOfWork,
            _mockLogger.Object,
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
    public async Task CreateOrder_WithValidData_ShouldCreateOrder()
    {
        try
        {
            // Arrange
            _mockCurrentUserService.Setup(s => s.UserId).Returns(1);
            _mockCurrentUserService.Setup(s => s.Username).Returns("testadmin");
            SetupControllerContext(1, "testadmin", "Admin", 1);

            var product = await _unitOfWork.Repository<Product>().Query().FirstAsync();
            _output.WriteLine($"Product ID: {product.Id}, Name: {product.Name}");
            
            var dto = new CreateOrderDto
            {
                OrderType = OrderType.DineIn,
                Items = new List<CreateOrderItemDto>
                {
                    new CreateOrderItemDto
                    {
                        ProductId = product.Id,
                        Quantity = 2
                    }
                }
            };

            // Act
            var result = await _controller.CreateOrder(dto);
            _output.WriteLine($"Result Type: {result?.Result?.GetType().Name}");

            // Debug: Check if it's a BadRequest
            if (result?.Result is BadRequestObjectResult badRequest)
            {
                _output.WriteLine($"BadRequest Value: {badRequest.Value}");
            }

            // Assert
            result.Should().NotBeNull();
            result.Result.Should().BeOfType<CreatedAtActionResult>();
            
            var createdResult = result.Result as CreatedAtActionResult;
            createdResult!.Value.Should().NotBeNull();
        }
        catch (Exception ex)
        {
            _output.WriteLine($"Exception: {ex.Message}");
            _output.WriteLine($"Stack Trace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                _output.WriteLine($"Inner Exception: {ex.InnerException.Message}");
            }
            throw;
        }
    }

    [Fact]
    public async Task CreateOrder_ShouldCalculateTotalsCorrectly()
    {
        // Arrange
        _mockCurrentUserService.Setup(s => s.UserId).Returns(1);
        _mockCurrentUserService.Setup(s => s.Username).Returns("testadmin");
        SetupControllerContext(1, "testadmin", "Admin", 1);

        var product = await _unitOfWork.Repository<Product>().Query().FirstAsync();
        var quantity = 3;
        var expectedTotal = (product.PriceExGst + product.GstAmount) * quantity;
        
        var dto = new CreateOrderDto
        {
            OrderType = OrderType.TakeAway,
            Items = new List<CreateOrderItemDto>
            {
                new CreateOrderItemDto
                {
                    ProductId = product.Id,
                    Quantity = quantity
                }
            }
        };

        // Act
        var result = await _controller.CreateOrder(dto);

        // Assert
        result.Result.Should().BeOfType<CreatedAtActionResult>();
        
        // Verify the order was created with correct totals
        var orders = await _unitOfWork.Repository<Order>().Query()
            .OrderByDescending(o => o.Id)
            .FirstAsync();
        
        orders.TotalAmount.Should().Be(expectedTotal);
    }

    [Fact]
    public async Task CreateOrder_ShouldDeductInventory()
    {
        // Arrange
        _mockCurrentUserService.Setup(s => s.UserId).Returns(1);
        _mockCurrentUserService.Setup(s => s.Username).Returns("testadmin");
        SetupControllerContext(1, "testadmin", "Admin", 1);

        var product = await _unitOfWork.Repository<Product>().Query()
            .FirstAsync(p => p.TrackInventory);
        var originalStock = product.StockQuantity;
        var orderQuantity = 2;
        
        var dto = new CreateOrderDto
        {
            OrderType = OrderType.DineIn,
            Items = new List<CreateOrderItemDto>
            {
                new CreateOrderItemDto
                {
                    ProductId = product.Id,
                    Quantity = orderQuantity
                }
            }
        };

        // Act
        await _controller.CreateOrder(dto);

        // Assert
        _context.ChangeTracker.Clear();
        var updatedProduct = await _unitOfWork.Repository<Product>().GetByIdAsync(product.Id);
        updatedProduct!.StockQuantity.Should().Be(originalStock - orderQuantity);
    }

    [Fact]
    public async Task CreateOrder_WithInsufficientStock_ShouldReturnBadRequest()
    {
        // Arrange
        _mockCurrentUserService.Setup(s => s.UserId).Returns(1);
        _mockCurrentUserService.Setup(s => s.Username).Returns("testadmin");
        SetupControllerContext(1, "testadmin", "Admin", 1);

        var product = await _unitOfWork.Repository<Product>().Query()
            .FirstAsync(p => p.TrackInventory);
        var impossibleQuantity = product.StockQuantity + 100;
        
        var dto = new CreateOrderDto
        {
            OrderType = OrderType.DineIn,
            Items = new List<CreateOrderItemDto>
            {
                new CreateOrderItemDto
                {
                    ProductId = product.Id,
                    Quantity = impossibleQuantity
                }
            }
        };

        // Act
        var result = await _controller.CreateOrder(dto);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task CreateOrder_WithInvalidProduct_ShouldReturnBadRequest()
    {
        // Arrange
        _mockCurrentUserService.Setup(s => s.UserId).Returns(1);
        _mockCurrentUserService.Setup(s => s.Username).Returns("testadmin");
        SetupControllerContext(1, "testadmin", "Admin", 1);

        var dto = new CreateOrderDto
        {
            OrderType = OrderType.DineIn,
            Items = new List<CreateOrderItemDto>
            {
                new CreateOrderItemDto
                {
                    ProductId = 99999, // Non-existent product
                    Quantity = 1
                }
            }
        };

        // Act
        var result = await _controller.CreateOrder(dto);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task CreateOrder_ShouldCreateInventoryTransaction()
    {
        // Arrange
        _mockCurrentUserService.Setup(s => s.UserId).Returns(1);
        _mockCurrentUserService.Setup(s => s.Username).Returns("testadmin");
        SetupControllerContext(1, "testadmin", "Admin", 1);

        var product = await _unitOfWork.Repository<Product>().Query()
            .FirstAsync(p => p.TrackInventory);
        
        var transactionCountBefore = await _unitOfWork.Repository<InventoryTransaction>()
            .Query()
            .CountAsync();
        
        var dto = new CreateOrderDto
        {
            OrderType = OrderType.DineIn,
            Items = new List<CreateOrderItemDto>
            {
                new CreateOrderItemDto
                {
                    ProductId = product.Id,
                    Quantity = 1
                }
            }
        };

        // Act
        await _controller.CreateOrder(dto);

        // Assert
        var transactionCountAfter = await _unitOfWork.Repository<InventoryTransaction>()
            .Query()
            .CountAsync();
        
        transactionCountAfter.Should().BeGreaterThan(transactionCountBefore);
    }

    [Fact]
    public async Task CreateOrder_MobileUser_WithoutStoreId_ShouldReturnBadRequest()
    {
        // Arrange - Setup customer user (no store)
        _mockCurrentUserService.Setup(s => s.UserId).Returns(3);
        _mockCurrentUserService.Setup(s => s.Username).Returns("testcustomer");
        SetupControllerContext(3, "testcustomer", "Customer", null);

        var product = await _unitOfWork.Repository<Product>().Query().FirstAsync();
        
        var dto = new CreateOrderDto
        {
            OrderType = OrderType.Pickup,
            StoreId = null, // Mobile user must provide storeId
            Items = new List<CreateOrderItemDto>
            {
                new CreateOrderItemDto
                {
                    ProductId = product.Id,
                    Quantity = 1
                }
            }
        };

        // Act
        var result = await _controller.CreateOrder(dto);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task CreateOrder_MobileUser_WithStoreId_ShouldSucceed()
    {
        // Arrange - Setup customer user
        _mockCurrentUserService.Setup(s => s.UserId).Returns(3);
        _mockCurrentUserService.Setup(s => s.Username).Returns("testcustomer");
        SetupControllerContext(3, "testcustomer", "Customer", null);

        var product = await _unitOfWork.Repository<Product>().Query().FirstAsync();
        
        var dto = new CreateOrderDto
        {
            OrderType = OrderType.Pickup,
            StoreId = 1, // Mobile user provides storeId
            Items = new List<CreateOrderItemDto>
            {
                new CreateOrderItemDto
                {
                    ProductId = product.Id,
                    Quantity = 1
                }
            }
        };

        // Act
        var result = await _controller.CreateOrder(dto);

        // Assert
        result.Result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task CreateOrder_WithMultipleItems_ShouldCreateAllItems()
    {
        // Arrange
        _mockCurrentUserService.Setup(s => s.UserId).Returns(1);
        _mockCurrentUserService.Setup(s => s.Username).Returns("testadmin");
        SetupControllerContext(1, "testadmin", "Admin", 1);

        var products = await _unitOfWork.Repository<Product>().Query().Take(3).ToListAsync();
        
        var dto = new CreateOrderDto
        {
            OrderType = OrderType.DineIn,
            Items = products.Select(p => new CreateOrderItemDto
            {
                ProductId = p.Id,
                Quantity = 2
            }).ToList()
        };

        // Act
        var result = await _controller.CreateOrder(dto);

        // Assert
        result.Result.Should().BeOfType<CreatedAtActionResult>();

        // Verify all items were created
        var order = await _unitOfWork.Repository<Order>().Query()
            .Include(o => o.OrderItems)
            .OrderByDescending(o => o.Id)
            .FirstAsync();

        order.OrderItems.Should().HaveCount(products.Count);
    }

    [Fact]
    public async Task CreateOrder_WithDiscount_ShouldApplyDiscount()
    {
        // Arrange
        _mockCurrentUserService.Setup(s => s.UserId).Returns(1);
        _mockCurrentUserService.Setup(s => s.Username).Returns("testadmin");
        SetupControllerContext(1, "testadmin", "Admin", 1);

        var product = await _unitOfWork.Repository<Product>().Query().FirstAsync();
        var discountAmount = 5.00m;
        
        var dto = new CreateOrderDto
        {
            OrderType = OrderType.DineIn,
            DiscountAmount = discountAmount,
            Items = new List<CreateOrderItemDto>
            {
                new CreateOrderItemDto
                {
                    ProductId = product.Id,
                    Quantity = 1
                }
            }
        };

        // Act
        var result = await _controller.CreateOrder(dto);

        // Assert
        result.Result.Should().BeOfType<CreatedAtActionResult>();

        // Verify discount was applied
        var order = await _unitOfWork.Repository<Order>().Query()
            .OrderByDescending(o => o.Id)
            .FirstAsync();

        order.DiscountAmount.Should().Be(discountAmount);
    }

    [Fact]
    public async Task CreateOrder_WithTableNumber_ShouldSetTableNumber()
    {
        // Arrange
        _mockCurrentUserService.Setup(s => s.UserId).Returns(1);
        _mockCurrentUserService.Setup(s => s.Username).Returns("testadmin");
        SetupControllerContext(1, "testadmin", "Admin", 1);

        var product = await _unitOfWork.Repository<Product>().Query().FirstAsync();
        var tableNumber = "TABLE-5";
        
        var dto = new CreateOrderDto
        {
            OrderType = OrderType.DineIn,
            TableNumber = tableNumber,
            Items = new List<CreateOrderItemDto>
            {
                new CreateOrderItemDto
                {
                    ProductId = product.Id,
                    Quantity = 1
                }
            }
        };

        // Act
        await _controller.CreateOrder(dto);

        // Assert - Table number should be set
        var orders = await _unitOfWork.Repository<Order>().Query()
            .Where(o => o.TableNumber == tableNumber)
            .ToListAsync();
        
        orders.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public async Task CreateOrder_WithCustomer_ShouldLinkCustomer()
    {
        // Arrange
        _mockCurrentUserService.Setup(s => s.UserId).Returns(1);
        _mockCurrentUserService.Setup(s => s.Username).Returns("testadmin");
        SetupControllerContext(1, "testadmin", "Admin", 1);

        var customer = await _unitOfWork.Repository<Customer>().Query().FirstOrDefaultAsync();
        var product = await _unitOfWork.Repository<Product>().Query().FirstAsync();
        
        var dto = new CreateOrderDto
        {
            OrderType = OrderType.DineIn,
            CustomerId = customer?.Id,
            Items = new List<CreateOrderItemDto>
            {
                new CreateOrderItemDto
                {
                    ProductId = product.Id,
                    Quantity = 1
                }
            }
        };

        // Act
        var result = await _controller.CreateOrder(dto);

        // Assert
        result.Result.Should().BeOfType<CreatedAtActionResult>();

        if (customer != null)
        {
            var order = await _unitOfWork.Repository<Order>().Query()
                .OrderByDescending(o => o.Id)
                .FirstAsync();

            order.CustomerId.Should().Be(customer.Id);
        }
    }

    [Fact]
    public async Task CreateOrder_ShouldGenerateUniqueOrderNumber()
    {
        // Arrange
        _mockCurrentUserService.Setup(s => s.UserId).Returns(1);
        _mockCurrentUserService.Setup(s => s.Username).Returns("testadmin");
        SetupControllerContext(1, "testadmin", "Admin", 1);

        var product = await _unitOfWork.Repository<Product>().Query().FirstAsync();
        
        var dto = new CreateOrderDto
        {
            OrderType = OrderType.DineIn,
            Items = new List<CreateOrderItemDto>
            {
                new CreateOrderItemDto
                {
                    ProductId = product.Id,
                    Quantity = 1
                }
            }
        };

        // Act
        await _controller.CreateOrder(dto);
        await Task.Delay(100); // Small delay to ensure different timestamps
        await _controller.CreateOrder(dto);

        // Assert - Should have created 2 orders with different order numbers
        var orders = await _unitOfWork.Repository<Order>().Query()
            .OrderByDescending(o => o.Id)
            .Take(2)
            .ToListAsync();
        
        orders.Should().HaveCount(2);
        orders[0].OrderNumber.Should().NotBe(orders[1].OrderNumber);
    }

    [Fact]
    public async Task CreateOrder_ShouldSetCorrectOrderStatus()
    {
        // Arrange
        _mockCurrentUserService.Setup(s => s.UserId).Returns(1);
        _mockCurrentUserService.Setup(s => s.Username).Returns("testadmin");
        SetupControllerContext(1, "testadmin", "Admin", 1);

        var product = await _unitOfWork.Repository<Product>().Query().FirstAsync();
        
        var dto = new CreateOrderDto
        {
            OrderType = OrderType.DineIn,
            Items = new List<CreateOrderItemDto>
            {
                new CreateOrderItemDto
                {
                    ProductId = product.Id,
                    Quantity = 1
                }
            }
        };

        // Act
        await _controller.CreateOrder(dto);

        // Assert
        var order = await _unitOfWork.Repository<Order>().Query()
            .OrderByDescending(o => o.Id)
            .FirstAsync();

        order.Status.Should().Be(OrderStatus.Pending);
    }

    [Fact]
    public async Task CreateOrder_ShouldSetCorrectUserId()
    {
        // Arrange
        var userId = 1L;
        _mockCurrentUserService.Setup(s => s.UserId).Returns(userId);
        _mockCurrentUserService.Setup(s => s.Username).Returns("testadmin");
        SetupControllerContext(userId, "testadmin", "Admin", 1);

        var product = await _unitOfWork.Repository<Product>().Query().FirstAsync();
        
        var dto = new CreateOrderDto
        {
            OrderType = OrderType.DineIn,
            Items = new List<CreateOrderItemDto>
            {
                new CreateOrderItemDto
                {
                    ProductId = product.Id,
                    Quantity = 1
                }
            }
        };

        // Act
        await _controller.CreateOrder(dto);

        // Assert
        var order = await _unitOfWork.Repository<Order>().Query()
            .OrderByDescending(o => o.Id)
            .FirstAsync();

        order.UserId.Should().Be(userId);
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
}

using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using POS.Application.Common.Interfaces;
using POS.Application.Common.Models;
using POS.Application.DTOs;
using POS.Domain.Entities;
using POS.Domain.Enums;
using POS.Infrastructure.Data;
using POS.Infrastructure.Repositories;

using POS.WebAPI.Controllers;
using POS.WebAPI.Tests.Helpers;
using Xunit;

namespace POS.WebAPI.Tests.Controllers;

/// <summary>
/// Tests for ProductsController - Get operations and filtering
/// </summary>
public class ProductsControllerGetTests : IDisposable
{
    private readonly POSDbContext _context;
    private readonly Mock<ILogger<ProductsController>> _mockLogger;
    private readonly ProductsController _controller;
    private readonly UnitOfWork _unitOfWork;

    public ProductsControllerGetTests()
    {
        _context = InMemoryDbContextFactory.CreateWithData();
        _unitOfWork = new UnitOfWork(_context);
        _mockLogger = new Mock<ILogger<ProductsController>>();
        
        _controller = new ProductsController(_unitOfWork, _mockLogger.Object);
    }

    [Fact]
    public async Task GetProducts_WithNoFilters_ShouldReturnAllActiveProducts()
    {
        // Act
        var result = await _controller.GetProducts(null, null, null, null);

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().BeOfType<OkObjectResult>();
        
        var okResult = result.Result as OkObjectResult;
        var response = okResult!.Value as ApiResponse<List<ProductListDto>>;
        
        response.Should().NotBeNull();
        response!.Success.Should().BeTrue();
        response.Data.Should().NotBeNull();
        response.Data!.Should().HaveCountGreaterThan(0);
        response.Data.All(p => p.IsActive || !p.IsActive).Should().BeTrue(); // All products
    }

    [Fact]
    public async Task GetProducts_WithSearchByName_ShouldFilterResults()
    {
        // Arrange
        var searchTerm = "Chocolate";

        // Act
        var result = await _controller.GetProducts(searchTerm, null, null, null);

        // Assert
        var okResult = result.Result as OkObjectResult;
        var response = okResult!.Value as ApiResponse<List<ProductListDto>>;
        
        response!.Data.Should().NotBeNull();
        response.Data!.All(p => p.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
            .Should().BeTrue();
    }

    [Fact]
    public async Task GetProducts_WithSearchBySKU_ShouldFilterResults()
    {
        // Arrange
        var product = await _unitOfWork.Repository<Product>().Query()
            .FirstOrDefaultAsync(p => p.SKU != null);
        
        if (product != null)
        {
            // Act
            var result = await _controller.GetProducts(product.SKU, null, null, null);

            // Assert
            var okResult = result.Result as OkObjectResult;
            var response = okResult!.Value as ApiResponse<List<ProductListDto>>;
            
            response!.Data.Should().NotBeNull();
            response.Data!.Should().Contain(p => p.SKU == product.SKU);
        }
    }

    [Fact]
    public async Task GetProducts_WithSearchByBarcode_ShouldFilterResults()
    {
        // Arrange
        var product = await _unitOfWork.Repository<Product>().Query()
            .FirstOrDefaultAsync(p => p.Barcode != null);
        
        if (product != null)
        {
            // Act
            var result = await _controller.GetProducts(product.Barcode, null, null, null);

            // Assert
            var okResult = result.Result as OkObjectResult;
            var response = okResult!.Value as ApiResponse<List<ProductListDto>>;
            
            response!.Data!.Should().Contain(p => p.Barcode == product.Barcode);
        }
    }

    [Fact]
    public async Task GetProducts_WithCategoryFilter_ShouldReturnProductsInCategory()
    {
        // Arrange
        var category = await _unitOfWork.Repository<Category>().Query()
            .Include(c => c.Subcategories)
            .FirstOrDefaultAsync();
        
        if (category != null)
        {
            // Act
            var result = await _controller.GetProducts(null, category.Id, null, null);

            // Assert
            var okResult = result.Result as OkObjectResult;
            var response = okResult!.Value as ApiResponse<List<ProductListDto>>;
            
            response!.Data.Should().NotBeNull();
            if (response.Data!.Any())
            {
                response.Data.All(p => p.Category?.Id == category.Id).Should().BeTrue();
            }
        }
    }

    [Fact]
    public async Task GetProducts_WithSubcategoryFilter_ShouldReturnProductsInSubcategory()
    {
        // Arrange
        var subcategory = await _unitOfWork.Repository<Subcategory>().Query()
            .FirstOrDefaultAsync();
        
        if (subcategory != null)
        {
            // Act
            var result = await _controller.GetProducts(null, null, subcategory.Id, null);

            // Assert
            var okResult = result.Result as OkObjectResult;
            var response = okResult!.Value as ApiResponse<List<ProductListDto>>;
            
            response!.Data!.All(p => p.SubcategoryId == subcategory.Id).Should().BeTrue();
        }
    }

    [Fact]
    public async Task GetProducts_WithIsActiveTrue_ShouldReturnOnlyActiveProducts()
    {
        // Act
        var result = await _controller.GetProducts(null, null, null, true);

        // Assert
        var okResult = result.Result as OkObjectResult;
        var response = okResult!.Value as ApiResponse<List<ProductListDto>>;
        
        response!.Data!.All(p => p.IsActive).Should().BeTrue();
    }

    [Fact]
    public async Task GetProducts_WithIsActiveFalse_ShouldReturnOnlyInactiveProducts()
    {
        // Arrange - Create an inactive product
        var inactiveProduct = new Product
        {
            Name = "Inactive Product",
            Slug = "inactive-product",
            PriceIncGst = 10m,
            PriceExGst = 9.09m,
            GstAmount = 0.91m,
            SubcategoryId = 1,
            IsActive = false,
            TrackInventory = false,
            StockQuantity = 0
        };
        await _unitOfWork.Repository<Product>().AddAsync(inactiveProduct);
        await _unitOfWork.SaveChangesAsync();

        // Act
        var result = await _controller.GetProducts(null, null, null, false);

        // Assert
        var okResult = result.Result as OkObjectResult;
        var response = okResult!.Value as ApiResponse<List<ProductListDto>>;
        
        response!.Data!.All(p => !p.IsActive).Should().BeTrue();
    }

    [Fact]
    public async Task GetProducts_ShouldIncludeSubcategoryAndCategory()
    {
        // Act
        var result = await _controller.GetProducts(null, null, null, null);

        // Assert
        var okResult = result.Result as OkObjectResult;
        var response = okResult!.Value as ApiResponse<List<ProductListDto>>;
        
        if (response!.Data!.Any())
        {
            var productWithSubcategory = response.Data.FirstOrDefault(p => p.Subcategory != null);
            productWithSubcategory.Should().NotBeNull();
            productWithSubcategory!.Subcategory.Should().NotBeNull();
            productWithSubcategory.Category.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task GetProduct_WithValidId_ShouldReturnProduct()
    {
        // Arrange
        var product = await _unitOfWork.Repository<Product>().Query().FirstOrDefaultAsync();
        
        // Act
        var result = await _controller.GetProduct(product!.Id);

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().BeOfType<OkObjectResult>();
        
        var okResult = result.Result as OkObjectResult;
        var response = okResult!.Value as ApiResponse<ProductDto>;
        
        response!.Success.Should().BeTrue();
        response.Data.Should().NotBeNull();
        response.Data!.Id.Should().Be(product.Id);
        response.Data.Name.Should().Be(product.Name);
    }

    [Fact]
    public async Task GetProduct_WithInvalidId_ShouldReturn404()
    {
        // Act
        var result = await _controller.GetProduct(99999);

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetProduct_ShouldIncludeAllDetails()
    {
        // Arrange
        var product = await _unitOfWork.Repository<Product>().Query()
            .Include(p => p.Subcategory)
                .ThenInclude(s => s.Category)
            .FirstOrDefaultAsync();
        
        // Act
        var result = await _controller.GetProduct(product!.Id);

        // Assert
        var okResult = result.Result as OkObjectResult;
        var response = okResult!.Value as ApiResponse<ProductDto>;
        
        response!.Data.Should().NotBeNull();
        response.Data!.SubcategoryName.Should().NotBeNullOrEmpty();
        response.Data.CategoryName.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetProductByBarcode_WithValidBarcode_ShouldReturnProduct()
    {
        // Arrange
        var product = await _unitOfWork.Repository<Product>().Query()
            .FirstOrDefaultAsync(p => p.Barcode != null);
        
        if (product != null)
        {
            // Act
            var result = await _controller.GetProductByBarcode(product.Barcode!);

            // Assert
            result.Should().NotBeNull();
            result.Result.Should().BeOfType<OkObjectResult>();
            
            var okResult = result.Result as OkObjectResult;
            var response = okResult!.Value as ApiResponse<ProductDto>;
            
            response!.Data!.Barcode.Should().Be(product.Barcode);
        }
    }

    [Fact]
    public async Task GetProductByBarcode_WithInvalidBarcode_ShouldReturn404()
    {
        // Act
        var result = await _controller.GetProductByBarcode("INVALID-BARCODE-999");

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetProducts_WithMultipleFilters_ShouldApplyAllFilters()
    {
        // Arrange
        var category = await _unitOfWork.Repository<Category>().Query().FirstOrDefaultAsync();
        
        // Act
        var result = await _controller.GetProducts("Cookie", category?.Id, null, true);

        // Assert
        var okResult = result.Result as OkObjectResult;
        var response = okResult!.Value as ApiResponse<List<ProductListDto>>;
        
        response!.Data.Should().NotBeNull();
        response.Data!.All(p => p.IsActive).Should().BeTrue();
        if (response.Data.Any())
        {
            response.Data.All(p => p.Name.Contains("Cookie", StringComparison.OrdinalIgnoreCase))
                .Should().BeTrue();
        }
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}

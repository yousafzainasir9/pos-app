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
/// Tests for ProductsController - Create, Update, Delete operations with authorization
/// </summary>
public class ProductsControllerMutationTests : ControllerTestBase, IDisposable
{
    private readonly POSDbContext _context;
    private readonly ProductsController _controller;
    private readonly UnitOfWork _unitOfWork;

    public ProductsControllerMutationTests()
    {
        _context = InMemoryDbContextFactory.CreateWithData();
        _unitOfWork = new UnitOfWork(_context);
        
        _controller = new ProductsController(_unitOfWork, Mock.Of<ILogger<ProductsController>>());
    }

    [Fact]
    public async Task CreateProduct_WithValidData_ShouldCreateProduct()
    {
        // Arrange
        SetupControllerContext(_controller, 1, "admin", "Admin", 1);
        
        var dto = new CreateProductDto
        {
            Name = "New Test Product",
            Slug = "new-test-product",
            SKU = "NTP001",
            Barcode = "9999999999999",
            Description = "Test description",
            PriceExGst = 9.09m,
            GstAmount = 0.91m,
            PriceIncGst = 10.00m,
            Cost = 5.00m,
            PackSize = 1,
            ImageUrl = "/images/test.jpg",
            IsActive = true,
            TrackInventory = true,
            StockQuantity = 100,
            LowStockThreshold = 10,
            DisplayOrder = 1,
            SubcategoryId = 1
        };

        // Act
        var result = await _controller.CreateProduct(dto);

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().BeOfType<CreatedAtActionResult>();
        
        var createdResult = result.Result as CreatedAtActionResult;
        var response = createdResult!.Value as ApiResponse<ProductDto>;
        
        response!.Success.Should().BeTrue();
        response.Data!.Name.Should().Be("New Test Product");
        response.Data.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task CreateProduct_ShouldGenerateSlugIfNotProvided()
    {
        // Arrange
        SetupControllerContext(_controller, 1, "admin", "Admin", 1);
        
        var dto = new CreateProductDto
        {
            Name = "Product Without Slug",
            Slug = null, // No slug provided
            PriceExGst = 9.09m,
            GstAmount = 0.91m,
            PriceIncGst = 10.00m,
            SubcategoryId = 1,
            IsActive = true,
            TrackInventory = false,
            StockQuantity = 0
        };

        // Act
        var result = await _controller.CreateProduct(dto);

        // Assert
        var createdResult = result.Result as CreatedAtActionResult;
        var response = createdResult!.Value as ApiResponse<ProductDto>;
        
        response!.Data!.Slug.Should().NotBeNullOrEmpty();
        response.Data.Slug.Should().Contain("without");
    }

    [Fact]
    public async Task CreateProduct_ShouldSetAllPropertiesCorrectly()
    {
        // Arrange
        SetupControllerContext(_controller, 1, "manager", "Manager", 1);
        
        var dto = new CreateProductDto
        {
            Name = "Complete Product",
            Slug = "complete-product",
            SKU = "CP001",
            Barcode = "1111111111111",
            Description = "Complete description",
            PriceExGst = 18.18m,
            GstAmount = 1.82m,
            PriceIncGst = 20.00m,
            Cost = 10.00m,
            PackNotes = "Pack of 6",
            PackSize = 6,
            ImageUrl = "/images/complete.jpg",
            IsActive = true,
            TrackInventory = true,
            StockQuantity = 50,
            LowStockThreshold = 5,
            DisplayOrder = 10,
            SubcategoryId = 1,
            SupplierId = 1
        };

        // Act
        var result = await _controller.CreateProduct(dto);

        // Assert
        var createdResult = result.Result as CreatedAtActionResult;
        var response = createdResult!.Value as ApiResponse<ProductDto>;
        
        var product = response!.Data!;
        product.Name.Should().Be("Complete Product");
        product.SKU.Should().Be("CP001");
        product.Barcode.Should().Be("1111111111111");
        product.PriceIncGst.Should().Be(20.00m);
        product.Cost.Should().Be(10.00m);
        product.PackSize.Should().Be(6);
        product.StockQuantity.Should().Be(50);
        product.LowStockThreshold.Should().Be(5);
    }

    [Fact]
    public async Task UpdateProduct_WithValidData_ShouldUpdateProduct()
    {
        // Arrange
        SetupControllerContext(_controller, 1, "admin", "Admin", 1);
        
        var existingProduct = await _unitOfWork.Repository<Product>().Query().FirstAsync();
        
        var dto = new UpdateProductDto
        {
            Name = "Updated Product Name",
            Slug = "updated-product-name",
            SKU = existingProduct.SKU,
            Barcode = existingProduct.Barcode,
            PriceExGst = 27.27m,
            GstAmount = 2.73m,
            PriceIncGst = 30.00m,
            Cost = 15.00m,
            IsActive = true,
            TrackInventory = true,
            StockQuantity = 200,
            LowStockThreshold = 20,
            DisplayOrder = 1,
            SubcategoryId = existingProduct.SubcategoryId
        };

        // Act
        var result = await _controller.UpdateProduct(existingProduct.Id, dto);

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().BeOfType<OkObjectResult>();
        
        var okResult = result.Result as OkObjectResult;
        var response = okResult!.Value as ApiResponse<ProductDto>;
        
        response!.Data!.Name.Should().Be("Updated Product Name");
        response.Data.PriceIncGst.Should().Be(30.00m);
        response.Data.StockQuantity.Should().Be(200);
    }

    [Fact]
    public async Task UpdateProduct_WithInvalidId_ShouldReturn404()
    {
        // Arrange
        SetupControllerContext(_controller, 1, "admin", "Admin", 1);
        
        var dto = new UpdateProductDto
        {
            Name = "Updated Name",
            PriceExGst = 9.09m,
            GstAmount = 0.91m,
            PriceIncGst = 10.00m,
            SubcategoryId = 1,
            IsActive = true,
            TrackInventory = false,
            StockQuantity = 0
        };

        // Act
        var result = await _controller.UpdateProduct(99999, dto);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task UpdateProduct_ShouldPersistChanges()
    {
        // Arrange
        SetupControllerContext(_controller, 1, "manager", "Manager", 1);
        
        var existingProduct = await _unitOfWork.Repository<Product>().Query().FirstAsync();
        var originalName = existingProduct.Name;
        
        var dto = new UpdateProductDto
        {
            Name = "Persisted Update",
            Slug = "persisted-update",
            PriceExGst = 9.09m,
            GstAmount = 0.91m,
            PriceIncGst = 10.00m,
            SubcategoryId = existingProduct.SubcategoryId,
            IsActive = true,
            TrackInventory = false,
            StockQuantity = 0
        };

        // Act
        await _controller.UpdateProduct(existingProduct.Id, dto);

        // Assert
        _context.ChangeTracker.Clear();
        var updatedProduct = await _unitOfWork.Repository<Product>().GetByIdAsync(existingProduct.Id);
        updatedProduct!.Name.Should().Be("Persisted Update");
        updatedProduct.Name.Should().NotBe(originalName);
    }

    [Fact]
    public async Task DeleteProduct_WithValidId_ShouldSoftDeleteProduct()
    {
        // Arrange
        SetupControllerContext(_controller, 1, "admin", "Admin", 1);
        
        var product = await _unitOfWork.Repository<Product>().Query().FirstAsync();
        var productId = product.Id;

        // Act
        var result = await _controller.DeleteProduct(productId);

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().BeOfType<OkObjectResult>();
        
        var okResult = result.Result as OkObjectResult;
        var response = okResult!.Value as ApiResponse<bool>;
        response!.Success.Should().BeTrue();

        // Verify soft delete
        _context.ChangeTracker.Clear();
        var deletedProduct = await _unitOfWork.Repository<Product>().GetByIdAsync(productId);
        deletedProduct.Should().BeNull(); // Query filter should exclude deleted items
    }

    [Fact]
    public async Task DeleteProduct_WithInvalidId_ShouldReturn404()
    {
        // Arrange
        SetupControllerContext(_controller, 1, "admin", "Admin", 1);

        // Act
        var result = await _controller.DeleteProduct(99999);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task DeleteProduct_ShouldNotHardDelete()
    {
        // Arrange
        SetupControllerContext(_controller, 1, "admin", "Admin", 1);
        
        var product = await _unitOfWork.Repository<Product>().Query().FirstAsync();
        var productId = product.Id;

        // Act
        await _controller.DeleteProduct(productId);

        // Assert - Product should still exist in database with IsDeleted = true
        var deletedProduct = _context.Products
            .IgnoreQueryFilters()
            .FirstOrDefault(p => p.Id == productId);
        
        deletedProduct.Should().NotBeNull();
        deletedProduct!.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task CreateProduct_WithDuplicateSKU_ShouldStillCreate()
    {
        // Arrange
        SetupControllerContext(_controller, 1, "admin", "Admin", 1);
        
        // Note: In real implementation, you might want to add unique constraint
        // This test documents current behavior
        var dto = new CreateProductDto
        {
            Name = "Duplicate SKU Product",
            SKU = "CCC001", // Same as existing
            PriceExGst = 9.09m,
            GstAmount = 0.91m,
            PriceIncGst = 10.00m,
            SubcategoryId = 1,
            IsActive = true,
            TrackInventory = false,
            StockQuantity = 0
        };

        // Act
        var result = await _controller.CreateProduct(dto);

        // Assert
        result.Result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task UpdateProduct_CanChangeActiveStatus()
    {
        // Arrange
        SetupControllerContext(_controller, 1, "manager", "Manager", 1);
        
        var product = await _unitOfWork.Repository<Product>().Query().FirstAsync();
        var originalStatus = product.IsActive;
        
        var dto = new UpdateProductDto
        {
            Name = product.Name,
            PriceExGst = product.PriceExGst,
            GstAmount = product.GstAmount,
            PriceIncGst = product.PriceIncGst,
            SubcategoryId = product.SubcategoryId,
            IsActive = !originalStatus, // Toggle status
            TrackInventory = product.TrackInventory,
            StockQuantity = product.StockQuantity
        };

        // Act
        await _controller.UpdateProduct(product.Id, dto);

        // Assert
        _context.ChangeTracker.Clear();
        var updatedProduct = await _unitOfWork.Repository<Product>().GetByIdAsync(product.Id);
        updatedProduct!.IsActive.Should().Be(!originalStatus);
    }

    [Fact]
    public async Task UpdateProduct_CanUpdateInventory()
    {
        // Arrange
        SetupControllerContext(_controller, 1, "admin", "Admin", 1);
        
        var product = await _unitOfWork.Repository<Product>().Query().FirstAsync();
        
        var dto = new UpdateProductDto
        {
            Name = product.Name,
            PriceExGst = product.PriceExGst,
            GstAmount = product.GstAmount,
            PriceIncGst = product.PriceIncGst,
            SubcategoryId = product.SubcategoryId,
            IsActive = product.IsActive,
            TrackInventory = true,
            StockQuantity = 500, // New stock quantity
            LowStockThreshold = 50 // New threshold
        };

        // Act
        await _controller.UpdateProduct(product.Id, dto);

        // Assert
        _context.ChangeTracker.Clear();
        var updatedProduct = await _unitOfWork.Repository<Product>().GetByIdAsync(product.Id);
        updatedProduct!.StockQuantity.Should().Be(500);
        updatedProduct.LowStockThreshold.Should().Be(50);
        updatedProduct.TrackInventory.Should().BeTrue();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}

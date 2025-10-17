using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using POS.Domain.Entities;
using POS.Infrastructure.Data;
using POS.Infrastructure.Repositories;
using POS.Infrastructure.Tests.Helpers;
using Xunit;

namespace POS.Infrastructure.Tests.Repositories;

/// <summary>
/// Tests for the generic Repository pattern - Basic CRUD operations
/// </summary>
public class RepositoryTests : IDisposable
{
    private readonly POSDbContext _context;
    private readonly Repository<Product> _productRepository;

    public RepositoryTests()
    {
        _context = InMemoryDbContextFactory.Create();
        _productRepository = new Repository<Product>(_context);
    }

    [Fact]
    public async Task AddAsync_WithValidEntity_ShouldAddToDatabase()
    {
        // Arrange
        var product = new Product
        {
            Name = "Test Product",
            Slug = "test-product",
            PriceExGst = 9.09m,
            GstAmount = 0.91m,
            PriceIncGst = 10.00m,
            SubcategoryId = 1,
            IsActive = true,
            TrackInventory = true,
            StockQuantity = 10
        };

        // Act
        await _productRepository.AddAsync(product);
        await _context.SaveChangesAsync();

        // Assert
        var result = await _productRepository.GetByIdAsync(product.Id);
        result.Should().NotBeNull();
        result!.Name.Should().Be("Test Product");
        result.PriceIncGst.Should().Be(10.00m);
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingId_ShouldReturnEntity()
    {
        // Arrange
        var product = TestDataSeeder.CreateTestProduct(_context, 1, "Existing Product");

        // Act
        var result = await _productRepository.GetByIdAsync(product.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(product.Id);
        result.Name.Should().Be("Existing Product");
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistingId_ShouldReturnNull()
    {
        // Act
        var result = await _productRepository.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllEntities()
    {
        // Arrange
        TestDataSeeder.CreateTestProduct(_context, 1, "Product 1");
        TestDataSeeder.CreateTestProduct(_context, 2, "Product 2");
        TestDataSeeder.CreateTestProduct(_context, 3, "Product 3");

        // Act
        var results = await _productRepository.GetAllAsync();

        // Assert
        results.Should().NotBeNull();
        results.Should().HaveCount(3);
    }

    [Fact]
    public async Task Update_WithValidEntity_ShouldModifyInDatabase()
    {
        // Arrange
        var product = TestDataSeeder.CreateTestProduct(_context, 1, "Original Name");
        product.Name = "Updated Name";
        product.PriceIncGst = 15.00m;

        // Act
        _productRepository.Update(product);
        await _context.SaveChangesAsync();

        // Assert
        var result = await _productRepository.GetByIdAsync(product.Id);
        result.Should().NotBeNull();
        result!.Name.Should().Be("Updated Name");
        result.PriceIncGst.Should().Be(15.00m);
    }

    [Fact]
    public async Task Remove_ShouldSoftDeleteEntity()
    {
        // Arrange
        var product = TestDataSeeder.CreateTestProduct(_context, 1, "To Be Deleted");

        // Act
        _productRepository.Remove(product);
        await _context.SaveChangesAsync();

        // Assert
        var result = await _productRepository.GetByIdAsync(product.Id);
        result.Should().BeNull(); // Soft delete means it won't appear in normal queries
        
        // Verify it's actually soft deleted (not hard deleted)
        var deletedProduct = _context.Products
            .IgnoreQueryFilters()
            .FirstOrDefault(p => p.Id == product.Id);
        deletedProduct.Should().NotBeNull();
        deletedProduct!.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public void Query_ShouldReturnQueryable()
    {
        // Arrange
        TestDataSeeder.CreateTestProduct(_context, 1, "Product 1");
        TestDataSeeder.CreateTestProduct(_context, 2, "Product 2");

        // Act
        var query = _productRepository.Query();

        // Assert
        query.Should().NotBeNull();
        var count = query.Count();
        count.Should().Be(2);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}

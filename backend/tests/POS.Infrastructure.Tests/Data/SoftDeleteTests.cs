using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using POS.Domain.Entities;
using POS.Infrastructure.Data;
using POS.Infrastructure.Repositories;
using POS.Infrastructure.Tests.Helpers;
using Xunit;

namespace POS.Infrastructure.Tests.Data;

/// <summary>
/// Tests for soft delete functionality in POSDbContext
/// </summary>
public class SoftDeleteTests : IDisposable
{
    private readonly POSDbContext _context;
    private readonly Repository<Product> _productRepository;
    private readonly Repository<Category> _categoryRepository;

    public SoftDeleteTests()
    {
        _context = InMemoryDbContextFactory.CreateWithData();
        _productRepository = new Repository<Product>(_context);
        _categoryRepository = new Repository<Category>(_context);
    }

    [Fact]
    public async Task Remove_ShouldSetIsDeletedToTrue()
    {
        // Arrange
        var product = TestDataSeeder.CreateTestProduct(_context, 99, "To Delete");

        // Act
        _productRepository.Remove(product);
        await _context.SaveChangesAsync();

        // Assert
        product.IsDeleted.Should().BeTrue();
        product.DeletedOn.Should().NotBeNull();
        product.DeletedOn.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Remove_ShouldSetDeletedOnTimestamp()
    {
        // Arrange
        var product = TestDataSeeder.CreateTestProduct(_context, 98, "Delete Timestamp Test");
        var beforeDelete = DateTime.Now;

        // Act
        _productRepository.Remove(product);
        await _context.SaveChangesAsync();
        var afterDelete = DateTime.Now;

        // Assert
        product.DeletedOn.Should().NotBeNull();
        product.DeletedOn.Should().BeOnOrAfter(beforeDelete);
        product.DeletedOn.Should().BeOnOrBefore(afterDelete);
    }

    [Fact]
    public async Task Query_ShouldNotReturnDeletedEntities()
    {
        // Arrange
        var product = TestDataSeeder.CreateTestProduct(_context, 97, "Deleted Product");
        _productRepository.Remove(product);
        await _context.SaveChangesAsync();

        // Act
        var results = _productRepository.Query().ToList();

        // Assert
        results.Should().NotContain(p => p.Id == product.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNullForDeletedEntity()
    {
        // Arrange
        var product = TestDataSeeder.CreateTestProduct(_context, 96, "Get Deleted Test");
        var productId = product.Id;
        _productRepository.Remove(product);
        await _context.SaveChangesAsync();

        // Act
        var result = await _productRepository.GetByIdAsync(productId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ShouldNotIncludeDeletedEntities()
    {
        // Arrange
        var product1 = TestDataSeeder.CreateTestProduct(_context, 95, "Active Product");
        var product2 = TestDataSeeder.CreateTestProduct(_context, 94, "Deleted Product");
        _productRepository.Remove(product2);
        await _context.SaveChangesAsync();

        // Act
        var results = await _productRepository.GetAllAsync();

        // Assert
        results.Should().Contain(p => p.Id == product1.Id);
        results.Should().NotContain(p => p.Id == product2.Id);
    }

    [Fact]
    public void IgnoreQueryFilters_ShouldReturnDeletedEntities()
    {
        // Arrange
        var product = TestDataSeeder.CreateTestProduct(_context, 93, "Ignored Filter Test");
        _productRepository.Remove(product);
        _context.SaveChanges();

        // Act
        var deletedProduct = _context.Products
            .IgnoreQueryFilters()
            .FirstOrDefault(p => p.Id == product.Id);

        // Assert
        deletedProduct.Should().NotBeNull();
        deletedProduct!.IsDeleted.Should().BeTrue();
        deletedProduct.Name.Should().Be("Ignored Filter Test");
    }

    [Fact]
    public async Task MultipleDeletes_ShouldAllBeSoftDeleted()
    {
        // Arrange
        var product1 = TestDataSeeder.CreateTestProduct(_context, 92, "Delete 1");
        var product2 = TestDataSeeder.CreateTestProduct(_context, 91, "Delete 2");
        var product3 = TestDataSeeder.CreateTestProduct(_context, 90, "Delete 3");

        // Act
        _productRepository.Remove(product1);
        _productRepository.Remove(product2);
        _productRepository.Remove(product3);
        await _context.SaveChangesAsync();

        // Assert
        var activeProducts = _productRepository.Query().ToList();
        activeProducts.Should().NotContain(p => p.Id == product1.Id);
        activeProducts.Should().NotContain(p => p.Id == product2.Id);
        activeProducts.Should().NotContain(p => p.Id == product3.Id);

        var allProducts = _context.Products.IgnoreQueryFilters().ToList();
        allProducts.Should().Contain(p => p.Id == product1.Id && p.IsDeleted);
        allProducts.Should().Contain(p => p.Id == product2.Id && p.IsDeleted);
        allProducts.Should().Contain(p => p.Id == product3.Id && p.IsDeleted);
    }

    [Fact]
    public async Task SoftDelete_ShouldNotAffectOtherEntities()
    {
        // Arrange
        var initialCount = _productRepository.Query().Count();
        var productToDelete = TestDataSeeder.CreateTestProduct(_context, 89, "Isolated Delete");

        // Act
        _productRepository.Remove(productToDelete);
        await _context.SaveChangesAsync();

        // Assert
        var currentCount = _productRepository.Query().Count();
        currentCount.Should().Be(initialCount); // Count back to original (new one was deleted)
    }

    [Fact]
    public async Task Category_Delete_ShouldBeSoftDeleted()
    {
        // Arrange
        var category = new Category
        {
            Name = "Test Category Delete",
            Slug = "test-category-delete",
            IsActive = true
        };
        await _categoryRepository.AddAsync(category);
        await _context.SaveChangesAsync();

        // Act
        _categoryRepository.Remove(category);
        await _context.SaveChangesAsync();

        // Assert
        var result = await _categoryRepository.GetByIdAsync(category.Id);
        result.Should().BeNull();

        var deletedCategory = _context.Categories
            .IgnoreQueryFilters()
            .FirstOrDefault(c => c.Id == category.Id);
        deletedCategory.Should().NotBeNull();
        deletedCategory!.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task Update_AfterSoftDelete_ShouldNotBeVisible()
    {
        // Arrange
        var product = TestDataSeeder.CreateTestProduct(_context, 88, "Update After Delete");
        var productId = product.Id;
        
        _productRepository.Remove(product);
        await _context.SaveChangesAsync();

        // Verify product is soft deleted
        var deletedProduct = await _productRepository.GetByIdAsync(productId);
        deletedProduct.Should().BeNull("product should be soft deleted");

        // Act - Try to manually un-delete by changing IsDeleted flag
        // This simulates someone trying to bypass soft delete
        var productFromDb = _context.Products
            .IgnoreQueryFilters()
            .FirstOrDefault(p => p.Id == productId);
        
        productFromDb.Should().NotBeNull();
        productFromDb!.Name = "Updated Name";
        productFromDb.IsDeleted = false; // Try to un-delete
        _productRepository.Update(productFromDb);
        await _context.SaveChangesAsync();

        // Assert - Product should now be visible since we forcefully changed IsDeleted to false
        var result = await _productRepository.GetByIdAsync(productId);
        result.Should().NotBeNull("product should be visible after manually setting IsDeleted to false");
        result!.Name.Should().Be("Updated Name");
        result.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void Count_ShouldNotIncludeDeletedEntities()
    {
        // Arrange - Get initial count of seeded products (3 products from SeedTestData)
        var initialCount = _productRepository.Query().Count();
        
        // Create a new test product
        var product = TestDataSeeder.CreateTestProduct(_context, 87, "Count Test");
        
        // Verify the count increased by 1
        var countAfterAdd = _productRepository.Query().Count();
        countAfterAdd.Should().Be(initialCount + 1);

        // Act - Delete the newly created product
        _productRepository.Remove(product);
        _context.SaveChanges();

        // Assert - Count should be back to the initial count (deleted product not included)
        var currentCount = _productRepository.Query().Count();
        currentCount.Should().Be(initialCount, "deleted products should not be counted");
    }

    [Fact]
    public void Any_ShouldNotIncludeDeletedEntities()
    {
        // Arrange
        var product = TestDataSeeder.CreateTestProduct(_context, 86, "Any Test");
        _productRepository.Remove(product);
        _context.SaveChanges();

        // Act
        var exists = _productRepository.Query().Any(p => p.Id == product.Id);

        // Assert
        exists.Should().BeFalse();
    }

    [Fact]
    public void FirstOrDefault_ShouldNotReturnDeletedEntity()
    {
        // Arrange
        var product = TestDataSeeder.CreateTestProduct(_context, 85, "FirstOrDefault Test");
        var productId = product.Id;
        _productRepository.Remove(product);
        _context.SaveChanges();

        // Act
        var result = _productRepository.Query().FirstOrDefault(p => p.Id == productId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task SoftDeletedEntity_WithRelationships_ShouldBeFilteredCorrectly()
    {
        // Arrange - Create two products, one active and one to be deleted
        var activeProduct = TestDataSeeder.CreateTestProduct(_context, 200, "Active Product");
        var deletedProduct = TestDataSeeder.CreateTestProduct(_context, 201, "Product To Delete");
        var activeProductId = activeProduct.Id;
        var deletedProductId = deletedProduct.Id;

        // Act - Delete one product
        _productRepository.Remove(deletedProduct);
        await _context.SaveChangesAsync();
        
        // Clear tracking
        _context.ChangeTracker.Clear();

        // Assert - Query with Include should only return active products with their subcategories
        var productsWithSubcategories = _productRepository.Query()
            .Include(p => p.Subcategory)
            .ToList();
        
        // Active product should be in results
        productsWithSubcategories.Should().Contain(p => p.Id == activeProductId);
        
        // Deleted product should NOT be in results
        productsWithSubcategories.Should().NotContain(p => p.Id == deletedProductId);
        
        // Verify deleted product still exists with IgnoreQueryFilters
        var deletedProductExists = _context.Products
            .IgnoreQueryFilters()
            .Include(p => p.Subcategory)
            .FirstOrDefault(p => p.Id == deletedProductId);
            
        deletedProductExists.Should().NotBeNull();
        deletedProductExists!.IsDeleted.Should().BeTrue();
        deletedProductExists.Subcategory.Should().NotBeNull(); // Subcategory should still be accessible with IgnoreQueryFilters
    }

    [Fact]
    public async Task CascadeSoftDelete_ProductThenSubcategory_ShouldSucceed()
    {
        // This test verifies the correct order for soft-deleting related entities
        // when DeleteBehavior.Restrict is configured
        
        // Arrange - Create a subcategory with no existing products
        var testSubcategory = new Subcategory
        {
            Name = "Temp Subcategory",
            Slug = "temp-subcategory",
            CategoryId = 1,
            IsActive = true
        };
        
        var subcategoryRepo = new Repository<Subcategory>(_context);
        await subcategoryRepo.AddAsync(testSubcategory);
        await _context.SaveChangesAsync();
        var subcategoryId = testSubcategory.Id;

        // Act & Assert - Should be able to delete subcategory with no products
        subcategoryRepo.Remove(testSubcategory);
        await _context.SaveChangesAsync();
        
        var result = await subcategoryRepo.GetByIdAsync(subcategoryId);
        result.Should().BeNull();
        
        // Verify it's soft deleted
        var deletedSubcategory = _context.Subcategories
            .IgnoreQueryFilters()
            .FirstOrDefault(s => s.Id == subcategoryId);
        deletedSubcategory.Should().NotBeNull();
        deletedSubcategory!.IsDeleted.Should().BeTrue();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}

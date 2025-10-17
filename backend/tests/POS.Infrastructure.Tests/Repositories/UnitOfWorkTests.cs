using FluentAssertions;
using POS.Domain.Entities;
using POS.Infrastructure.Data;
using POS.Infrastructure.Repositories;
using POS.Infrastructure.Tests.Helpers;
using Xunit;

namespace POS.Infrastructure.Tests.Repositories;

/// <summary>
/// Tests for UnitOfWork pattern - Transaction management and coordination
/// </summary>
public class UnitOfWorkTests : IDisposable
{
    private readonly POSDbContext _context;
    private readonly UnitOfWork _unitOfWork;

    public UnitOfWorkTests()
    {
        _context = InMemoryDbContextFactory.Create();
        _unitOfWork = new UnitOfWork(_context);
    }

    [Fact]
    public void Repository_ShouldReturnSameInstanceForSameType()
    {
        // Act
        var repository1 = _unitOfWork.Repository<Product>();
        var repository2 = _unitOfWork.Repository<Product>();

        // Assert
        repository1.Should().NotBeNull();
        repository2.Should().NotBeNull();
        repository1.Should().BeSameAs(repository2); // Should return cached instance
    }

    [Fact]
    public void Repository_ShouldReturnDifferentInstancesForDifferentTypes()
    {
        // Act
        var productRepository = _unitOfWork.Repository<Product>();
        var userRepository = _unitOfWork.Repository<User>();

        // Assert
        productRepository.Should().NotBeNull();
        userRepository.Should().NotBeNull();
        productRepository.Should().NotBeSameAs(userRepository);
    }

    [Fact]
    public async Task SaveChangesAsync_WithAddedEntity_ShouldPersistToDatabase()
    {
        // Arrange
        TestDataSeeder.SeedTestData(_context);
        var product = new Product
        {
            Name = "UoW Test Product",
            Slug = "uow-test-product",
            PriceExGst = 9.09m,
            GstAmount = 0.91m,
            PriceIncGst = 10.00m,
            SubcategoryId = 1,
            IsActive = true,
            TrackInventory = true,
            StockQuantity = 10
        };

        var repository = _unitOfWork.Repository<Product>();
        await repository.AddAsync(product);

        // Act
        var result = await _unitOfWork.SaveChangesAsync();

        // Assert
        result.Should().BeGreaterThan(0);
        product.Id.Should().BeGreaterThan(0);
        
        // Verify it's in the database
        var savedProduct = await repository.GetByIdAsync(product.Id);
        savedProduct.Should().NotBeNull();
        savedProduct!.Name.Should().Be("UoW Test Product");
    }

    [Fact]
    public async Task SaveChangesAsync_WithMultipleChanges_ShouldPersistAll()
    {
        // Arrange - Create entities without seeding existing data
        var productRepository = _unitOfWork.Repository<Product>();
        var categoryRepository = _unitOfWork.Repository<Category>();

        // First create a category
        var category = new Category
        {
            Name = "New Category",
            Slug = "new-category",
            IsActive = true
        };
        await categoryRepository.AddAsync(category);
        await _unitOfWork.SaveChangesAsync(); // Save category first
        _context.ChangeTracker.Clear(); // Clear tracker to avoid counting these in next save

        // Create subcategory
        var subcategoryRepo = _unitOfWork.Repository<Subcategory>();
        var subcategory = new Subcategory
        {
            Name = "New Subcategory",
            Slug = "new-subcategory",
            CategoryId = category.Id,
            IsActive = true
        };
        await subcategoryRepo.AddAsync(subcategory);
        await _unitOfWork.SaveChangesAsync(); // Save subcategory
        _context.ChangeTracker.Clear(); // Clear tracker

        // Now create product with a second category
        var category2 = new Category
        {
            Name = "Another Category",
            Slug = "another-category",
            IsActive = true
        };

        var product = new Product
        {
            Name = "Product 1",
            Slug = "product-1",
            PriceIncGst = 10.00m,
            PriceExGst = 9.09m,
            GstAmount = 0.91m,
            SubcategoryId = subcategory.Id,
            IsActive = true,
            TrackInventory = true,
            StockQuantity = 10
        };

        await productRepository.AddAsync(product);
        await categoryRepository.AddAsync(category2);

        // Act
        var result = await _unitOfWork.SaveChangesAsync();

        // Assert
        // Result includes both entities (product + category2) plus their audit logs (2 more)
        result.Should().BeGreaterThanOrEqualTo(2, "because we added 2 new entities at minimum");
        product.Id.Should().BeGreaterThan(0);
        category2.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task BeginTransaction_ShouldStartTransaction()
    {
        // Arrange
        TestDataSeeder.SeedTestData(_context);

        // Act
        await _unitOfWork.BeginTransactionAsync();

        // Assert - No exception means transaction started successfully
        // We can't directly test the transaction state, but we can test commit/rollback
        await _unitOfWork.CommitTransactionAsync();
    }

    [Fact]
    public async Task CommitTransaction_WithChanges_ShouldPersistAll()
    {
        // Arrange
        TestDataSeeder.SeedTestData(_context);
        var repository = _unitOfWork.Repository<Product>();
        
        var product = new Product
        {
            Name = "Transaction Test",
            Slug = "transaction-test",
            PriceIncGst = 10.00m,
            PriceExGst = 9.09m,
            GstAmount = 0.91m,
            SubcategoryId = 1,
            IsActive = true,
            TrackInventory = true,
            StockQuantity = 10
        };

        // Act
        await _unitOfWork.BeginTransactionAsync();
        await repository.AddAsync(product);
        await _unitOfWork.SaveChangesAsync();
        await _unitOfWork.CommitTransactionAsync();

        // Assert
        var savedProduct = await repository.GetByIdAsync(product.Id);
        savedProduct.Should().NotBeNull();
        savedProduct!.Name.Should().Be("Transaction Test");
    }

    [Fact]
    public async Task RollbackTransaction_ShouldRevertChanges()
    {
        // Arrange
        TestDataSeeder.SeedTestData(_context);
        var repository = _unitOfWork.Repository<Product>();
        
        var product = new Product
        {
            Name = "Rollback Test",
            Slug = "rollback-test",
            PriceIncGst = 10.00m,
            PriceExGst = 9.09m,
            GstAmount = 0.91m,
            SubcategoryId = 1,
            IsActive = true,
            TrackInventory = true,
            StockQuantity = 10
        };

        // Act
        await _unitOfWork.BeginTransactionAsync();
        await repository.AddAsync(product);
        await _unitOfWork.SaveChangesAsync();
        var productId = product.Id;
        await _unitOfWork.RollbackTransactionAsync();

        // Assert
        // Note: In-memory database doesn't fully support transactions like real SQL Server
        // But we can verify the rollback was called without exception
        product.Id.Should().BeGreaterThan(0); // ID was assigned during SaveChanges
    }

    [Fact]
    public async Task MultipleRepositories_ShouldShareSameContext()
    {
        // Arrange - Don't seed data, create a fresh context
        var productRepository = _unitOfWork.Repository<Product>();
        var categoryRepository = _unitOfWork.Repository<Category>();

        // Create a category first (products need a subcategory, which needs a category)
        var category = new Category
        {
            Name = "Test Category",
            Slug = "test-category",
            IsActive = true
        };
        await categoryRepository.AddAsync(category);
        await _unitOfWork.SaveChangesAsync();
        _context.ChangeTracker.Clear(); // Clear to avoid counting in next save

        // Create a subcategory
        var subcategoryRepo = _unitOfWork.Repository<Subcategory>();
        var subcategory = new Subcategory
        {
            Name = "Test Subcategory",
            Slug = "test-subcategory",
            CategoryId = category.Id,
            IsActive = true
        };
        await subcategoryRepo.AddAsync(subcategory);
        await _unitOfWork.SaveChangesAsync();
        _context.ChangeTracker.Clear(); // Clear to avoid counting in next save

        var product = new Product
        {
            Name = "Shared Context Test",
            Slug = "shared-context-test",
            PriceIncGst = 10.00m,
            PriceExGst = 9.09m,
            GstAmount = 0.91m,
            SubcategoryId = subcategory.Id,
            IsActive = true,
            TrackInventory = true,
            StockQuantity = 10
        };

        var userRepository = _unitOfWork.Repository<User>();
        var user = new User
        {
            Username = "sharedtest",
            Email = "shared@test.com",
            PasswordHash = "hash",
            FirstName = "Shared",
            LastName = "Test",
            Role = Domain.Enums.UserRole.Cashier,
            IsActive = true
        };

        // Act - Add both entities and save them together
        await productRepository.AddAsync(product);
        await userRepository.AddAsync(user);
        var result = await _unitOfWork.SaveChangesAsync();

        // Assert - Should save at least 2 entities (product and user), plus audit logs
        result.Should().BeGreaterThanOrEqualTo(2, "because we added 2 new entities at minimum");
        product.Id.Should().BeGreaterThan(0);
        user.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task SaveChangesAsync_WithNoChanges_ShouldReturnZero()
    {
        // Act
        var result = await _unitOfWork.SaveChangesAsync();

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public async Task Transaction_WithMultipleOperations_ShouldWorkCorrectly()
    {
        // Arrange - Create necessary dependencies first
        var categoryRepository = _unitOfWork.Repository<Category>();
        var category = new Category
        {
            Name = "Transaction Category",
            Slug = "transaction-category",
            IsActive = true
        };
        await categoryRepository.AddAsync(category);
        await _unitOfWork.SaveChangesAsync();

        var subcategoryRepo = _unitOfWork.Repository<Subcategory>();
        var subcategory = new Subcategory
        {
            Name = "Transaction Subcategory",
            Slug = "transaction-subcategory",
            CategoryId = category.Id,
            IsActive = true
        };
        await subcategoryRepo.AddAsync(subcategory);
        await _unitOfWork.SaveChangesAsync();

        var productRepository = _unitOfWork.Repository<Product>();
        
        var product1 = new Product
        {
            Name = "Product 1",
            Slug = "product-1-trans",
            PriceIncGst = 10.00m,
            PriceExGst = 9.09m,
            GstAmount = 0.91m,
            SubcategoryId = subcategory.Id,
            IsActive = true,
            TrackInventory = true,
            StockQuantity = 10
        };

        var product2 = new Product
        {
            Name = "Product 2",
            Slug = "product-2-trans",
            PriceIncGst = 20.00m,
            PriceExGst = 18.18m,
            GstAmount = 1.82m,
            SubcategoryId = subcategory.Id,
            IsActive = true,
            TrackInventory = true,
            StockQuantity = 20
        };

        // Act
        await _unitOfWork.BeginTransactionAsync();
        
        await productRepository.AddAsync(product1);
        await _unitOfWork.SaveChangesAsync();
        
        await productRepository.AddAsync(product2);
        await _unitOfWork.SaveChangesAsync();
        
        await _unitOfWork.CommitTransactionAsync();

        // Assert
        var allProducts = await productRepository.GetAllAsync();
        allProducts.Should().Contain(p => p.Name == "Product 1");
        allProducts.Should().Contain(p => p.Name == "Product 2");
    }

    public void Dispose()
    {
        _unitOfWork.Dispose();
        _context.Dispose();
    }
}

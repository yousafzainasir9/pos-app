using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using POS.Domain.Entities;
using POS.Infrastructure.Data;
using POS.Infrastructure.Repositories;
using POS.Infrastructure.Tests.Helpers;
using Xunit;

namespace POS.Infrastructure.Tests.Repositories;

/// <summary>
/// Tests for Repository Query operations - Advanced querying, filtering, sorting
/// </summary>
public class RepositoryQueryTests : IDisposable
{
    private readonly POSDbContext _context;
    private readonly Repository<Product> _productRepository;

    public RepositoryQueryTests()
    {
        _context = InMemoryDbContextFactory.CreateWithData();
        _productRepository = new Repository<Product>(_context);
    }

    [Fact]
    public void Query_WithNoFilters_ShouldReturnAllActiveProducts()
    {
        // Act
        var results = _productRepository.Query().ToList();

        // Assert
        results.Should().NotBeNull();
        results.Should().HaveCountGreaterThan(0);
        results.All(p => !p.IsDeleted).Should().BeTrue();
    }

    [Fact]
    public void Query_WithWhereClause_ShouldFilterResults()
    {
        // Act
        var results = _productRepository.Query()
            .Where(p => p.PriceIncGst > 5.00m)
            .ToList();

        // Assert
        results.Should().NotBeNull();
        results.All(p => p.PriceIncGst > 5.00m).Should().BeTrue();
    }

    [Fact]
    public void Query_WithInclude_ShouldLoadNavigationProperties()
    {
        // Act
        var results = _productRepository.Query()
            .Include(p => p.Subcategory)
            .ToList();

        // Assert
        results.Should().NotBeNull();
        results.Should().HaveCountGreaterThan(0);
        results.All(p => p.Subcategory != null).Should().BeTrue();
    }

    [Fact]
    public void Query_WithIncludeAndThenInclude_ShouldLoadNestedNavigationProperties()
    {
        // Act
        var results = _productRepository.Query()
            .Include(p => p.Subcategory)
                .ThenInclude(s => s.Category)
            .ToList();

        // Assert
        results.Should().NotBeNull();
        results.Should().HaveCountGreaterThan(0);
        
        var productsWithCategory = results.Where(p => p.Subcategory?.Category != null).ToList();
        productsWithCategory.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public void Query_WithOrderBy_ShouldSortResults()
    {
        // Act
        var results = _productRepository.Query()
            .OrderBy(p => p.Name)
            .ToList();

        // Assert
        results.Should().NotBeNull();
        results.Should().HaveCountGreaterThan(1);
        
        for (int i = 0; i < results.Count - 1; i++)
        {
            string.Compare(results[i].Name, results[i + 1].Name, StringComparison.Ordinal)
                .Should().BeLessOrEqualTo(0);
        }
    }

    [Fact]
    public void Query_WithOrderByDescending_ShouldSortResultsDescending()
    {
        // Act
        var results = _productRepository.Query()
            .OrderByDescending(p => p.PriceIncGst)
            .ToList();

        // Assert
        results.Should().NotBeNull();
        results.Should().HaveCountGreaterThan(1);
        
        for (int i = 0; i < results.Count - 1; i++)
        {
            results[i].PriceIncGst.Should().BeGreaterOrEqualTo(results[i + 1].PriceIncGst);
        }
    }

    [Fact]
    public void Query_WithSkipAndTake_ShouldPaginateResults()
    {
        // Arrange
        var pageSize = 2;
        var pageNumber = 1; // Second page (0-indexed)

        // Act
        var allResults = _productRepository.Query().OrderBy(p => p.Id).ToList();
        var pagedResults = _productRepository.Query()
            .OrderBy(p => p.Id)
            .Skip(pageNumber * pageSize)
            .Take(pageSize)
            .ToList();

        // Assert
        pagedResults.Should().NotBeNull();
        pagedResults.Should().HaveCount(Math.Min(pageSize, allResults.Count - (pageNumber * pageSize)));
        
        if (allResults.Count > pageNumber * pageSize)
        {
            pagedResults.First().Id.Should().Be(allResults[pageNumber * pageSize].Id);
        }
    }

    [Fact]
    public void Query_WithComplexFilter_ShouldReturnMatchingResults()
    {
        // Act
        var results = _productRepository.Query()
            .Where(p => p.IsActive && 
                        p.PriceIncGst >= 3.00m && 
                        p.PriceIncGst <= 10.00m &&
                        p.StockQuantity > 0)
            .ToList();

        // Assert
        results.Should().NotBeNull();
        results.All(p => p.IsActive).Should().BeTrue();
        results.All(p => p.PriceIncGst >= 3.00m && p.PriceIncGst <= 10.00m).Should().BeTrue();
        results.All(p => p.StockQuantity > 0).Should().BeTrue();
    }

    [Fact]
    public void Query_WithSelect_ShouldProjectResults()
    {
        // Act
        var results = _productRepository.Query()
            .Select(p => new { p.Id, p.Name, p.PriceIncGst })
            .ToList();

        // Assert
        results.Should().NotBeNull();
        results.Should().HaveCountGreaterThan(0);
        results.All(r => !string.IsNullOrEmpty(r.Name)).Should().BeTrue();
    }

    [Fact]
    public void Query_WithCount_ShouldReturnCorrectCount()
    {
        // Act
        var count = _productRepository.Query().Count();
        var activeCount = _productRepository.Query().Count(p => p.IsActive);

        // Assert
        count.Should().BeGreaterThan(0);
        activeCount.Should().BeGreaterThan(0);
        activeCount.Should().BeLessOrEqualTo(count);
    }

    [Fact]
    public async Task Query_WithFirstOrDefaultAsync_ShouldReturnFirstMatch()
    {
        // Act
        var result = await _productRepository.Query()
            .Where(p => p.IsActive)
            .OrderBy(p => p.Id)
            .FirstOrDefaultAsync();

        // Assert
        result.Should().NotBeNull();
        result!.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Query_WithAnyAsync_ShouldReturnTrueIfExists()
    {
        // Act
        var exists = await _productRepository.Query()
            .AnyAsync(p => p.PriceIncGst > 0);

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public void Query_WithGroupBy_ShouldGroupResults()
    {
        // Act
        var groupedResults = _productRepository.Query()
            .GroupBy(p => p.SubcategoryId)
            .Select(g => new
            {
                SubcategoryId = g.Key,
                Count = g.Count(),
                AveragePrice = g.Average(p => p.PriceIncGst)
            })
            .ToList();

        // Assert
        groupedResults.Should().NotBeNull();
        groupedResults.Should().HaveCountGreaterThan(0);
        groupedResults.All(g => g.Count > 0).Should().BeTrue();
    }

    [Fact]
    public void Query_WithMultipleIncludes_ShouldLoadAllNavigationProperties()
    {
        // Act
        var results = _productRepository.Query()
            .Include(p => p.Subcategory)
            .Include(p => p.Supplier)
            .ToList();

        // Assert
        results.Should().NotBeNull();
        results.Should().HaveCountGreaterThan(0);
        results.All(p => p.Subcategory != null).Should().BeTrue();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}

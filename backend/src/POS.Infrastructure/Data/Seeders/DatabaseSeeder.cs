using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using POS.Domain.Entities;
using POS.Domain.Enums;
using System.Text.Json;

namespace POS.Infrastructure.Data.Seeders;

public class DatabaseSeeder
{
    private readonly POSDbContext _context;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(POSDbContext context, ILogger<DatabaseSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        try
        {
            // Seed Store
            if (!await _context.Stores.AnyAsync())
            {
                await SeedStoresAsync();
            }

            // Seed Users
            if (!await _context.Users.AnyAsync())
            {
                await SeedUsersAsync();
            }

            // Seed Categories and Products from JSON
            if (!await _context.Categories.AnyAsync())
            {
                await SeedCatalogAsync();
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Database seeded successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database");
            throw;
        }
    }

    private async Task SeedStoresAsync()
    {
        var store = new Store
        {
            Name = "Cookie Barrel Main",
            Code = "CB001",
            Address = "123 Main Street",
            City = "Sydney",
            State = "NSW",
            PostalCode = "2000",
            Country = "Australia",
            Phone = "+61 2 1234 5678",
            Email = "main@cookiebarrel.com.au",
            TaxNumber = "ABN 12 345 678 901",
            TaxRate = 0.10m,
            Currency = "AUD",
            IsActive = true,
            OpeningTime = new TimeOnly(7, 0),
            ClosingTime = new TimeOnly(18, 0)
        };

        await _context.Stores.AddAsync(store);
    }

    private async Task SeedUsersAsync()
    {
        var store = await _context.Stores.FirstAsync();

        var users = new List<User>
        {
            new User
            {
                Username = "admin",
                Email = "admin@cookiebarrel.com.au",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                FirstName = "System",
                LastName = "Administrator",
                Role = UserRole.Admin,
                IsActive = true,
                StoreId = store.Id,
                Pin = "9999"
            },
            new User
            {
                Username = "manager",
                Email = "manager@cookiebarrel.com.au",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Manager123!"),
                FirstName = "Store",
                LastName = "Manager",
                Role = UserRole.Manager,
                IsActive = true,
                StoreId = store.Id,
                Pin = "1234"
            },
            new User
            {
                Username = "cashier1",
                Email = "cashier1@cookiebarrel.com.au",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Cashier123!"),
                FirstName = "John",
                LastName = "Doe",
                Role = UserRole.Cashier,
                IsActive = true,
                StoreId = store.Id,
                Pin = "1111"
            }
        };

        await _context.Users.AddRangeAsync(users);
    }

    private async Task SeedCatalogAsync()
    {
        var jsonPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "documentation", "cookie_barrel_catalog_devseed_2025-09-19.json");
        
        if (!File.Exists(jsonPath))
        {
            _logger.LogWarning("Catalog JSON file not found at {Path}", jsonPath);
            return;
        }

        var jsonContent = await File.ReadAllTextAsync(jsonPath);
        var catalog = JsonSerializer.Deserialize<CatalogData>(jsonContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (catalog?.Categories == null)
        {
            _logger.LogWarning("No categories found in catalog JSON");
            return;
        }

        int categoryOrder = 0;
        foreach (var categoryData in catalog.Categories)
        {
            var category = new Category
            {
                Name = categoryData.Name,
                Slug = categoryData.Slug,
                DisplayOrder = ++categoryOrder,
                IsActive = true
            };

            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync(); // Save to get the ID

            if (categoryData.Subcategories != null)
            {
                int subcategoryOrder = 0;
                foreach (var subcategoryData in categoryData.Subcategories)
                {
                    var subcategory = new Subcategory
                    {
                        Name = subcategoryData.Name,
                        Slug = subcategoryData.Slug,
                        CategoryId = category.Id,
                        DisplayOrder = ++subcategoryOrder,
                        IsActive = true
                    };

                    await _context.Subcategories.AddAsync(subcategory);
                    await _context.SaveChangesAsync(); // Save to get the ID

                    if (subcategoryData.Products != null)
                    {
                        int productOrder = 0;
                        foreach (var productData in subcategoryData.Products)
                        {
                            var priceExGst = productData.PriceExGstAud ?? 0;
                            var gstAmount = Math.Round(priceExGst * catalog.Gst.Rate, 2);
                            var priceIncGst = priceExGst + gstAmount;

                            var product = new Product
                            {
                                Name = productData.Name,
                                Slug = productData.Slug,
                                PriceExGst = priceExGst,
                                GstAmount = gstAmount,
                                PriceIncGst = priceIncGst,
                                PackNotes = productData.PackNotes,
                                SubcategoryId = subcategory.Id,
                                IsActive = true,
                                TrackInventory = true,
                                StockQuantity = 100, // Default stock
                                LowStockThreshold = 10,
                                DisplayOrder = ++productOrder
                            };

                            // Extract pack size from name if possible
                            if (product.Name.Contains("pack of"))
                            {
                                var packMatch = System.Text.RegularExpressions.Regex.Match(product.Name, @"pack of (\d+)");
                                if (packMatch.Success && int.TryParse(packMatch.Groups[1].Value, out int packSize))
                                {
                                    product.PackSize = packSize;
                                }
                            }

                            // Generate SKU
                            product.SKU = $"{category.Slug.ToUpper()[..Math.Min(3, category.Slug.Length)]}-{subcategory.Slug.ToUpper()[..Math.Min(3, subcategory.Slug.Length)]}-{productOrder:000}";

                            await _context.Products.AddAsync(product);
                        }
                    }
                }
            }
        }
    }

    // Classes for deserializing the JSON catalog
    private class CatalogData
    {
        public string Vendor { get; set; } = string.Empty;
        public string Currency { get; set; } = string.Empty;
        public GstData Gst { get; set; } = new();
        public List<CategoryData> Categories { get; set; } = new();
    }

    private class GstData
    {
        public decimal Rate { get; set; }
        public bool ExclusivePricing { get; set; }
    }

    private class CategoryData
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public List<SubcategoryData> Subcategories { get; set; } = new();
    }

    private class SubcategoryData
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public List<ProductData> Products { get; set; } = new();
    }

    private class ProductData
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public decimal? PriceExGstAud { get; set; }
        public string? PackNotes { get; set; }
        public string? SourceUrl { get; set; }
    }
}

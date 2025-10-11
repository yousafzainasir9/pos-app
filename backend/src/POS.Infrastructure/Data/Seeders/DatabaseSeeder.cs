using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using POS.Domain.Entities;
using POS.Domain.Enums;
using OfficeOpenXml;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Globalization;

namespace POS.Infrastructure.Data.Seeders;

public class DatabaseSeeder
{
    private readonly POSDbContext _context;
    private readonly ILogger<DatabaseSeeder> _logger;
    private readonly string _excelDataPath;
    private readonly string _jsonDataPath;
    private readonly Random _random = new Random();
    
    // Image URL mappings for different product types
    private readonly Dictionary<string, List<string>> _imageUrlsByCategory = new()
    {
        ["cookies"] = new List<string>
        {
            "https://images.unsplash.com/photo-1499636136210-6f4ee915583e?w=500&h=500&fit=crop",
            "https://images.unsplash.com/photo-1558961363-fa8fdf82db35?w=500&h=500&fit=crop",
            "https://images.unsplash.com/photo-1590080876876-5ae8d2b2cba8?w=500&h=500&fit=crop",
            "https://images.unsplash.com/photo-1486427944299-d1955d23e34d?w=500&h=500&fit=crop",
            "https://images.unsplash.com/photo-1581899326674-1a3f39c4f1d7?w=500&h=500&fit=crop"
        },
        ["breads"] = new List<string>
        {
            "https://images.unsplash.com/photo-1509440159596-0249088772ff?w=500&h=500&fit=crop",
            "https://images.unsplash.com/photo-1555507036-ab1f4038808a?w=500&h=500&fit=crop",
            "https://images.unsplash.com/photo-1549931319-a545dcf3bc73?w=500&h=500&fit=crop",
            "https://images.unsplash.com/photo-1598373182133-52452f7691ef?w=500&h=500&fit=crop",
            "https://images.unsplash.com/photo-1608198399988-427d83c2b152?w=500&h=500&fit=crop"
        },
        ["cakes"] = new List<string>
        {
            "https://images.unsplash.com/photo-1578985545062-69928b1d9587?w=500&h=500&fit=crop",
            "https://images.unsplash.com/photo-1565958011703-44f9829ba187?w=500&h=500&fit=crop",
            "https://images.unsplash.com/photo-1621303837174-89787a7d4729?w=500&h=500&fit=crop",
            "https://images.unsplash.com/photo-1464349095431-e9a21285b5f3?w=500&h=500&fit=crop",
            "https://images.unsplash.com/photo-1606313564200-e75d5e30476c?w=500&h=500&fit=crop"
        },
        ["pastries"] = new List<string>
        {
            "https://images.unsplash.com/photo-1555507036-ab1f4038808a?w=500&h=500&fit=crop",
            "https://images.unsplash.com/photo-1608198399988-427d83c2b152?w=500&h=500&fit=crop",
            "https://images.unsplash.com/photo-1530610476181-d83430b64dcd?w=500&h=500&fit=crop",
            "https://images.unsplash.com/photo-1509440159596-0249088772ff?w=500&h=500&fit=crop",
            "https://images.unsplash.com/photo-1586985289688-ca3cf47d3e6e?w=500&h=500&fit=crop"
        },
        ["beverages"] = new List<string>
        {
            "https://images.unsplash.com/photo-1495474472287-4d71bcdd2085?w=500&h=500&fit=crop",
            "https://images.unsplash.com/photo-1559056199-641a0ac8b55e?w=500&h=500&fit=crop",
            "https://images.unsplash.com/photo-1556679343-c7306c1976bc?w=500&h=500&fit=crop",
            "https://images.unsplash.com/photo-1544787219-7f47ccb76574?w=500&h=500&fit=crop",
            "https://images.unsplash.com/photo-1509042239860-f550ce710b93?w=500&h=500&fit=crop"
        },
        ["default"] = new List<string>
        {
            "https://images.unsplash.com/photo-1555507036-ab1f4038808a?w=500&h=500&fit=crop",
            "https://images.unsplash.com/photo-1509440159596-0249088772ff?w=500&h=500&fit=crop",
            "https://images.unsplash.com/photo-1499636136210-6f4ee915583e?w=500&h=500&fit=crop",
            "https://images.unsplash.com/photo-1578985545062-69928b1d9587?w=500&h=500&fit=crop",
            "https://images.unsplash.com/photo-1495474472287-4d71bcdd2085?w=500&h=500&fit=crop"
        }
    };

    public DatabaseSeeder(POSDbContext context, ILogger<DatabaseSeeder> logger)
    {
        _context = context;
        _logger = logger;
        
        // Set EPPlus license context (required for EPPlus 5+)
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        
        // Use relative paths from the running application
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        
        // Navigate up to find the backend folder
        var currentDir = new DirectoryInfo(baseDirectory);
        while (currentDir != null && !currentDir.Name.Equals("backend", StringComparison.OrdinalIgnoreCase))
        {
            currentDir = currentDir.Parent;
        }
        
        if (currentDir != null)
        {
            _excelDataPath = Path.Combine(currentDir.FullName, "data", "excel");
            _jsonDataPath = Path.Combine(currentDir.Parent!.FullName, "documentation");
        }
        else
        {
            _excelDataPath = Path.GetFullPath(Path.Combine(baseDirectory, "..", "..", "..", "..", "data", "excel"));
            _jsonDataPath = Path.GetFullPath(Path.Combine(baseDirectory, "..", "..", "..", "..", "..", "documentation"));
        }
        
        // Create directories if they don't exist
        if (!Directory.Exists(_excelDataPath))
        {
            Directory.CreateDirectory(_excelDataPath);
        }
    }

    public async Task SeedAsync(bool forceRefresh = false)
    {
        try
        {
            // If not forcing refresh, check if data already exists
            if (!forceRefresh && await _context.Stores.AnyAsync())
            {
                _logger.LogInformation("Database already contains data. Skipping seed.");
                return;
            }
            
            if (forceRefresh)
            {
                _logger.LogInformation("Force refresh enabled - Seeding comprehensive data...");
            }

            // Seed all data in proper order (dependencies first)
            await SeedStoresAsync();
            await SeedSuppliersAsync();
            await SeedUsersAsync();
            await SeedCustomersAsync();
            await SeedCustomerUsersAsync(); // Create user accounts for customers
            await SeedCategoriesAndProductsAsync();
            await SeedShiftsAsync();
            await SeedOrdersAsync();
            await SeedPaymentsAsync();
            await SeedInventoryTransactionsAsync();
            
            _logger.LogInformation("Database seeding completed successfully with all tables populated");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database");
            throw;
        }
    }

    private async Task SeedStoresAsync()
    {
        if (await _context.Stores.AnyAsync()) return;

        var stores = new List<Store>
        {
            new Store
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
            },
            new Store
            {
                Name = "Cookie Barrel Westfield",
                Code = "CB002",
                Address = "Shop 45, Westfield Shopping Centre",
                City = "Sydney",
                State = "NSW",
                PostalCode = "2150",
                Country = "Australia",
                Phone = "+61 2 9876 5432",
                Email = "westfield@cookiebarrel.com.au",
                TaxNumber = "ABN 12 345 678 901",
                TaxRate = 0.10m,
                Currency = "AUD",
                IsActive = true,
                OpeningTime = new TimeOnly(9, 0),
                ClosingTime = new TimeOnly(21, 0)
            },
            new Store
            {
                Name = "Cookie Barrel Airport",
                Code = "CB003",
                Address = "Terminal 2, Sydney Airport",
                City = "Sydney",
                State = "NSW",
                PostalCode = "2020",
                Country = "Australia",
                Phone = "+61 2 9667 9000",
                Email = "airport@cookiebarrel.com.au",
                TaxNumber = "ABN 12 345 678 901",
                TaxRate = 0.10m,
                Currency = "AUD",
                IsActive = true,
                OpeningTime = new TimeOnly(5, 0),
                ClosingTime = new TimeOnly(23, 0)
            }
        };

        await _context.Stores.AddRangeAsync(stores);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Seeded {Count} stores", stores.Count);
    }

    private async Task SeedSuppliersAsync()
    {
        if (await _context.Suppliers.AnyAsync()) return;

        var suppliers = new List<Supplier>
        {
            new Supplier
            {
                Name = "Premium Bakery Supplies",
                ContactPerson = "Michael Brown",
                Email = "orders@premiumbakery.com.au",
                Phone = "+61 2 5555 0100",
                Address = "45 Industrial Way",
                City = "Sydney",
                State = "NSW",
                PostalCode = "2140",
                Country = "Australia",
                TaxNumber = "ABN 98 765 432 109",
                Notes = "Main supplier for baking ingredients",
                IsActive = true
            },
            new Supplier
            {
                Name = "Fresh Dairy Direct",
                ContactPerson = "Emma Wilson",
                Email = "sales@freshdairy.com.au",
                Phone = "+61 2 5555 0200",
                Address = "12 Farm Road",
                City = "Melbourne",
                State = "VIC",
                PostalCode = "3000",
                Country = "Australia",
                TaxNumber = "ABN 87 654 321 098",
                Notes = "Dairy products supplier",
                IsActive = true
            },
            new Supplier
            {
                Name = "Packaging Solutions Co",
                ContactPerson = "David Lee",
                Email = "info@packagingsolutions.com.au",
                Phone = "+61 2 5555 0300",
                Address = "78 Box Street",
                City = "Brisbane",
                State = "QLD",
                PostalCode = "4000",
                Country = "Australia",
                TaxNumber = "ABN 76 543 210 987",
                Notes = "Boxes and packaging materials",
                IsActive = true
            },
            new Supplier
            {
                Name = "Organic Ingredients Ltd",
                ContactPerson = "Sarah Green",
                Email = "orders@organicingredients.com.au",
                Phone = "+61 2 5555 0400",
                Address = "90 Nature Lane",
                City = "Adelaide",
                State = "SA",
                PostalCode = "5000",
                Country = "Australia",
                TaxNumber = "ABN 65 432 109 876",
                Notes = "Organic and specialty ingredients",
                IsActive = true
            }
        };

        await _context.Suppliers.AddRangeAsync(suppliers);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Seeded {Count} suppliers", suppliers.Count);
    }

    private async Task SeedUsersAsync()
    {
        if (await _context.Users.AnyAsync()) return;

        var stores = await _context.Stores.ToListAsync();
        var users = new List<User>();

        // Admin user
        users.Add(new User
        {
            Username = "admin",
            Email = "admin@cookiebarrel.com.au",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
            FirstName = "System",
            LastName = "Administrator",
            Role = UserRole.Admin,
            IsActive = true,
            StoreId = stores[0].Id,
            Pin = "9999",
            Phone = "+61 400 000 001"
        });

        // Create users for each store
        int userCounter = 1;
        foreach (var store in stores)
        {
            // Manager
            users.Add(new User
            {
                Username = $"manager{userCounter}",
                Email = $"manager{userCounter}@cookiebarrel.com.au",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Manager123!"),
                FirstName = GetFirstName(userCounter),
                LastName = GetLastName(userCounter),
                Role = UserRole.Manager,
                IsActive = true,
                StoreId = store.Id,
                Pin = $"{1000 + userCounter:D4}",
                Phone = $"+61 400 100 {userCounter:D3}"
            });

            // 3 Cashiers per store
            for (int i = 0; i < 3; i++)
            {
                userCounter++;
                users.Add(new User
                {
                    Username = $"cashier{userCounter}",
                    Email = $"cashier{userCounter}@cookiebarrel.com.au",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Cashier123!"),
                    FirstName = GetFirstName(userCounter + 10),
                    LastName = GetLastName(userCounter + 10),
                    Role = UserRole.Cashier,
                    IsActive = true,
                    StoreId = store.Id,
                    Pin = $"{2000 + userCounter:D4}",
                    Phone = $"+61 400 200 {userCounter:D3}"
                });
            }
            userCounter++;
        }

        await _context.Users.AddRangeAsync(users);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Seeded {Count} users", users.Count);
    }

    private async Task SeedCustomersAsync()
    {
        if (await _context.Customers.AnyAsync()) return;

        var customers = new List<Customer>();
        
        // Walk-in customer
        customers.Add(new Customer
        {
            FirstName = "Walk-in",
            LastName = "Customer",
            Email = "walkin@system.local",
            IsActive = true,
            Notes = "Default customer for walk-in sales"
        });

        // Generate 100 customers with realistic data
        string[] firstNames = { "John", "Jane", "Michael", "Sarah", "David", "Emma", "James", "Lisa", "Robert", "Mary",
                               "William", "Patricia", "Richard", "Jennifer", "Thomas", "Linda", "Charles", "Elizabeth" };
        string[] lastNames = { "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis",
                              "Rodriguez", "Martinez", "Anderson", "Taylor", "Wilson", "Moore", "Jackson" };

        for (int i = 1; i <= 100; i++)
        {
            var firstName = firstNames[_random.Next(firstNames.Length)];
            var lastName = lastNames[_random.Next(lastNames.Length)];
            
            customers.Add(new Customer
            {
                FirstName = firstName,
                LastName = lastName,
                Email = $"{firstName.ToLower()}.{lastName.ToLower()}{i}@example.com",
                Phone = $"+61 4{_random.Next(10, 99)} {_random.Next(100, 999)} {_random.Next(100, 999)}",
                Address = $"{_random.Next(1, 999)} {GetStreetName()} {GetStreetType()}",
                City = GetCity(i % 5),
                State = GetState(i % 5),
                PostalCode = $"{2000 + (i % 8) * 100}",
                Country = "Australia",
                IsActive = true,
                LoyaltyCardNumber = $"LC{1000000 + i:D7}",
                LoyaltyPoints = _random.Next(0, 1000),
                TotalPurchases = _random.Next(50, 10000),
                TotalOrders = _random.Next(1, 200),
                LastOrderDate = DateTime.UtcNow.AddDays(-_random.Next(1, 90)),
                DateOfBirth = i % 3 == 0 ? new DateTime(1950 + _random.Next(0, 50), _random.Next(1, 12), _random.Next(1, 28)) : null,
                Notes = i % 10 == 0 ? "VIP customer - provide special offers" : null
            });
        }

        // Corporate customers
        customers.Add(new Customer
        {
            FirstName = "Corporate",
            LastName = "Acme Corp",
            Email = "orders@acmecorp.com",
            Phone = "+61 2 9000 1000",
            Address = "Level 10, 100 Corporate Tower",
            City = "Sydney",
            State = "NSW",
            PostalCode = "2000",
            Country = "Australia",
            IsActive = true,
            LoyaltyCardNumber = "LC9000001",
            LoyaltyPoints = 5000,
            TotalPurchases = 50000,
            TotalOrders = 500,
            Notes = "Corporate account - NET 30 terms"
        });

        await _context.Customers.AddRangeAsync(customers);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Seeded {Count} customers", customers.Count);
    }

    private async Task SeedCategoriesAndProductsAsync()
    {
        if (await _context.Products.AnyAsync()) return;

        // Try to load from JSON first
        var jsonFile = Path.Combine(_jsonDataPath, "cookie_barrel_catalog_devseed_2025-09-19.json");
        
        if (File.Exists(jsonFile))
        {
            await SeedProductsFromJsonAsync(jsonFile);
        }
        else
        {
            await SeedDefaultProductsAsync();
        }
    }

    private async Task SeedProductsFromJsonAsync(string jsonFile)
    {
        var jsonContent = await File.ReadAllTextAsync(jsonFile);
        var catalog = JsonSerializer.Deserialize<CatalogData>(jsonContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (catalog?.Categories == null) return;

        var suppliers = await _context.Suppliers.ToListAsync();
        int totalProducts = 0;

        foreach (var categoryData in catalog.Categories)
        {
            var category = new Category
            {
                Name = categoryData.Name,
                Slug = categoryData.Slug,
                Description = $"Cookie Barrel {categoryData.Name}",
                IsActive = true,
                DisplayOrder = await _context.Categories.CountAsync() + 1
            };

            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();

            if (categoryData.Subcategories != null)
            {
                foreach (var subcategoryData in categoryData.Subcategories)
                {
                    var subcategory = new Subcategory
                    {
                        Name = subcategoryData.Name,
                        Slug = subcategoryData.Slug,
                        Description = $"Selection of {subcategoryData.Name}",
                        CategoryId = category.Id,
                        IsActive = true,
                        DisplayOrder = await _context.Subcategories.CountAsync(s => s.CategoryId == category.Id) + 1
                    };

                    await _context.Subcategories.AddAsync(subcategory);
                    await _context.SaveChangesAsync();

                    if (subcategoryData.Products != null)
                    {
                        foreach (var productData in subcategoryData.Products)
                        {
                            var priceExGst = productData.PriceExGstAud ?? _random.Next(5, 50);
                            var gstAmount = Math.Round(priceExGst * (catalog.Gst?.Rate ?? 0.10m), 2);
                            var supplier = suppliers[_random.Next(suppliers.Count)];

                            var product = new Product
                            {
                                Name = productData.Name,
                                Slug = productData.Slug,
                                SKU = $"{category.Slug.ToUpper()[..3]}-{subcategory.Slug.ToUpper()[..3]}-{++totalProducts:D4}",
                                Barcode = $"200{totalProducts:D10}",
                                Description = $"Delicious {productData.Name}",
                                PriceExGst = priceExGst,
                                GstAmount = gstAmount,
                                PriceIncGst = priceExGst + gstAmount,
                                Cost = Math.Round(priceExGst * 0.6m, 2),
                                PackNotes = productData.PackNotes,
                                SubcategoryId = subcategory.Id,
                                SupplierId = supplier.Id,
                                IsActive = true,
                                TrackInventory = true,
                                StockQuantity = GetRandomStockQuantity(),
                                LowStockThreshold = _random.Next(10, 30),
                                DisplayOrder = totalProducts,
                                ImageUrl = GetImageUrlForProduct(category.Slug, subcategory.Slug, productData.Name)
                            };

                            // Extract pack size
                            var packMatch = Regex.Match(product.Name, @"pack of (\d+)", RegexOptions.IgnoreCase);
                            if (packMatch.Success && int.TryParse(packMatch.Groups[1].Value, out int packSize))
                            {
                                product.PackSize = packSize;
                            }

                            await _context.Products.AddAsync(product);
                        }
                    }
                }
            }
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("Seeded {Count} products from JSON catalog", totalProducts);
    }

    private async Task SeedDefaultProductsAsync()
    {
        var suppliers = await _context.Suppliers.ToListAsync();
        
        // Create sample categories and products
        var categories = new[]
        {
            ("Breads", "breads", new[] { "Croissants", "Baguettes", "Sourdough" }),
            ("Cakes", "cakes", new[] { "Chocolate", "Vanilla", "Fruit" }),
            ("Cookies", "cookies", new[] { "Chocolate Chip", "Oatmeal", "Sugar" }),
            ("Beverages", "beverages", new[] { "Coffee", "Tea", "Juice" })
        };

        int productId = 0;
        foreach (var (catName, catSlug, subcats) in categories)
        {
            var category = new Category
            {
                Name = catName,
                Slug = catSlug,
                Description = $"{catName} products",
                DisplayOrder = await _context.Categories.CountAsync() + 1,
                IsActive = true
            };
            
            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();

            foreach (var subcatName in subcats)
            {
                var subcategory = new Subcategory
                {
                    Name = subcatName,
                    Slug = subcatName.ToLower().Replace(" ", "-"),
                    CategoryId = category.Id,
                    DisplayOrder = await _context.Subcategories.CountAsync(s => s.CategoryId == category.Id) + 1,
                    IsActive = true
                };
                
                await _context.Subcategories.AddAsync(subcategory);
                await _context.SaveChangesAsync();

                // Add 5 products per subcategory
                for (int i = 1; i <= 5; i++)
                {
                    productId++;
                    var price = _random.Next(5, 50);
                    var product = new Product
                    {
                        Name = $"{subcatName} Product {i}",
                        Slug = $"{subcatName.ToLower().Replace(" ", "-")}-{i}",
                        SKU = $"{catSlug.ToUpper()[..3]}-{productId:D4}",
                        Barcode = $"200{productId:D10}",
                        Description = $"Fresh {subcatName} product",
                        PriceExGst = price,
                        GstAmount = Math.Round(price * 0.1m, 2),
                        PriceIncGst = Math.Round(price * 1.1m, 2),
                        Cost = Math.Round(price * 0.6m, 2),
                        StockQuantity = GetRandomStockQuantity(),
                        LowStockThreshold = 20,
                        SubcategoryId = subcategory.Id,
                        SupplierId = suppliers[_random.Next(suppliers.Count)].Id,
                        IsActive = true,
                        TrackInventory = true,
                        DisplayOrder = i,
                        ImageUrl = GetImageUrlForProduct(catSlug, subcategory.Slug, subcatName)
                    };
                    
                    await _context.Products.AddAsync(product);
                }
            }
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("Seeded default products");
    }

    private async Task SeedShiftsAsync()
    {
        if (await _context.Shifts.AnyAsync()) return;

        var stores = await _context.Stores.ToListAsync();
        var cashiers = await _context.Users.Where(u => u.Role == UserRole.Cashier).ToListAsync();
        var managers = await _context.Users.Where(u => u.Role == UserRole.Manager).ToListAsync();

        var shifts = new List<Shift>();

        // Create shifts for the past 30 days
        for (int daysAgo = 30; daysAgo >= 0; daysAgo--)
        {
            var shiftDate = DateTime.UtcNow.AddDays(-daysAgo).Date;
            
            foreach (var store in stores)
            {
                var storeCashiers = cashiers.Where(c => c.StoreId == store.Id).ToList();
                var storeManager = managers.FirstOrDefault(m => m.StoreId == store.Id);
                
                if (!storeCashiers.Any()) continue;

                // Morning shift (7am - 2pm)
                var morningShift = new Shift
                {
                    ShiftNumber = $"SHIFT-{store.Code}-{shiftDate:yyyyMMdd}-AM",
                    StartTime = shiftDate.AddHours(7),
                    EndTime = shiftDate.AddHours(14),
                    StartingCash = 500m,
                    EndingCash = _random.Next(800, 1500),
                    CashSales = _random.Next(500, 1500),
                    CardSales = _random.Next(1000, 3000),
                    OtherSales = _random.Next(50, 200),
                    Status = ShiftStatus.Closed,
                    UserId = storeCashiers[_random.Next(storeCashiers.Count)].Id,
                    StoreId = store.Id,
                    ClosedByUserId = storeManager?.Id ?? storeCashiers[0].Id
                };
                morningShift.TotalSales = morningShift.CashSales + morningShift.CardSales + morningShift.OtherSales;
                morningShift.TotalOrders = _random.Next(50, 150);
                shifts.Add(morningShift);

                // Afternoon shift (2pm - 9pm)
                var afternoonShift = new Shift
                {
                    ShiftNumber = $"SHIFT-{store.Code}-{shiftDate:yyyyMMdd}-PM",
                    StartTime = shiftDate.AddHours(14),
                    EndTime = daysAgo == 0 ? null : shiftDate.AddHours(21), // Current day shift might be open
                    StartingCash = 500m,
                    EndingCash = daysAgo == 0 ? null : _random.Next(800, 1500),
                    CashSales = daysAgo == 0 ? _random.Next(200, 500) : _random.Next(500, 1500),
                    CardSales = daysAgo == 0 ? _random.Next(300, 1000) : _random.Next(1000, 3000),
                    OtherSales = daysAgo == 0 ? _random.Next(20, 100) : _random.Next(50, 200),
                    Status = daysAgo == 0 ? ShiftStatus.Open : ShiftStatus.Closed,
                    UserId = storeCashiers[_random.Next(storeCashiers.Count)].Id,
                    StoreId = store.Id,
                    ClosedByUserId = daysAgo == 0 ? null : (storeManager?.Id ?? storeCashiers[0].Id)
                };
                
                if (daysAgo > 0)
                {
                    afternoonShift.TotalSales = afternoonShift.CashSales + afternoonShift.CardSales + afternoonShift.OtherSales;
                    afternoonShift.TotalOrders = _random.Next(50, 150);
                }
                
                shifts.Add(afternoonShift);
            }
        }

        await _context.Shifts.AddRangeAsync(shifts);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Seeded {Count} shifts", shifts.Count);
    }

    private async Task SeedOrdersAsync()
    {
        if (await _context.Orders.AnyAsync()) return;

        var stores = await _context.Stores.ToListAsync();
        var users = await _context.Users.Where(u => u.Role == UserRole.Cashier).ToListAsync();
        var customers = await _context.Customers.ToListAsync();
        var products = await _context.Products.Where(p => p.IsActive).ToListAsync();
        var shifts = await _context.Shifts.ToListAsync();

        var walkInCustomer = customers.FirstOrDefault(c => c.FirstName == "Walk-in");
        var regularCustomers = customers.Where(c => c.FirstName != "Walk-in").ToList();

        var orders = new List<Order>();
        var orderItems = new List<OrderItem>();
        int orderNumber = 1000;

        // Generate orders for each shift
        foreach (var shift in shifts.Where(s => s.Status == ShiftStatus.Closed))
        {
            var shiftOrders = shift.TotalOrders ?? _random.Next(20, 100);
            var shiftUser = await _context.Users.FindAsync(shift.UserId);
            
            for (int i = 0; i < shiftOrders; i++)
            {
                var orderDate = shift.StartTime.AddMinutes(_random.Next(0, (int)(shift.EndTime!.Value - shift.StartTime).TotalMinutes));
                var customer = _random.Next(100) < 60 ? walkInCustomer : regularCustomers[_random.Next(regularCustomers.Count)];

                var order = new Order
                {
                    OrderNumber = $"ORD{orderNumber++:D6}",
                    OrderDate = orderDate,
                    Status = OrderStatus.Completed,
                    OrderType = _random.Next(100) < 90 ? OrderType.TakeAway : OrderType.DineIn,
                    CustomerId = customer?.Id,
                    StoreId = shift.StoreId,
                    UserId = shift.UserId,
                    ShiftId = shift.Id,
                    CompletedAt = orderDate.AddMinutes(_random.Next(1, 5)),
                    SubTotal = 0,
                    TaxAmount = 0,
                    TotalAmount = 0,
                    DiscountAmount = _random.Next(100) < 10 ? _random.Next(5, 20) : 0
                };

                // Add order items
                int itemCount = _random.Next(1, 8);
                decimal orderSubtotal = 0;
                decimal orderTax = 0;

                for (int j = 0; j < itemCount; j++)
                {
                    var product = products[_random.Next(products.Count)];
                    var quantity = _random.Next(1, 5);

                    var item = new OrderItem
                    {
                        Order = order,
                        ProductId = product.Id,
                        Quantity = quantity,
                        UnitPriceExGst = product.PriceExGst,
                        UnitGstAmount = product.GstAmount,
                        UnitPriceIncGst = product.PriceIncGst,
                        SubTotal = product.PriceExGst * quantity,
                        TaxAmount = product.GstAmount * quantity,
                        TotalAmount = product.PriceIncGst * quantity,
                        DiscountAmount = 0,
                        IsVoided = false
                    };

                    orderSubtotal += item.SubTotal;
                    orderTax += item.TaxAmount;
                    orderItems.Add(item);
                }

                order.SubTotal = orderSubtotal;
                order.TaxAmount = orderTax;
                order.TotalAmount = orderSubtotal + orderTax - order.DiscountAmount;
                order.PaidAmount = order.TotalAmount;
                order.ChangeAmount = 0;

                orders.Add(order);
            }
        }

        // Add some orders for today's open shift
        var openShift = shifts.FirstOrDefault(s => s.Status == ShiftStatus.Open);
        if (openShift != null)
        {
            for (int i = 0; i < 10; i++)
            {
                var orderDate = DateTime.UtcNow.AddHours(-_random.Next(1, 5));
                var customer = _random.Next(100) < 60 ? walkInCustomer : regularCustomers[_random.Next(regularCustomers.Count)];

                var order = new Order
                {
                    OrderNumber = $"ORD{orderNumber++:D6}",
                    OrderDate = orderDate,
                    Status = _random.Next(100) < 80 ? OrderStatus.Completed : OrderStatus.Pending,
                    OrderType = OrderType.TakeAway,
                    CustomerId = customer?.Id,
                    StoreId = openShift.StoreId,
                    UserId = openShift.UserId,
                    ShiftId = openShift.Id,
                    CompletedAt = orderDate.AddMinutes(2),
                    SubTotal = 0,
                    TaxAmount = 0,
                    TotalAmount = 0,
                    DiscountAmount = 0
                };

                // Add items
                int itemCount = _random.Next(1, 5);
                decimal orderSubtotal = 0;
                decimal orderTax = 0;

                for (int j = 0; j < itemCount; j++)
                {
                    var product = products[_random.Next(products.Count)];
                    var quantity = _random.Next(1, 3);

                    var item = new OrderItem
                    {
                        Order = order,
                        ProductId = product.Id,
                        Quantity = quantity,
                        UnitPriceExGst = product.PriceExGst,
                        UnitGstAmount = product.GstAmount,
                        UnitPriceIncGst = product.PriceIncGst,
                        SubTotal = product.PriceExGst * quantity,
                        TaxAmount = product.GstAmount * quantity,
                        TotalAmount = product.PriceIncGst * quantity,
                        DiscountAmount = 0,
                        IsVoided = false
                    };

                    orderSubtotal += item.SubTotal;
                    orderTax += item.TaxAmount;
                    orderItems.Add(item);
                }

                order.SubTotal = orderSubtotal;
                order.TaxAmount = orderTax;
                order.TotalAmount = orderSubtotal + orderTax;
                order.PaidAmount = order.Status == OrderStatus.Completed ? order.TotalAmount : 0;

                orders.Add(order);
            }
        }

        await _context.Orders.AddRangeAsync(orders);
        await _context.OrderItems.AddRangeAsync(orderItems);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Seeded {OrderCount} orders with {ItemCount} items", orders.Count, orderItems.Count);
    }

    private async Task SeedPaymentsAsync()
    {
        if (await _context.Payments.AnyAsync()) return;

        var completedOrders = await _context.Orders
            .Where(o => o.Status == OrderStatus.Completed)
            .ToListAsync();

        var payments = new List<Payment>();

        foreach (var order in completedOrders)
        {
            // Some orders might have split payments
            if (_random.Next(100) < 10 && order.TotalAmount > 50) // 10% chance of split payment
            {
                var cashAmount = Math.Round(order.TotalAmount * 0.5m, 2);
                var cardAmount = order.TotalAmount - cashAmount;

                payments.Add(new Payment
                {
                    OrderId = order.Id,
                    Amount = cashAmount,
                    PaymentMethod = PaymentMethod.Cash,
                    Status = PaymentStatus.Completed,
                    PaymentDate = order.CompletedAt ?? order.OrderDate,
                    ProcessedByUserId = order.UserId
                });

                payments.Add(new Payment
                {
                    OrderId = order.Id,
                    Amount = cardAmount,
                    PaymentMethod = PaymentMethod.CreditCard,
                    Status = PaymentStatus.Completed,
                    PaymentDate = order.CompletedAt ?? order.OrderDate,
                    ProcessedByUserId = order.UserId,
                    CardLastFourDigits = _random.Next(1000, 9999).ToString(),
                    CardType = GetRandomCardType(),
                    ReferenceNumber = $"REF{_random.Next(100000, 999999)}"
                });
            }
            else
            {
                // Single payment
                var paymentMethod = _random.Next(100) < 40 ? PaymentMethod.Cash : PaymentMethod.CreditCard;
                
                var payment = new Payment
                {
                    OrderId = order.Id,
                    Amount = order.TotalAmount,
                    PaymentMethod = paymentMethod,
                    Status = PaymentStatus.Completed,
                    PaymentDate = order.CompletedAt ?? order.OrderDate,
                    ProcessedByUserId = order.UserId
                };

                if (paymentMethod == PaymentMethod.CreditCard)
                {
                    payment.CardLastFourDigits = _random.Next(1000, 9999).ToString();
                    payment.CardType = GetRandomCardType();
                    payment.ReferenceNumber = $"REF{_random.Next(100000, 999999)}";
                }

                payments.Add(payment);
            }
        }

        await _context.Payments.AddRangeAsync(payments);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Seeded {Count} payments", payments.Count);
    }

    private async Task SeedInventoryTransactionsAsync()
    {
        if (await _context.InventoryTransactions.AnyAsync()) return;

        var products = await _context.Products.Where(p => p.TrackInventory).ToListAsync();
        var stores = await _context.Stores.ToListAsync();
        var suppliers = await _context.Suppliers.ToListAsync();
        var managers = await _context.Users.Where(u => u.Role == UserRole.Manager).ToListAsync();
        var transactions = new List<InventoryTransaction>();

        foreach (var product in products)
        {
            foreach (var store in stores)
            {
                var manager = managers.FirstOrDefault(m => m.StoreId == store.Id) ?? managers.First();
                var supplier = suppliers.FirstOrDefault(s => s.Id == product.SupplierId) ?? suppliers.First();

                // Initial stock receipt (30 days ago)
                var initialStock = product.StockQuantity + _random.Next(50, 150);
                transactions.Add(new InventoryTransaction
                {
                    ProductId = product.Id,
                    StoreId = store.Id,
                    SupplierId = supplier.Id,
                    UserId = manager.Id,
                    TransactionType = InventoryTransactionType.Purchase,
                    Quantity = initialStock,
                    StockBefore = 0,
                    StockAfter = initialStock,
                    UnitCost = product.Cost,
                    TotalCost = product.Cost * initialStock,
                    TransactionDate = DateTime.UtcNow.AddDays(-30),
                    ReferenceNumber = $"PO{DateTime.Now:yyyyMMdd}{transactions.Count + 1:D4}",
                    Notes = "Initial stock receipt"
                });

                // Weekly restocks
                for (int week = 3; week >= 1; week--)
                {
                    if (_random.Next(100) < 70) // 70% chance of restock each week
                    {
                        var restockQty = _random.Next(20, 100);
                        transactions.Add(new InventoryTransaction
                        {
                            ProductId = product.Id,
                            StoreId = store.Id,
                            SupplierId = supplier.Id,
                            UserId = manager.Id,
                            TransactionType = InventoryTransactionType.Purchase,
                            Quantity = restockQty,
                            StockBefore = initialStock,
                            StockAfter = initialStock + restockQty,
                            UnitCost = product.Cost,
                            TotalCost = product.Cost * restockQty,
                            TransactionDate = DateTime.UtcNow.AddDays(-week * 7),
                            ReferenceNumber = $"PO{DateTime.Now:yyyyMMdd}{transactions.Count + 1:D4}",
                            Notes = "Weekly restock"
                        });
                    }
                }

                // Random adjustments and wastage
                if (_random.Next(100) < 30) // 30% chance of adjustment
                {
                    var adjustmentQty = _random.Next(-10, 10);
                    if (adjustmentQty != 0)
                    {
                        transactions.Add(new InventoryTransaction
                        {
                            ProductId = product.Id,
                            StoreId = store.Id,
                            UserId = manager.Id,
                            TransactionType = adjustmentQty > 0 ? InventoryTransactionType.Adjustment : InventoryTransactionType.Damage,
                            Quantity = Math.Abs(adjustmentQty),
                            StockBefore = product.StockQuantity,
                            StockAfter = product.StockQuantity + adjustmentQty,
                            TransactionDate = DateTime.UtcNow.AddDays(-_random.Next(1, 15)),
                            Notes = adjustmentQty > 0 ? "Stock count adjustment - found extra" : "Damaged/expired goods"
                        });
                    }
                }

                // Stock transfers between stores
                if (stores.Count > 1 && _random.Next(100) < 20) // 20% chance of transfer
                {
                    var targetStore = stores.FirstOrDefault(s => s.Id != store.Id);
                    if (targetStore != null)
                    {
                        var transferQty = _random.Next(5, 30);
                        transactions.Add(new InventoryTransaction
                        {
                            ProductId = product.Id,
                            StoreId = store.Id,
                            UserId = manager.Id,
                            TransactionType = InventoryTransactionType.Transfer,
                            Quantity = -transferQty,
                            StockBefore = product.StockQuantity,
                            StockAfter = product.StockQuantity - transferQty,
                            TransactionDate = DateTime.UtcNow.AddDays(-_random.Next(1, 10)),
                            ReferenceNumber = $"TR{DateTime.Now:yyyyMMdd}{transactions.Count + 1:D4}",
                            Notes = $"Transfer to {targetStore.Name}"
                        });
                    }
                }
            }
        }

        // Add sales transactions from orders
        var orderItems = await _context.OrderItems
            .Include(oi => oi.Order)
            .Where(oi => !oi.IsVoided)
            .Take(500) // Limit for performance
            .ToListAsync();

        foreach (var item in orderItems)
        {
            var product = await _context.Products.FindAsync(item.ProductId);
            if (product != null && product.TrackInventory)
            {
                transactions.Add(new InventoryTransaction
                {
                    ProductId = item.ProductId,
                    StoreId = item.Order.StoreId,
                    UserId = item.Order.UserId,
                    OrderId = item.OrderId,
                    TransactionType = InventoryTransactionType.Sale,
                    Quantity = -item.Quantity,
                    StockBefore = product.StockQuantity + item.Quantity,
                    StockAfter = product.StockQuantity,
                    TransactionDate = item.Order.OrderDate,
                    ReferenceNumber = item.Order.OrderNumber,
                    Notes = "Sale transaction"
                });
            }
        }

        await _context.InventoryTransactions.AddRangeAsync(transactions);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Seeded {Count} inventory transactions", transactions.Count);
    }

    private async Task SeedCustomerUsersAsync()
    {
        // Only seed customer users if they don't exist
        if (await _context.Users.AnyAsync(u => u.Role == UserRole.Customer)) return;

        // Get some customers to create user accounts for
        var customers = await _context.Customers
            .Where(c => c.Email != null && c.Email != "walkin@system.local")
            .Take(10) // Create accounts for first 10 customers
            .ToListAsync();

        if (!customers.Any())
        {
            _logger.LogWarning("No customers available to create user accounts");
            return;
        }

        var customerUsers = new List<User>();

        // Create a test customer account for easy mobile app testing with PIN
        var testCustomer = customers.First();
        customerUsers.Add(new User
        {
            Username = "customer",
            Email = "customer@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Customer123!"),
            FirstName = "Test",
            LastName = "Customer",
            Role = UserRole.Customer,
            IsActive = true,
            CustomerId = testCustomer.Id,
            Phone = testCustomer.Phone ?? "+61 400 000 100",
            Pin = "1234", // Easy test PIN for customers
            StoreId = null // Customers can order from any store
        });

        // Create user accounts for remaining customers with PINs
        for (int i = 1; i < customers.Count; i++)
        {
            var customer = customers[i];
            var username = GenerateUsernameFromEmail(customer.Email!);
            
            customerUsers.Add(new User
            {
                Username = username,
                Email = customer.Email!,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Customer123!"), // Default password
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                Role = UserRole.Customer,
                IsActive = true,
                CustomerId = customer.Id,
                Phone = customer.Phone,
                Pin = $"{3000 + i:D4}", // Customer PINs start from 3001
                StoreId = null // Customers can order from any store
            });
        }

        await _context.Users.AddRangeAsync(customerUsers);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Seeded {Count} customer user accounts with PINs", customerUsers.Count);
    }

    private string GenerateUsernameFromEmail(string email)
    {
        // Extract username part before @ and make it unique
        var username = email.Split('@')[0].ToLower().Replace(".", "");
        return username;
    }

    // Helper methods
    private string GetFirstName(int index)
    {
        string[] names = { "John", "Jane", "Michael", "Sarah", "David", "Emma", "James", "Lisa", "Robert", "Mary",
                          "William", "Patricia", "Richard", "Jennifer", "Thomas", "Linda", "Charles", "Elizabeth" };
        return names[index % names.Length];
    }

    private string GetLastName(int index)
    {
        string[] names = { "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis",
                          "Rodriguez", "Martinez", "Anderson", "Taylor", "Wilson", "Moore", "Jackson" };
        return names[index % names.Length];
    }

    private string GetStreetName()
    {
        string[] streets = { "Main", "High", "Park", "Church", "Victoria", "George", "King", "Queen", "Elizabeth", "Market" };
        return streets[_random.Next(streets.Length)];
    }

    private string GetStreetType()
    {
        string[] types = { "Street", "Road", "Avenue", "Boulevard", "Lane", "Drive", "Court", "Place" };
        return types[_random.Next(types.Length)];
    }

    private string GetCity(int index)
    {
        string[] cities = { "Sydney", "Melbourne", "Brisbane", "Perth", "Adelaide" };
        return cities[index % cities.Length];
    }

    private string GetState(int index)
    {
        string[] states = { "NSW", "VIC", "QLD", "WA", "SA" };
        return states[index % states.Length];
    }

    private string GetRandomCardType()
    {
        string[] cardTypes = { "Visa", "MasterCard", "Amex", "Debit" };
        return cardTypes[_random.Next(cardTypes.Length)];
    }

    private int GetRandomStockQuantity()
    {
        // Create realistic stock distribution:
        // 15% - Critical low stock (0-5 units)
        // 15% - Low stock (6-15 units)
        // 25% - Medium stock (16-50 units)
        // 30% - Good stock (51-100 units)
        // 15% - High stock (101-200 units)
        
        var chance = _random.Next(100);
        
        if (chance < 15) // 15% critical low stock
        {
            return _random.Next(0, 6); // 0, 1, 2, 3, 4, 5
        }
        else if (chance < 30) // 15% low stock
        {
            return _random.Next(6, 16); // 6-15
        }
        else if (chance < 55) // 25% medium stock
        {
            return _random.Next(16, 51); // 16-50
        }
        else if (chance < 85) // 30% good stock
        {
            return _random.Next(51, 101); // 51-100
        }
        else // 15% high stock
        {
            return _random.Next(101, 201); // 101-200
        }
    }

    private string GetImageUrlForProduct(string categorySlug, string subcategorySlug, string productName)
    {
        // Try to match by subcategory first
        var key = subcategorySlug.ToLower();
        
        // Map common subcategories to image categories
        if (key.Contains("cookie") || key.Contains("biscuit"))
            key = "cookies";
        else if (key.Contains("bread") || key.Contains("baguette") || key.Contains("sourdough") || key.Contains("loaf"))
            key = "breads";
        else if (key.Contains("cake") || key.Contains("gateau"))
            key = "cakes";
        else if (key.Contains("pastry") || key.Contains("pastries") || key.Contains("croissant") || key.Contains("danish"))
            key = "pastries";
        else if (key.Contains("coffee") || key.Contains("tea") || key.Contains("beverage") || key.Contains("drink") || key.Contains("juice"))
            key = "beverages";
        else
            key = categorySlug.ToLower();

        // Get images for the category or use default
        var imageList = _imageUrlsByCategory.ContainsKey(key) 
            ? _imageUrlsByCategory[key] 
            : _imageUrlsByCategory["default"];

        // Return a random image from the list
        return imageList[_random.Next(imageList.Count)];
    }

    // Data classes for JSON
    private class CatalogData
    {
        public GstData? Gst { get; set; }
        public List<CategoryData> Categories { get; set; } = new();
    }

    private class GstData
    {
        public decimal Rate { get; set; }
    }

    private class CategoryData
    {
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public List<SubcategoryData> Subcategories { get; set; } = new();
    }

    private class SubcategoryData
    {
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public List<ProductData> Products { get; set; } = new();
    }

    private class ProductData
    {
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public decimal? PriceExGstAud { get; set; }
        public string? PackNotes { get; set; }
    }
}

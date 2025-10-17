using POS.Domain.Entities;
using POS.Domain.Enums;
using POS.Infrastructure.Data;

namespace POS.WebAPI.Tests.Helpers;

public static class TestDataSeeder
{
    public static void SeedTestData(POSDbContext context)
    {
        // Seed Stores
        var store1 = new Store
        {
            Id = 1,
            Name = "Test Store 1",
            Code = "TS001",
            Address = "123 Test St",
            City = "Sydney",
            State = "NSW",
            PostalCode = "2000",
            Country = "Australia",
            Phone = "0412345678",
            Email = "store1@test.com",
            TaxRate = 0.10m,
            Currency = "AUD",
            IsActive = true,
            OpeningTime = new TimeOnly(9, 0),
            ClosingTime = new TimeOnly(17, 0)
        };

        var store2 = new Store
        {
            Id = 2,
            Name = "Test Store 2",
            Code = "TS002",
            Address = "456 Demo Ave",
            City = "Melbourne",
            State = "VIC",
            PostalCode = "3000",
            Country = "Australia",
            Phone = "0498765432",
            Email = "store2@test.com",
            TaxRate = 0.10m,
            Currency = "AUD",
            IsActive = true
        };

        context.Stores.AddRange(store1, store2);

        // Seed Users
        var adminUser = new User
        {
            Id = 1,
            Username = "testadmin",
            Email = "admin@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test123!"),
            FirstName = "Test",
            LastName = "Admin",
            Role = UserRole.Admin,
            IsActive = true,
            StoreId = 1,
            Pin = "9999"
        };

        var cashierUser = new User
        {
            Id = 2,
            Username = "testcashier",
            Email = "cashier@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test123!"),
            FirstName = "Test",
            LastName = "Cashier",
            Role = UserRole.Cashier,
            IsActive = true,
            StoreId = 1,
            Pin = "1111"
        };

        var customerUser = new User
        {
            Id = 3,
            Username = "testcustomer",
            Email = "customer@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test123!"),
            FirstName = "Test",
            LastName = "Customer",
            Role = UserRole.Customer,
            IsActive = true,
            Pin = "5555",
            Phone = "0411111111"
        };

        context.Users.AddRange(adminUser, cashierUser, customerUser);

        // Seed Categories
        var category1 = new Category
        {
            Id = 1,
            Name = "Cookies",
            Slug = "cookies",
            Description = "Delicious cookies",
            IsActive = true,
            DisplayOrder = 1
        };

        var category2 = new Category
        {
            Id = 2,
            Name = "Cakes",
            Slug = "cakes",
            Description = "Tasty cakes",
            IsActive = true,
            DisplayOrder = 2
        };

        context.Categories.AddRange(category1, category2);

        // Seed Subcategories
        var subcategory1 = new Subcategory
        {
            Id = 1,
            Name = "Chocolate Cookies",
            Slug = "chocolate-cookies",
            CategoryId = 1,
            IsActive = true,
            DisplayOrder = 1
        };

        var subcategory2 = new Subcategory
        {
            Id = 2,
            Name = "Vanilla Cookies",
            Slug = "vanilla-cookies",
            CategoryId = 1,
            IsActive = true,
            DisplayOrder = 2
        };

        var subcategory3 = new Subcategory
        {
            Id = 3,
            Name = "Birthday Cakes",
            Slug = "birthday-cakes",
            CategoryId = 2,
            IsActive = true,
            DisplayOrder = 1
        };

        context.Subcategories.AddRange(subcategory1, subcategory2, subcategory3);

        // Seed Suppliers
        var supplier1 = new Supplier
        {
            Id = 1,
            Name = "Test Supplier 1",
            ContactPerson = "John Doe",
            Email = "supplier1@test.com",
            Phone = "0412345678",
            IsActive = true
        };

        context.Suppliers.Add(supplier1);

        // Seed Products
        var product1 = new Product
        {
            Id = 1,
            Name = "Chocolate Chip Cookie",
            Slug = "chocolate-chip-cookie",
            SKU = "CCC001",
            Barcode = "1234567890123",
            Description = "Classic chocolate chip cookie",
            PriceExGst = 3.64m,
            GstAmount = 0.36m,
            PriceIncGst = 4.00m,
            Cost = 2.00m,
            PackSize = 1,
            ImageUrl = "/images/chocolate-chip.jpg",
            IsActive = true,
            TrackInventory = true,
            StockQuantity = 100,
            LowStockThreshold = 10,
            SubcategoryId = 1,
            SupplierId = 1,
            DisplayOrder = 1
        };

        var product2 = new Product
        {
            Id = 2,
            Name = "Vanilla Sugar Cookie",
            Slug = "vanilla-sugar-cookie",
            SKU = "VSC001",
            Barcode = "1234567890124",
            Description = "Sweet vanilla sugar cookie",
            PriceExGst = 2.73m,
            GstAmount = 0.27m,
            PriceIncGst = 3.00m,
            Cost = 1.50m,
            PackSize = 1,
            ImageUrl = "/images/vanilla-sugar.jpg",
            IsActive = true,
            TrackInventory = true,
            StockQuantity = 150,
            LowStockThreshold = 15,
            SubcategoryId = 2,
            SupplierId = 1,
            DisplayOrder = 2
        };

        var product3 = new Product
        {
            Id = 3,
            Name = "Birthday Cake - Small",
            Slug = "birthday-cake-small",
            SKU = "BCS001",
            Barcode = "1234567890125",
            Description = "Small birthday cake",
            PriceExGst = 27.27m,
            GstAmount = 2.73m,
            PriceIncGst = 30.00m,
            Cost = 15.00m,
            PackSize = 1,
            ImageUrl = "/images/birthday-cake-small.jpg",
            IsActive = true,
            TrackInventory = true,
            StockQuantity = 20,
            LowStockThreshold = 5,
            SubcategoryId = 3,
            SupplierId = 1,
            DisplayOrder = 1
        };

        context.Products.AddRange(product1, product2, product3);

        // Seed Customer
        var customer1 = new Customer
        {
            Id = 1,
            FirstName = "Jane",
            LastName = "Smith",
            Email = "jane.smith@example.com",
            Phone = "0411222333",
            Address = "789 Customer Rd",
            City = "Sydney",
            State = "NSW",
            PostalCode = "2000",
            Country = "Australia",
            DateOfBirth = new DateTime(1990, 5, 15),
            IsActive = true,
            LoyaltyPoints = 100
        };

        context.Customers.Add(customer1);

        // Save all seeded data
        context.SaveChanges();
    }

    /// <summary>
    /// Seeds a simple product for quick tests
    /// </summary>
    public static Product CreateTestProduct(POSDbContext context, long id = 99, string name = "Test Product")
    {
        // Ensure we have a subcategory
        var subcategory = context.Subcategories.FirstOrDefault();
        if (subcategory == null)
        {
            subcategory = new Subcategory
            {
                Id = 99,
                Name = "Test Subcategory",
                Slug = "test-subcategory",
                CategoryId = 1,
                IsActive = true
            };
            context.Subcategories.Add(subcategory);
            context.SaveChanges();
        }

        var product = new Product
        {
            Id = id,
            Name = name,
            Slug = name.ToLower().Replace(" ", "-"),
            SKU = $"TEST{id:000}",
            Barcode = $"999999999{id:0000}",
            PriceExGst = 9.09m,
            GstAmount = 0.91m,
            PriceIncGst = 10.00m,
            Cost = 5.00m,
            IsActive = true,
            TrackInventory = true,
            StockQuantity = 50,
            LowStockThreshold = 10,
            SubcategoryId = subcategory.Id
        };

        context.Products.Add(product);
        context.SaveChanges();

        return product;
    }

    /// <summary>
    /// Seeds a test order with items
    /// </summary>
    public static Order CreateTestOrder(POSDbContext context, long userId, long storeId, long? customerId = null)
    {
        var orderNumber = $"TEST{DateTime.Now:yyyyMMddHHmmss}";
        
        var order = new Order
        {
            OrderNumber = orderNumber,
            OrderDate = DateTime.Now,
            Status = OrderStatus.Pending,
            OrderType = OrderType.DineIn,
            UserId = userId,
            StoreId = storeId,
            CustomerId = customerId,
            SubTotal = 0,
            TaxAmount = 0,
            TotalAmount = 0,
            PaidAmount = 0,
            ChangeAmount = 0
        };

        context.Orders.Add(order);
        context.SaveChanges();

        return order;
    }
}
